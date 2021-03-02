using System.Collections.Generic;

namespace MyServiceBus.Domains.Sessions
{
    public class TopicPublisherInfo
    {
        
        private readonly ConcurrentDictionaryWithNoLocksOnRead<string, string> _topicsPublishers = new ();


        public readonly MetricPerSecond PublishMetricPerSecond = new ();

        
        public void AddIfNotExists(string topic)
        {
            _topicsPublishers.Add(topic, ()=>topic);
        }

        public bool IsTopicPublisher(string topicName)
        {
            return _topicsPublishers.ContainsKey(topicName);
        }
        
                
        public IReadOnlyList<string> GetTopicsToPublish()
        {
            return _topicsPublishers.GetAllValues();
        }
    }
}