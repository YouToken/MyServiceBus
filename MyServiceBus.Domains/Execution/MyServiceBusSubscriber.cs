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

        public async ValueTask<TopicQueue> SubscribeToQueueAsync(TopicQueue topicQueue, IMyServiceBusSession session)
        {
            topicQueue.QueueSubscribersList.Subscribe(session);
            await _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
            return topicQueue;
        }

        public ValueTask ConfirmDeliveryAsync(MyTopic topic, string queueName, long confirmationId, bool ok)
        {
            var topicQueue = topic.ConfirmDelivery(queueName, confirmationId, ok);
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
                    
                    if (queue.DeleteOnDisconnect && queue.QueueSubscribersList.GetCount() == 0)
                        topic.DeleteQueue(queue.QueueId);
                }
            }
        }
        
    }
    
}