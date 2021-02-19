using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Metrics;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.Queues
{
    public interface ITopicQueueWriteAccess
    {
        (long messageId, int attemptNo) DequeAndLease();
        void EnqueueMessages(IEnumerable<MessageContentGrpcModel> messages);

        void ConfirmDelivery(TheQueueSubscriber subscriber, TimeSpan executionDuration);

        void ConfirmNotDelivery(TheQueueSubscriber subscriber, TimeSpan executionDuration);

        void CancelDelivery(TheQueueSubscriber leasedSubscriber);

    }

    public class TopicQueue : ITopicQueueWriteAccess
    {

        private readonly QueueWithIntervals _queue;

        private readonly QueueWithIntervals _leasedQueue;

        private readonly object _topicLock = new();

        private readonly Dictionary<long, int> _attempts = new();

        private readonly MetricList<int> _executionDuration = new ();
        
        private long _executedAmount = 0;
        private TimeSpan _executionDuraton = default;

        public TopicQueue(MyTopic topic, string queueId, bool deleteOnDisconnect, IEnumerable<IQueueIndexRange> ranges)
        {
            Topic = topic;
            QueueId = queueId;
            DeleteOnDisconnect = deleteOnDisconnect;

            long messageId = 0;

            foreach (var range in ranges)
            {
                messageId = range.ToId;
                _queue = new QueueWithIntervals(range.FromId, range.ToId);
            }

            _leasedQueue = new QueueWithIntervals(messageId);
            QueueSubscribersList = new QueueSubscribersList(this, _topicLock);
        }

        public TopicQueue(MyTopic topic, string queueId, bool deleteOnDisconnect, long messageId)
        {
            Topic = topic;
            QueueId = queueId;
            DeleteOnDisconnect = deleteOnDisconnect;
            _queue = new QueueWithIntervals(messageId);
            _leasedQueue = new QueueWithIntervals(messageId);
            QueueSubscribersList = new QueueSubscribersList(this, _topicLock);
        }

        public MyTopic Topic { get; }
        public string QueueId { get; }

        public bool DeleteOnDisconnect { get; }

        public (IReadOnlyList<IQueueIndexRange> queues, IReadOnlyList<IQueueIndexRange> leased) GetQueueIntervals()
        {
            lock (_topicLock)
            {
                return (_queue.GetSnapshot(), _leasedQueue.GetSnapshot());
            }
        }

        public void LockAndGetWriteAccess(Action<ITopicQueueWriteAccess> callback)
        {
            lock (_topicLock)
            {
                callback(this);
            }
        }
        
        public T LockAndGetWriteAccess<T>(Func<ITopicQueueWriteAccess, T> callback)
        {
            lock (_topicLock)
            {
                return callback(this);
            }
        }


        (long messageId, int attemptNo) ITopicQueueWriteAccess.DequeAndLease()
        {
            var result = _queue.Dequeue();
            if (result >= 0)
                _leasedQueue.Enqueue(result);

            var attemptNo = 1;
            _attempts.TryGetValue(attemptNo, out attemptNo);

            return (result, attemptNo);
        }
        
        private void NotDelivered(MessageContentGrpcModel message, int attemptNo)
        {
            _queue.Enqueue(message.MessageId);

            if (!_attempts.TryAdd(message.MessageId, attemptNo))
                _attempts[message.MessageId] = attemptNo;
        }


        private void DisposeNotDeliveredMessages(
            IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> messages, int incrementAttemptNo)
        {
            foreach (var (message, attemptNo) in messages)
            {
                _leasedQueue.Remove(message.MessageId);
                NotDelivered(message, attemptNo + incrementAttemptNo);
            }
        }
        
        
        void ITopicQueueWriteAccess. CancelDelivery(TheQueueSubscriber leasedSubscriber)
        {
            DisposeNotDeliveredMessages(leasedSubscriber.MessagesCollector, 0);
            leasedSubscriber.SetToUnLeased();
        }

        void ITopicQueueWriteAccess.EnqueueMessages(IEnumerable<MessageContentGrpcModel> messages)
        {
            foreach (var message in messages)
                _queue.Enqueue(message.MessageId);
        }



        private void UpdateLastAmount(int amount, TimeSpan executionDuration)
        {
            _executedAmount += amount;
            _executionDuraton += executionDuration;

        }

        void ITopicQueueWriteAccess.ConfirmDelivery(TheQueueSubscriber subscriber, TimeSpan executionDuration)
        {
            UpdateLastAmount(subscriber.MessagesOnDelivery.Count, executionDuration);


            foreach (var (message, _) in subscriber.MessagesOnDelivery)
                _leasedQueue.Remove(message.MessageId);
            
            subscriber.SetToUnLeased();
        }

        void ITopicQueueWriteAccess.ConfirmNotDelivery(TheQueueSubscriber subscriber, TimeSpan executionDuration)
        {
            UpdateLastAmount(subscriber.MessagesOnDelivery.Count, executionDuration);
            
            DisposeNotDeliveredMessages(subscriber.MessagesOnDelivery, 1);
            subscriber.SetToUnLeased();
        }

        public long GetLeasedMessagesCount()
        {
            lock (_topicLock)
            {
                return _leasedQueue.Count;
            }
        }

        public long GetMessagesCount()
        {
            lock (_topicLock)
            {
                return _queue.Count;
            }
        }

        public IQueueSnapshot GetSnapshot()
        {
            return new QueueSnapshot
            {
                QueueId = QueueId,
                RangesData = _queue.GetSnapshot()
            };
        }


        public long GetMinId()
        {

            lock (_topicLock)
            {
                var minFromQueue = _queue.GetMinId();
                var minFromLeasedQueue = _leasedQueue.GetMinId();

                return minFromQueue < minFromLeasedQueue ? minFromQueue : minFromLeasedQueue;

            }
        }

        public QueueSubscribersList QueueSubscribersList { get; }


        public async ValueTask<bool> DisconnectedAsync(IMyServiceBusSession session)
        {
            
            var theSubscriber = QueueSubscribersList.Unsubscribe(session);
            
            if (theSubscriber == null)
                return false;


            while (theSubscriber.Status == SubscriberStatus.Leased)
                await Task.Delay(100);


            lock (_topicLock)
            {
                if (theSubscriber.MessagesOnDelivery.Count>0)
                    DisposeNotDeliveredMessages(theSubscriber.MessagesOnDelivery, 1);

                return true;
            }

        }

        public override string ToString()
        {

            var result = new StringBuilder();


            lock (_topicLock)
            {
                result.Append("Queue:[");
                if (_queue.Count == 0)
                {
                    result.Append("Empty");
                }
                else
                    foreach (var snapshot in _queue.GetSnapshot())
                    {
                        result.Append(snapshot.FromId + " - " + snapshot.ToId + ";");
                    }

                result.Append("]");

                result.Append("Leased:[");
                if (_leasedQueue.Count == 0)
                {
                    result.Append("Empty");
                }
                else
                    foreach (var snapshot in _leasedQueue.GetSnapshot())
                    {
                        result.Append(snapshot.FromId + " - " + snapshot.ToId + ";");
                    }

                result.Append("]");
            }


            return result.ToString();

        }

        public void SetInterval(long minId, long maxId)
        {
            lock (_topicLock)
            {
                var subscribersCount = QueueSubscribersList.GetCount();

                if (subscribersCount > 0)
                    throw new Exception(
                        $"Queue has: {subscribersCount}. You can rewind the queue only if it has 0 subscribers");

                _queue.SetMinMessageId(minId, maxId);
            }
        }

        public IReadOnlyList<int> GetExecutionDuration()
        {
            lock (_executionDuration)
            {
                return _executionDuration.GetItems();
            }
        }

        public void KickMetricsTimer()
        {
            lock (_executionDuration)
            {
                if (_executedAmount == 0)
                    return;
                
                var amount = _executionDuraton / _executedAmount;
                _executionDuration.PutData((int) (amount.TotalMilliseconds*1000));
                _executedAmount = 0;
                _executionDuraton = TimeSpan.Zero;
            }
            
        }
    }
}