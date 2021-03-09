using System.Threading.Tasks;
using MyServiceBus.Abstractions;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Execution
{

    public enum ReplayMessageResult
    {
        Ok, MessageNotFound
    }

    public class MyServiceBusSubscriber
    {
        private readonly MyServiceBusDeliveryHandler _myServiceBusDeliveryHandler;
        private readonly TopicsList _topicsList;
        private readonly MessagesPageLoader _messagesPageLoader;

        public MyServiceBusSubscriber(MyServiceBusDeliveryHandler myServiceBusDeliveryHandler, 
            TopicsList topicsList, MessagesPageLoader messagesPageLoader)
        {
            _myServiceBusDeliveryHandler = myServiceBusDeliveryHandler;
            _topicsList = topicsList;
            _messagesPageLoader = messagesPageLoader;
        }

        public async ValueTask<TopicQueue> SubscribeToQueueAsync(TopicQueue topicQueue, IMyServiceBusSubscriberSession session)
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
        
        public ValueTask ConfirmMessagesByNotDeliveryAsync(MyTopic topic, string queueName, long confirmationId, QueueWithIntervals queueWithIntervals)
        {
            var topicQueue = topic.ConfirmMessagesButNotDelivery(queueName, confirmationId, queueWithIntervals);
            return _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
        }
        
        public ValueTask ConfirmDeliveryAsync(MyTopic topic, string queueName, long confirmationId, QueueWithIntervals okMessages)
        {
            var topicQueue = topic.ConfirmDelivery(queueName, confirmationId, false, okMessages);
            return _myServiceBusDeliveryHandler.SendMessagesAsync(topicQueue);
        }
        
        public async ValueTask DisconnectSubscriberAsync(IMyServiceBusSubscriberSession session)
        {
            var topics = _topicsList.Get();

            foreach (var topic in topics)
            {
                foreach (var queue in topic.GetQueues())
                {
                    if (!await queue.DisconnectedAsync(session))
                        continue;
                    
                    await _myServiceBusDeliveryHandler.SendMessagesAsync(queue);
                    
                    if (queue.TopicQueueType == TopicQueueType.DeleteOnDisconnect && queue.SubscribersList.GetCount() == 0)
                        topic.DeleteQueue(queue.QueueId);
                }
            }
        }


        public async ValueTask<ReplayMessageResult> ReplayMessageAsync(TopicQueue topicQueue, long messageId)
        {

            var pageId = MessagesPageId.CreateFromMessageId(messageId);
            var result = topicQueue.Topic.MessagesContentCache.TryGetMessage(pageId, messageId);
            
            if (!result.pageIsLoaded)
            {
                await _messagesPageLoader.LoadPageAsync(topicQueue.Topic, pageId);
                result = topicQueue.Topic.MessagesContentCache.TryGetMessage(pageId, messageId);
                if (!result.pageIsLoaded)
                    return ReplayMessageResult.MessageNotFound;
            }
            
            if (result.message == null)
                return ReplayMessageResult.MessageNotFound;

            topicQueue.EnqueueMessage(result.message);

            return ReplayMessageResult.Ok;
        }
        
    }
    
}