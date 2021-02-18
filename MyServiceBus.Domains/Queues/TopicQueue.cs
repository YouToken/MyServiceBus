using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MyServiceBus.Abstractions.QueueIndex;
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

        void ConfirmDelivery(long confirmationId, long topicMessageId);

        void ConfirmNotDelivery(long confirmationId, long topicMessageId);

        void ConfirmNotDelivered(IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> messages,
            int incrementAttemptNo);
    }

    public class TopicQueue : ITopicQueueWriteAccess
    {

        private readonly QueueWithIntervals _queue;

        private readonly QueueWithIntervals _leasedQueue;

        private readonly object _lockObject;

        private readonly Dictionary<long, int> _attempts = new();

        private long _setMinMessageId = -1;
        
        public TopicQueue(MyTopic topic, string queueId, bool deleteOnDisconnect, IEnumerable<IQueueIndexRange> ranges,
            object lockObject)
        {
            _lockObject = lockObject;
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
            QueueSubscribersList = new QueueSubscribersList(this, lockObject);
        }

        public TopicQueue(MyTopic topic, string queueId, bool deleteOnDisconnect, long messageId, object lockObject)
        {
            _lockObject = lockObject;
            Topic = topic;
            QueueId = queueId;
            DeleteOnDisconnect = deleteOnDisconnect;
            _queue = new QueueWithIntervals(messageId);
            _leasedQueue = new QueueWithIntervals(messageId);
            QueueSubscribersList = new QueueSubscribersList(this, lockObject);
        }

        public MyTopic Topic { get; }
        public string QueueId { get; }

        public bool DeleteOnDisconnect { get; }

        public (IReadOnlyList<IQueueIndexRange> queues, IReadOnlyList<IQueueIndexRange> leased) GetQueueIntervals()
        {
            lock (_lockObject)
            {
                return (_queue.GetSnapshot(), _leasedQueue.GetSnapshot());
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

        public ValueTask LockAndGetWriteAccessAsync(Func<ITopicQueueWriteAccess, ValueTask> callback)
        {
            lock (_lockObject)
            {
                return callback(this);
            }

        }

        public void LockAndGetWriteAccess(Action<ITopicQueueWriteAccess> callback)
        {
            lock (_lockObject)
            {
                callback(this);
            }
        }
        
        private void NotDelivered(MessageContentGrpcModel message, int attemptNo)
        {
            _leasedQueue.Remove(message.MessageId);
            _queue.Enqueue(message.MessageId);

            if (!_attempts.TryAdd(message.MessageId, attemptNo))
                _attempts[message.MessageId] = attemptNo;
        }

        void ITopicQueueWriteAccess.ConfirmNotDelivered(
            IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> messages, int incrementAttemptNo)
        {
            foreach (var (message, attemptNo) in messages)
                NotDelivered(message, attemptNo + incrementAttemptNo);
        }


        void ITopicQueueWriteAccess.EnqueueMessages(IEnumerable<MessageContentGrpcModel> messages)
        {
            lock (_lockObject)
            {
                foreach (var message in messages)
                    _queue.Enqueue(message.MessageId);
            }
        }

        void ITopicQueueWriteAccess.ConfirmDelivery(long confirmationId, long topicMessageId)
        {

            lock (_lockObject)
            {
                var messagesDelivered = QueueSubscribersList.Delivered(confirmationId);

                if (messagesDelivered == null)
                    throw new Exception(
                        $"Can not find collector on delivery with confirmationId {confirmationId} for TopicId: {Topic} and QueueId: {QueueId}");

                foreach (var msgDelivered in messagesDelivered)
                {
                    _leasedQueue.Remove(msgDelivered.message.MessageId);
                }

                if (_setMinMessageId > -1)
                {
                    _queue.SetMinMessageId(_setMinMessageId, topicMessageId);
                    _setMinMessageId = -1;
                }

            }
        }

        void ITopicQueueWriteAccess.ConfirmNotDelivery(long confirmationId, long topicMessageId)
        {

            lock (_lockObject)
            {
                var messagesDelivered = QueueSubscribersList.Delivered(confirmationId);

                if (messagesDelivered == null)
                    throw new Exception(
                        $"Can not find collector on delivery with confirmationId {confirmationId} for TopicId: {Topic} and QueueId: {QueueId}");

                foreach (var msg in messagesDelivered)
                {
                    NotDelivered(msg.message, msg.attemptNo+1);
                }

                if (_setMinMessageId > -1)
                {
                    _queue.SetMinMessageId(_setMinMessageId, topicMessageId);
                    _setMinMessageId = -1;
                }

            }
        }

        public long GetLeasedMessagesCount()
        {
            lock (_lockObject)
            {
                return _leasedQueue.GetMessagesCount();
            }
        }

        public long GetMessagesCount()
        {
            lock (_lockObject)
            {
                return _queue.GetMessagesCount();
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

            lock (_lockObject)
            {
                var minFromQueue = _queue.GetMinId();
                var minFromLeasedQueue = _leasedQueue.GetMinId();

                return minFromQueue < minFromLeasedQueue ? minFromQueue : minFromLeasedQueue;

            }
        }

        public QueueSubscribersList QueueSubscribersList { get; }

        public async ValueTask<bool> DisconnectedAsync(IQueueSubscriber queueSubscriber)
        {

            var theSubscriber = QueueSubscribersList.Unsubscribe(queueSubscriber);

            if (theSubscriber == null)
                return false;

            if (theSubscriber.Status == SubscriberStatus.Leased)
            {
                Console.WriteLine($"Got subscriber {theSubscriber.QueueSubscriber.SubscriberId} in Leased Status");

                while (theSubscriber.Status == SubscriberStatus.Leased)
                    await Task.Delay(100);

            }

            if (theSubscriber.Status == SubscriberStatus.OnDelivery)
            {
                LockAndGetWriteAccess(writer =>
                {
                    writer.ConfirmNotDelivered(theSubscriber.MessagesOnDelivery, 0);
                });
            }


            return true;
        }

        public override string ToString()
        {

            var result = new StringBuilder();


            lock (_lockObject)
            {
                result.Append("Queue:[");
                if (_queue.GetMessagesCount() == 0)
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
                if (_leasedQueue.GetMessagesCount() == 0)
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
            if (_leasedQueue.Count == 0)
                _queue.SetMinMessageId(minId, maxId);
            else
                _setMinMessageId = minId;
        }
    }
}