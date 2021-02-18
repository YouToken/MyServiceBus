using System;
using System.Collections.Generic;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.Topics
{

    public class MyTopic
    {
        private readonly IMetricCollector _metricCollector;

        private readonly TopicQueueList _topicQueueList; 


        public MessageIdGenerator MessageId { get; }
        
        public MessagesContentCache MessagesContentCache { get; }
        
        public override string ToString()
        {
            return "Topic: " + TopicId;
        }

        public MyTopic(string id, long startMessageId, IMetricCollector metricCollector)
        {
            _metricCollector = metricCollector;
            TopicId = id;
            MessageId = new MessageIdGenerator(startMessageId);
            _topicQueueList = new TopicQueueList();
            MessagesContentCache = new MessagesContentCache(id);
        }
        
        public string TopicId { get; }

        private int _messagePerSecond;
        public int MessagesPerSecond { get; private set; }
        
        private int _requestsPerSecond;
        public int RequestsPerSecond { get; private set; }

        internal void Timer()
        {
            MessagesPerSecond = _messagePerSecond;
            _messagePerSecond = 0;

            RequestsPerSecond = _requestsPerSecond;
            _requestsPerSecond = 0;
        }

        public IReadOnlyList<TopicQueue> GetQueues()
        {
            return _topicQueueList.GetQueues();
        }

        public long MessagesCount => _topicQueueList.GetMessagesCount();

        public void DeleteQueue(string queueName)
        {
            _topicQueueList.DeleteQueue(queueName);
        }

        public TopicQueue CreateQueueIfNotExists(string queueName, bool deleteOnDisconnect)
        {
            return _topicQueueList.CreateQueueIfNotExists(this, queueName, deleteOnDisconnect, MessageId.Value);
        }

        public IReadOnlyList<MessageContentGrpcModel> Publish(IEnumerable<byte[]> messages, DateTime now)
        {
            _requestsPerSecond++;

            var newMessages = new List<MessageContentGrpcModel>();

            MessageId.Lock(generator =>
            {
                foreach (var message in messages)
                {

                    var newMessage = new MessageContentGrpcModel
                    {
                        MessageId = generator.GetNextMessageId(),
                        Created = now,
                        Data = message
                    };

                    newMessages.Add(newMessage);

                    _messagePerSecond++;
                }
            });
            
            _metricCollector.TopicQueueSize(TopicId, _topicQueueList.GetMessagesCount());

            MessagesContentCache.AddMessages(newMessages);
            return newMessages;
        }

        public TopicQueue ConfirmDelivery(string queueName, long confirmationId, bool ok)
        {
            var queue = _topicQueueList.GetQueue(queueName);

            var subscriber = queue.QueueSubscribersList.TryGetSubscriber(confirmationId);

            if (subscriber == null)
                return queue;
            
            queue.LockAndGetWriteAccess(writeAccess =>
            {
                if (ok)
                    writeAccess.ConfirmDelivery(subscriber);
                else
                    writeAccess.ConfirmNotDelivery(subscriber);  
            });

            _topicQueueList.CalcMinMessageId();
            _metricCollector.TopicQueueSize(TopicId, _topicQueueList.GetMessagesCount());

            return queue;
        }

        public long GetQueueMessagesCount(string queueName)
        {
            return _topicQueueList.GetQueueMessagesCount(queueName);
        }

        public ITopicPersistence GetQueuesSnapshot()
        {
            return new TopicPersistence
            {
                TopicId = TopicId,
                MessageId = MessageId.Value,
                QueueSnapshots = _topicQueueList.GetQueuesSnapshot()
            };
        }

        public long GetMinMessageId()
        {
            return _topicQueueList.MinMessageId;
        }

        public TopicQueue GetQueue(string queueId)
        {
            return _topicQueueList.GetQueue(queueId);
        }

        public void Init(IReadOnlyList<IQueueSnapshot> queueSnapshots)
        {
            foreach (var queueSnapshot in queueSnapshots)
            {
                Console.WriteLine($"Restoring Queue: {TopicId}.{queueSnapshot.QueueId} with Ranges:");
                foreach (var indexRange in queueSnapshot.Ranges)
                {
                    Console.WriteLine(indexRange.FromId + "-" + indexRange.ToId);
                }

                _topicQueueList.Init(this, queueSnapshot.QueueId, false, queueSnapshot.Ranges);
            }
        }

        public void SetQueueMessageId(string queueId, long messageId)
        {

            var queue = _topicQueueList.GetQueue(queueId);

            if (queue == null)
                throw new Exception($"Queue {queueId} is not found");

            if (messageId < 0)
                throw new Exception($"MessageId must be above 0");

            if (messageId > MessageId.Value)
                throw new Exception($"MessageId can not be greater than the Topic messageId which is {MessageId} now");

            queue.SetInterval(messageId, MessageId.Value);
        }
        
    }


}