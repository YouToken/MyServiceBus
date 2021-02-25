using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Domains.Topics
{
    public class TopicsList
    {
        private readonly IMetricCollector _metricCollector;
        
        private readonly ConcurrentDictionaryWithNoLocksOnRead<string, MyTopic> _topics = new ();

        public int SnapshotId => _topics.SnapshotId;

        public TopicsList(IMetricCollector metricCollector)
        {
            _metricCollector = metricCollector;
        }

        public IReadOnlyList<MyTopic> Get()
        {
            return _topics.GetAllValues();
        }
        
        public (IReadOnlyList<MyTopic> topics, int snapshotId) GetWithSnapshotId()
        {
            return _topics.GetAllValuesWithSnapshot();
        }
        
        public MyTopic Get(string topicId)
        {
            return _topics[topicId];
        }

        public MyTopic TryGet(string topicId)
        {
            return _topics.TryGetValue(topicId, out var result) ? result : null;
        }

        private MyTopic AddNewTopic(string topicId,  long startMessageId)
        {
            return _topics.Add(topicId, () => new MyTopic(topicId, startMessageId, _metricCollector));
        }

        public MyTopic AddIfNotExists(string topicId)
        {
            topicId = topicId.ToLower();
            return AddNewTopic(topicId, 0);
        }

        public void KickMetricsTimer()
        {
            foreach (var topic in _topics.GetAllValues())
                topic.KickMetricsTimer();
        }

        public void Restore(IEnumerable<ITopicPersistence> topics)
        {

            var newTopics = topics.Select(topicPersistence =>
            {        
                Console.WriteLine("Restoring topic: " + topicPersistence.TopicId);
                var newTopic =  new MyTopic(topicPersistence.TopicId, topicPersistence.MessageId, _metricCollector);
                newTopic.Init(topicPersistence.QueueSnapshots);
                return newTopic;
            });

            _topics.AddBulk(newTopics, topic => topic.TopicId);
        }

    }
}