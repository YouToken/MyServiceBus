using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Domains.Topics
{
    public class TopicsList
    {
        private readonly IMetricCollector _metricCollector;

        private Dictionary<string, MyTopic> _topics = new ();
        private IReadOnlyList<MyTopic> _topicsAsList = new List<MyTopic>();
        
        
        private readonly object _lockObject = new ();

        public int SnapshotId { get; private set; }

        public TopicsList(IMetricCollector metricCollector)
        {
            _metricCollector = metricCollector;
        }

        public IReadOnlyList<MyTopic> Get()
        {
            return _topicsAsList;
        }
        
        public (IReadOnlyList<MyTopic> topics, int snapshotId) GetWithSnapshotId()
        {
            lock (_lockObject)
            {
                return (_topicsAsList, SnapshotId); 
            }
        }
        
        public MyTopic Get(string topicId)
        {
            return _topics[topicId];
        }

        public MyTopic TryGet(string topicId)
        {
            return _topics.ContainsKey(topicId) ? _topics[topicId] : null;
        }

        private MyTopic AddNewTopic(string topicId,  long startMessageId)
        {

            lock (_lockObject)
            {
                if (_topics.ContainsKey(topicId))
                    return _topics[topicId];

                var newTopic = new MyTopic(topicId, startMessageId, _metricCollector);
                var newTopics = new Dictionary<string, MyTopic>(_topics) {{topicId, newTopic}};
                _topics = newTopics;
                _topicsAsList = _topics.Values.ToList();
                SnapshotId++;
                
                return _topics[topicId];
            }
        }

        public MyTopic AddIfNotExists(string topicId)
        {
            topicId = topicId.ToLower();
            return AddNewTopic(topicId, 0);
        }

        public MyTopic TryFindTopic(string topicName)
        {
            var topic = _topics;
            return topic.ContainsKey(topicName) 
                ? topic[topicName] 
                : null;
        }

        public void KickMetricsTimer()
        {
            var topics = _topicsAsList;

            foreach (var topic in topics)
                topic.KickMetricsTimer();
        }

        public void Restore(IEnumerable<ITopicPersistence> topics)
        {
            lock (_lockObject)
            {
                foreach (var topicPersistence in topics)
                {
                    var topic = AddNewTopic(topicPersistence.TopicId,  topicPersistence.MessageId);
                    Console.WriteLine("Restoring topic: "+topicPersistence.TopicId);
                    topic.Init(topicPersistence.QueueSnapshots);
                }
                SnapshotId++;
            }
        }
        
    }
}