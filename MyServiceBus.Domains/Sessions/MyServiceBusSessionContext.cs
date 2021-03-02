using System;
using System.Collections.Generic;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.Sessions
{

    public class MyServiceBusSessionContext
    {

        public readonly TopicPublisherInfo PublisherInfo = new ();

        public readonly MetricPerSecond MessagesDeliveryMetricPerSecond = new();
        
        private IReadOnlyList<TopicQueue> _subscribersToQueueAsList = Array.Empty<TopicQueue>();
        
        public void SubscribeToQueue(TopicQueue queue)
        {
            lock (this)
            {
                _subscribersToQueueAsList = _subscribersToQueueAsList.AddToReadOnlyList(queue);
            }
        }

        public IReadOnlyList<TopicQueue> GetQueueSubscribers()
        {
            return _subscribersToQueueAsList;
        }

        public void OneSecondTimer()
        {
            MessagesDeliveryMetricPerSecond.OneSecondTimer();
            
            PublisherInfo.PublishMetricPerSecond.OneSecondTimer();
            
            foreach (var topicQueue in _subscribersToQueueAsList)
            {
                topicQueue.SubscribersList.OneSecondTimer();
            }
        }
 
    }

}