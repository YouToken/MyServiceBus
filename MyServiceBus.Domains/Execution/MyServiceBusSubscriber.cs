using System.Threading.Tasks;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Execution
{

    public class MyServiceBusSubscriber
    {
        private readonly MyServiceBusDeliveryHandler _myServiceBusDeliveryHandler;
        private readonly TopicsList _topicsList;

        public MyServiceBusSubscriber(MyServiceBusDeliveryHandler myServiceBusDeliveryHandler, 
            TopicsList topicsList)
        {
            _myServiceBusDeliveryHandler = myServiceBusDeliveryHandler;
            _topicsList = topicsList;
        }

        public async ValueTask<TopicQueue> SubscribeToQueueAsync(TopicQueue topicQueue, IMyServiceBusSession session)
        {
            topicQueue.SubscribersList.Subscribe(session);
            await _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
            return topicQueue;
        }

        public ValueTask ConfirmDeliveryAsync(MyTopic topic, string queueName, long confirmationId, bool ok)
        {
            var topicQueue = topic.ConfirmDelivery(queueName, confirmationId, ok, null);
            return _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
        }
        
        public ValueTask ConfirmDeliveryAsync(MyTopic topic, string queueName, long confirmationId, QueueWithIntervals okMessages)
        {
            var topicQueue = topic.ConfirmDelivery(queueName, confirmationId, false, okMessages);
            return _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
        }
        
        public async ValueTask DisconnectSubscriberAsync(IMyServiceBusSession session)
        {
            var topics = _topicsList.Get();

            foreach (var topic in topics)
            {
                foreach (var queue in topic.GetQueues())
                {
                    if (!await queue.DisconnectedAsync(session))
                        continue;
                    
                    await _myServiceBusDeliveryHandler.SendMessagesAsync(queue);
                    
                    if (queue.DeleteOnDisconnect && queue.SubscribersList.GetCount() == 0)
                        topic.DeleteQueue(queue.QueueId);
                }
            }
        }
        
    }
    
}