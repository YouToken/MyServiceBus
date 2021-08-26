using System;
using System.Collections.Generic;
using System.Threading;

namespace MyServiceBus.Domains.Sessions
{
    public class TopicPublisherInfo
    {

        private Dictionary<string, DateTime> _topicsPublishers = new ();


        public readonly MetricPerSecond PublishMetricPerSecond = new ();


        public void AddIfNotExists(string topic)
        {

            if (_topicsPublishers.ContainsKey(topic))
            {
                _topicsPublishers[topic] = DateTime.UtcNow;
                return;
            }

            lock (_topicsPublishers)
            {
                var (added, newDictionary, _) = _topicsPublishers.AddIfNotExistsByCreatingNewDictionary(topic, () => DateTime.UtcNow);

                if (added)
                    _topicsPublishers = newDictionary;
                else
                    _topicsPublishers[topic] = DateTime.UtcNow;
            }

        }

        public bool IsTopicPublisher(string topicName)
        {
            return _topicsPublishers.ContainsKey(topicName);
        }
        
                
        public Dictionary<string, DateTime> GetTopicsToPublish()
        {
            return _topicsPublishers;
        }
        

    }
}