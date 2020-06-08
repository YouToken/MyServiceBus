using System.Threading.Tasks;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Execution
{

    public class MyServiceBusSubscriber
    {
        private readonly MyServiceBusDeliveryHandler _myServiceBusDeliveryHandler;
        private readonly TopicsList _topicsList;

        public MyServiceBusSubscriber(MyServiceBusDeliveryHandler myServiceBusDeliveryHandler, TopicsList topicsList)
        {
            _myServiceBusDeliveryHandler = myServiceBusDeliveryHandler;
            _topicsList = topicsList;
        }

        public async ValueTask<TopicQueue> SubscribeToQueueAsync(TopicQueue topicQueue, IQueueSubscriber queueSubscriber)
        {
            topicQueue.QueueSubscribersList.Subscribe(queueSubscriber);
            await _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
            return topicQueue;
        }

        public ValueTask ConfirmDeliveryAsync(TopicQueue topicQueue, long confirmationId, bool ok)
        {

            if (ok)
                topicQueue.ConfirmDelivery(confirmationId);
            else
                topicQueue.ConfirmNotDelivery(confirmationId);
            
            return _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
        }
        
        public async ValueTask DisconnectSubscriberAsync(IQueueSubscriber subscriber)
        {
            var topics = _topicsList.Get();

            foreach (var topic in topics)
            {
                foreach (var queue in topic.GetQueues())
                {
                    if (!await queue.DisconnectedAsync(subscriber))
                        continue;
                    
                    await _myServiceBusDeliveryHandler.SendMessagesAsync(queue);
                    
                    if (queue.DeleteOnDisconnect && queue.QueueSubscribersList.GetCount() == 0)
                        topic.DeleteQueue(queue.QueueId);
                }
            }
        }
        
    }
    
}