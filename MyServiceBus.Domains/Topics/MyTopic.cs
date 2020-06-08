using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCoreDecorators;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.Topics
{

    public class MyTopic : ITopicPersistence
    {
        private readonly Dictionary<string, TopicQueue> _topicQueues = new Dictionary<string, TopicQueue>();
        private IReadOnlyList<TopicQueue> _queuesAsReadOnlyList = Array.Empty<TopicQueue>();

        private int _queueCount;

        private readonly object _lockObject = new object();
        public override string ToString()
        {
            return "Topic: " + TopicId;
        }

        public MyTopic(string id, long startMessageId)
        {
            TopicId = id;
            MessageId = startMessageId;
        }
        public string TopicId { get; }
        public long MessageId { get; private set; }
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
            return _queuesAsReadOnlyList;
        }


        public long GetMessagesCount()
        {
            if (_queueCount == 0)
                return 0;
            
            return MessageId-GetMinMessageId();
        }

        public void DeleteQueue(string queueName)
        {
            lock (_lockObject)
            {
                if (!_topicQueues.ContainsKey(queueName))
                    return;

                _topicQueues.Remove(queueName);
                _queuesAsReadOnlyList = _topicQueues.Values.AsReadOnlyList();
                _queueCount = _topicQueues.Count;
            }
        }

        public TopicQueue CreateQueueIfNotExists(string queueName, bool deleteOnDisconnect)
        {
            lock (_lockObject)
            {
                if (_topicQueues.ContainsKey(queueName))
                    return _topicQueues[queueName];

                var queue = new TopicQueue(this, queueName, deleteOnDisconnect, MessageId, _lockObject);
                _topicQueues.Add(queueName, queue);

                _queueCount = _topicQueues.Count;
                _queuesAsReadOnlyList = _topicQueues.Values.AsReadOnlyList();

                return queue;
            }
        }

        private long GetNextMessageId()
        {
            var result = MessageId;
            MessageId++;
            return result;
        }


        public IReadOnlyList<MessageContent> Publish(IEnumerable<byte[]> messages, DateTime now,  
            Action<IReadOnlyList<MessageContent>> callbackInsideLock)
        {

            _requestsPerSecond++;
            
            if (_queueCount == 0)
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

                    var newMessage = MessageContent.Create(messageId, 0, message, now);

                    messagesToPersist.Add(newMessage);

                    foreach (var topic in _topicQueues.Values)
                        topic.NewMessage(messageId);

                    _messagePerSecond++;
                }

                callbackInsideLock(messagesToPersist);
            }

            return messagesToPersist;
        }

        public long GetQueueMessagesCount(string queueName)
        {
            lock (_lockObject)
            {
                if (!_topicQueues.ContainsKey(queueName))
                    throw new Exception($"Queue [{queueName}] is not found");

                var topicQueue = _topicQueues[queueName];

                return topicQueue.GetMessagesCount();

            }
        }


        public IReadOnlyList<IQueueSnapshot> GetQueuesSnapshot()
        {
            List<IQueueSnapshot> result = null;

            lock (_lockObject)
            {
                foreach (var topicQueue in _topicQueues.Values.Where(itm => !itm.DeleteOnDisconnect))
                {
                    var snapshot = topicQueue.GetSnapshot();

                    if (result == null)
                        result = new List<IQueueSnapshot>();

                    result.Add(snapshot);
                }
            }

            if (result == null)
                return Array.Empty<IQueueSnapshot>();

            return result;
        }

        public long GetMinMessageId()
        {
            lock (_lockObject)
            {
                if (_topicQueues.Count == 0)
                    return 0;

                return _topicQueues.Values.Min(itm => itm.GetMinId());
            }
        }

        public TopicQueue GetQueue(string queueId)
        {
            lock (_lockObject)
            {
                if (_topicQueues.ContainsKey(queueId))
                    return _topicQueues[queueId];

                throw new Exception($"Queue with id {queueId} is not found");
            }
        }
    }


}