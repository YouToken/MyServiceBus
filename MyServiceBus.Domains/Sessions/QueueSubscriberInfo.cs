using System;
using System.Collections.Generic;

namespace MyServiceBus.Domains.Sessions
{
    public class QueueSubscriberInfo
    {
        private Dictionary<string, Dictionary<string, DateTime>> _subscribers = new ();

        public void DeliveryAttempt(string topic, string queueId)
        {

            if (_subscribers.TryGetValue(topic, out var subscriber))
            {
                if (subscriber.ContainsKey(queueId))
                {
                    subscriber[queueId] = DateTime.UtcNow;
                    return;
                }
            }

            lock (_subscribers)
            {
                var resultByTopic = _subscribers.AddIfNotExistsByCreatingNewDictionary(topic, () => new Dictionary<string, DateTime>());

                if (resultByTopic.added)
                    _subscribers = resultByTopic.newDictionary;


                var subscriberInfo = _subscribers[topic];
                
                if (subscriberInfo.ContainsKey(queueId))
                    subscriberInfo[queueId] = DateTime.UtcNow;
                else
                {
                    _subscribers[topic] = new Dictionary<string, DateTime>(subscriberInfo)
                    {
                        {queueId, DateTime.UtcNow}
                    };
                }


            }

        }

        public bool HasSubscriber(string topicName, string queueId)
        {
            return _subscribers.TryGetValue(topicName, out var subscriberInfo) && subscriberInfo.ContainsKey(queueId);
        }
        
        
        private readonly DateTime _defaultDateTime = DateTime.UtcNow;
                
        public DateTime GetSubscriberLastPacketDateTime(string topicName, string queueId)
        {
            if (_subscribers.TryGetValue(topicName, out var subscriberInfo))
            {
                if (subscriberInfo.TryGetValue(queueId, out var result))
                    return result;
            }

            return _defaultDateTime;
        }

        private readonly IReadOnlyDictionary<string, DateTime> _empty = new Dictionary<string, DateTime>();

     
    }
}