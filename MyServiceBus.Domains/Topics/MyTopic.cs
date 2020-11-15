using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.Topics
{

    public class MyTopic
    {
        private readonly IMetricCollector _metricCollector;

        private readonly TopicQueues _topicQueues;


        private readonly object _lockObject = new object();
        
        public long MessageId { get; private set; }
        
        private long GetNextMessageId()
        {
            var result = MessageId;
            MessageId++;
            return result;
        }
        
        public override string ToString()
        {
            return "Topic: " + TopicId;
        }

        public MyTopic(string id, long startMessageId, IMetricCollector metricCollector)
        {
            _metricCollector = metricCollector;
            TopicId = id;
            MessageId = startMessageId;
            _topicQueues = new TopicQueues();
        }
        public string TopicId { get; }
        public int MaxMessagesInCache { get; private set; }

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
            return _topicQueues.GetQueues();
        }

        public long MessagesCount => _topicQueues.GetMessagesCount();

        public void DeleteQueue(string queueName)
        {
            lock (_lockObject)
            {
                _topicQueues.DeleteQueue(queueName);
            }
        }

        public TopicQueue CreateQueueIfNotExists(string queueName, bool deleteOnDisconnect)
        {
            lock (_lockObject)
            {
                return _topicQueues.CreateQueueIfNotExists(this, queueName, deleteOnDisconnect, MessageId, _lockObject);
            }
        }


        public IReadOnlyList<MessageContent> Publish(IEnumerable<byte[]> messages, DateTime now,  
            Action<IReadOnlyList<MessageContent>> callbackInsideLock)
        {

            _requestsPerSecond++;
            
            if (_topicQueues.QueueCount == 0)
            {
                _messagePerSecond += messages.Count();
                return Array.Empty<MessageContent>();
            }

            var messagesToPersist = new List<MessageContent>();

            lock (_lockObject)
            {
                foreach (var message in messages)
                {
                    var messageId = GetNextMessageId();

                    var newMessage = MessageContent.Create(messageId, message, now);

                    messagesToPersist.Add(newMessage);

                    _topicQueues.NewMessage(messageId);
                    _messagePerSecond++;
                }

                callbackInsideLock(messagesToPersist);
            }
            
            _metricCollector.TopicQueueSize(TopicId, _topicQueues.GetMessagesCount());

            return messagesToPersist;
        }

        public TopicQueue ConfirmDelivery(string queueName, long confirmationId, bool ok)
        {

            lock (_lockObject)
            {
                var queue = _topicQueues.GetQueue(queueName);
                
                if (ok)
                    queue.ConfirmDelivery(confirmationId);
                else
                    queue.ConfirmNotDelivery(confirmationId);
                
                _topicQueues.CalcMinMessageId();
                _metricCollector.TopicQueueSize(TopicId, _topicQueues.GetMessagesCount());

                return queue;
            }
        }

        public long GetQueueMessagesCount(string queueName)
        {
            lock (_lockObject)
            {
                return _topicQueues.GetQueueMessagesCount(queueName);
            }
        }


        public ITopicPersistence GetQueuesSnapshot()
        {
            lock (_lockObject)
            {
                return new TopicPersistence
                {
                    TopicId = TopicId,
                    MessageId = MessageId,
                    MaxMessagesInCache = MaxMessagesInCache,
                    QueueSnapshots = _topicQueues.GetQueuesSnapshot()
                };
            }
        }

        public long GetMinMessageId()
        {
            lock (_lockObject)
            {
                return _topicQueues.MinMessageId;
            }
        }

        public TopicQueue GetQueue(string queueId)
        {
            lock (_lockObject)
            {
                return _topicQueues.GetQueue(queueId);
            }
        }

        public void Init(IReadOnlyList<IQueueSnapshot> queueSnapshots)
        {
            lock (_lockObject)
            {
                foreach (var queueSnapshot in queueSnapshots)
                {
                    Console.WriteLine($"Restoring Queue: {TopicId}.{queueSnapshot.QueueId} with Ranges:");
                    foreach (var indexRange in queueSnapshot.Ranges)
                    {
                        Console.WriteLine(indexRange.FromId+"-"+indexRange.ToId);
                    }
                    _topicQueues.Init(this, queueSnapshot.QueueId, false, queueSnapshot.Ranges, _lockObject);
                }
            }
        }
    }


}