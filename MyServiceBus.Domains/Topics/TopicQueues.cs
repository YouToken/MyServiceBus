using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCoreDecorators;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.Topics
{
    public class TopicQueues
    {
        private readonly Dictionary<string, TopicQueue> _topicQueues = new Dictionary<string, TopicQueue>();
        private IReadOnlyList<TopicQueue> _queuesAsReadOnlyList = Array.Empty<TopicQueue>();


        public int QueueCount { get; private set; }
        
        public void DeleteQueue(string queueName)
        {
            if (!_topicQueues.ContainsKey(queueName))
                return;

            _topicQueues.Remove(queueName);
            _queuesAsReadOnlyList = _topicQueues.Values.AsReadOnlyList();
            QueueCount = _topicQueues.Count;
        }
        
        public IReadOnlyList<TopicQueue> GetQueues()
        {
            return _queuesAsReadOnlyList;
        }


        private long _maxMessageId;
        public long MinMessageId { get; private set; }


        public void NewMessage(long messageId)
        {
            foreach (var queue in _queuesAsReadOnlyList)
            {
                queue.NewMessage(messageId);
                if (messageId > _maxMessageId)
                    _maxMessageId = messageId;
            }
            
        }


        public long GetMessagesCount()
        {
            if (QueueCount == 0)
                return 0;
            
            return _maxMessageId-MinMessageId+1;
        }


        public void CalcMinMessageId()
        {
            if (_topicQueues.Count == 0)
            {
                MinMessageId = 0;
                return;
            }

            MinMessageId = _topicQueues.Values.Min(itm => itm.GetMinId());
        }

        public TopicQueue GetQueue(string queueId)
        {

            if (_topicQueues.ContainsKey(queueId))
                return _topicQueues[queueId];

            throw new Exception($"Queue with id {queueId} is not found");
        }


        private void AddQueuePostProcessing()
        {
            QueueCount = _topicQueues.Count;
            _queuesAsReadOnlyList = _topicQueues.Values.AsReadOnlyList(); 
        }
        
        public void Init(MyTopic topic, string queueName, bool deleteOnDisconnect,  IEnumerable<IQueueIndexRange> ranges,
            object lockObject)
        {

            var queue = new TopicQueue(topic, queueName, deleteOnDisconnect, ranges, lockObject);
            _topicQueues.Add(queueName, queue);

            AddQueuePostProcessing();

            CalcMinMessageId();
        }


        public TopicQueue CreateQueueIfNotExists(MyTopic topic, string queueName, bool deleteOnDisconnect, long messageId,
            object lockObject)
        {

            if (_topicQueues.ContainsKey(queueName))
                return _topicQueues[queueName];

            var queue = new TopicQueue(topic, queueName, deleteOnDisconnect, messageId, lockObject);
            _topicQueues.Add(queueName, queue);

            AddQueuePostProcessing();

            return queue;
        }

        public long GetQueueMessagesCount(string queueName)
        {

            if (!_topicQueues.ContainsKey(queueName))
                throw new Exception($"Queue [{queueName}] is not found");

            var topicQueue = _topicQueues[queueName];

            return topicQueue.GetMessagesCount();
        }


        public IReadOnlyList<IQueueSnapshot> GetQueuesSnapshot()
        {
            List<IQueueSnapshot> result = null;

            foreach (var topicQueue in _topicQueues.Values.Where(itm => !itm.DeleteOnDisconnect))
            {
                var snapshot = topicQueue.GetSnapshot();

                result ??= new List<IQueueSnapshot>();
                result.Add(snapshot);
            }

            if (result == null)
                return Array.Empty<IQueueSnapshot>();

            return result;
        }


    }
}