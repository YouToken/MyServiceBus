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

    public class TopicQueue 
    {
        private class ExecutionMonitoring
        {
            public long ExecutedAmount { get; private set; }
            public TimeSpan ExecutionDuration { get; private set; }
            
            internal void UpdateLastAmount(int amount, TimeSpan executionDuration)
            {
                ExecutedAmount += amount;
                ExecutionDuration += executionDuration;
            }

            internal void Reset()
            {
                ExecutedAmount = 0;
                ExecutionDuration = TimeSpan.Zero;
            }
        }

        private readonly QueueWithIntervals _queue;

        private readonly object _topicLock = new();

        private readonly Dictionary<long, int> _attempts = new();

        private readonly MetricList<int> _executionDuration = new ();

        private readonly ExecutionMonitoring _executionMonitoring = new ();
        
        public QueueSubscribersList SubscribersList { get; }

        public TopicQueue(MyTopic topic, string queueId, bool deleteOnDisconnect, IEnumerable<IQueueIndexRange> ranges)
        {
            Topic = topic;
            QueueId = queueId;
            DeleteOnDisconnect = deleteOnDisconnect;
            _queue = new QueueWithIntervals(ranges);
            SubscribersList = new QueueSubscribersList(this, _topicLock);
        }

        public TopicQueue(MyTopic topic, string queueId, bool deleteOnDisconnect, long messageId)
        {
            Topic = topic;
            QueueId = queueId;
            DeleteOnDisconnect = deleteOnDisconnect;
            _queue = new QueueWithIntervals(messageId);
            SubscribersList = new QueueSubscribersList(this, _topicLock);
        }

        public MyTopic Topic { get; }
        public string QueueId { get; }
        public bool DeleteOnDisconnect { get; }
        public IReadOnlyList<IQueueIndexRange> GetReadyQueueSnapshot()
        {
            lock (_topicLock)
            {
                return _queue.GetSnapshot();
            }
        }

        public IReadOnlyList<IQueueIndexRange> GetLeasedQueueSnapshot(IMyServiceBusSession session)
        {
            lock (_topicLock)
            {
                var subscriber = SubscribersList.TryGetSubscriber(session);
                return subscriber == null ? Array.Empty<IQueueIndexRange>() : subscriber.LeasedQueue.GetSnapshot();
            }
        }
        

        public IEnumerable<(long messageId, int attemptNo)> DequeNextMessage()
        {
            lock (_topicLock)
            {
                var result = _queue.Dequeue();

                while (result>-1)
                {
                    var attemptNo = 1;
                    _attempts.TryGetValue(attemptNo, out attemptNo);

                   yield return (result, attemptNo);
                   
                   result = _queue.Dequeue();
                }
            }
   
        }



        private void DisposeNotDeliveredMessages(
            IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> messages, int incrementAttemptNo)
        {
            foreach (var (message, attemptNo) in messages)
            {
                // Make it not through Message By Messages - but though merge of to IntervalQueues to increase performance
                _queue.Enqueue(message.MessageId);

                if (!_attempts.TryAdd(message.MessageId, attemptNo))
                    _attempts[message.MessageId] = attemptNo + incrementAttemptNo;
            }
        }


        public void CancelDelivery(TheQueueSubscriber leasedSubscriber)
        {
            lock (_topicLock)
            {
                DisposeNotDeliveredMessages(leasedSubscriber.MessagesCollector, 0);
                leasedSubscriber.SetToUnLeased();
            }
        }

        public void EnqueueMessages(IEnumerable<MessageContentGrpcModel> messages)
        {
            lock (_topicLock)
            {
                foreach (var message in messages)
                    _queue.Enqueue(message.MessageId);       
            }
        }


        public void ConfirmDelivery(TheQueueSubscriber subscriber, TimeSpan executionDuration)
        {
            lock (_topicLock)
            {
                _executionMonitoring.UpdateLastAmount(subscriber.MessagesOnDelivery.Count, executionDuration);
                subscriber.SetToUnLeased(); 
            }
        }

        public void ConfirmNotDelivery(TheQueueSubscriber subscriber, TimeSpan executionDuration)
        {
            lock (_topicLock)
            {
                _executionMonitoring.UpdateLastAmount(subscriber.MessagesOnDelivery.Count, executionDuration);

                DisposeNotDeliveredMessages(subscriber.MessagesOnDelivery, 1);
                subscriber.SetToUnLeased();
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
            lock (_topicLock)
            {
                return new QueueSnapshot
                {
                    QueueId = QueueId,
                    RangesData = _queue.GetSnapshot()
                };
                
            }
        }

        public long GetMinId()
        {
            lock (_topicLock)
            {
                var minFromLeasedQueue = this.GetMinMessageId();
                var minFromQueue = _queue.GetMinId();
                
                if (minFromLeasedQueue<0)
                    return _queue.GetMinId();

                return minFromQueue < minFromLeasedQueue ? minFromQueue : minFromLeasedQueue;
            }
        }


        public async ValueTask<bool> DisconnectedAsync(IMyServiceBusSession session)
        {
            
            var theSubscriber = SubscribersList.Unsubscribe(session);
            
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
            }


            return result.ToString();

        }

        public void SetInterval(long minId, long maxId)
        {
            lock (_topicLock)
            {
                var subscribersCount = SubscribersList.GetCount();

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
                if (_executionMonitoring.ExecutedAmount == 0)
                    return;

                var amount = _executionMonitoring.ExecutionDuration / _executionMonitoring.ExecutedAmount;
                _executionDuration.PutData((int)(amount.TotalMilliseconds * 1000));

                _executionMonitoring.Reset();
            }
        }
        
    }
}