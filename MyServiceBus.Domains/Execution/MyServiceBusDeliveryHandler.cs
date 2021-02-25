using System;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Execution
{
    
    public class MyServiceBusDeliveryHandler
    {
        private readonly MessagesPageLoader _messagesPageLoader;
        private readonly IMyServiceBusSettings _myServiceBusSettings;
        private readonly Log _log;
        
        public MyServiceBusDeliveryHandler(MessagesPageLoader messagesPageLoader,
            IMyServiceBusSettings myServiceBusSettings, Log log)
        {
            _messagesPageLoader = messagesPageLoader;
            _myServiceBusSettings = myServiceBusSettings;
            _log = log;
        }

        private async ValueTask FillMessagesAsync(TopicQueue topicQueue, TheQueueSubscriber subscriber)
        {
            foreach (var (messageId, attemptNo) in topicQueue.DequeNextMessage())
            {

                if (messageId < 0)
                    return;

                var pageId = messageId.GetMessageContentPageId();
                
                var (myMessage, pageIsLoaded) =
                    topicQueue.Topic.MessagesContentCache.TryGetMessage(pageId, messageId);

                if (!pageIsLoaded)
                {
                    await _messagesPageLoader.LoadPageAsync(topicQueue.Topic, pageId);
                    (myMessage, _) = topicQueue.Topic.MessagesContentCache.TryGetMessage(pageId, messageId);
                }

                if (myMessage == null)
                {
                    _log.AddLog(LogLevel.Warning, topicQueue,
                        $"Message #{messageId} with AttemptNo:{attemptNo} is not found. Skipping it...");
                    continue;
                }

                subscriber.AddMessage(myMessage, attemptNo);


                if (subscriber.Session.Disconnected)
                {
                    _log.AddLog(LogLevel.Warning, topicQueue,
                        $"Disconnected while we were Filling package with Messages for the Session: {subscriber.Session.SubscriberId}");
                    return;
                }

                if (subscriber.MessagesSize >= _myServiceBusSettings.MaxDeliveryPackageSize)
                    return;
            }

        }

        public async ValueTask SendMessagesAsync(TopicQueue topicQueue)
        {
            var leasedSubscriber = topicQueue.SubscribersList.LeaseSubscriber();
            
            if (leasedSubscriber == null)
                return;

            try
            {
                await FillMessagesAsync(topicQueue, leasedSubscriber);
            }
            catch (Exception ex)
            {
                if (leasedSubscriber.MessagesSize > 0)
                {
                    topicQueue.CancelDelivery(leasedSubscriber);
                }
                _log.AddLog(LogLevel.Error, topicQueue, ex.Message);
                Console.WriteLine(ex);
            }
            finally
            {
                topicQueue.SubscribersList.UnLease(leasedSubscriber);
            }
        }

        public async ValueTask SendMessagesAsync(MyTopic topic)
        {
            foreach (var topicQueue in topic.GetQueues())
                await SendMessagesAsync(topicQueue);
        }
    }
}