using System;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.Execution
{
    internal enum FillPageIterationResult
    {
        Finished, LoadPage
    }
    
    
    public class MyServiceBusDeliveryHandler
    {
        private readonly IMyServiceBusMessagesPersistenceGrpcService _messagesPersistenceGrpcService;
        private readonly IMyServiceBusSettings _myServiceBusSettings;
        private readonly Log _log;

        public MyServiceBusDeliveryHandler(IMyServiceBusMessagesPersistenceGrpcService messagesPersistenceGrpcService, 
            IMyServiceBusSettings myServiceBusSettings, Log log)
        {
            _messagesPersistenceGrpcService = messagesPersistenceGrpcService;
            _myServiceBusSettings = myServiceBusSettings;
            _log = log;
        }
        
        private async Task LoadPageAsync(MessagesContentCache cache, long messageId)
        {
            var pageId = messageId.GetMessageContentPageId();
            
            var attemptNo = 0;

            while (true)
            {
                if (attemptNo >= 5)
                {
                    var emptyPage = new MessagesPageInMemory(pageId);
                    cache.UploadPage(emptyPage);
                    return;
                }

                try
                {
                    Console.WriteLine(
                        $"Trying to restore message for topic {cache.TopicId} with messageId:{messageId} during LoadMessageAsync");

                    var page =
                        await _messagesPersistenceGrpcService.GetPageAsync(cache.TopicId, pageId.Value)
                            .ToPageInMemoryAsync(pageId);

                    cache.UploadPage(page);

                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"Count not load page {pageId} for topic {cache.TopicId}. Attempt: {attemptNo}. Message: " +
                        e.Message);

                    await Task.Delay(200);
                    attemptNo++;
                }

            }
        }

        private async ValueTask FillMessagesAsync(TopicQueue topicQueue, TheQueueSubscriber subscriber)
        {

            long messageId = -1;


            while (true)
            {

                var nextAction = topicQueue.LockAndGetWriteAccess(topicDequeue =>
                {
                    do
                    {
                        var nextMessage = topicDequeue.DequeAndLease();
                        messageId = nextMessage.messageId;

                        if (messageId < 0)
                            return FillPageIterationResult.Finished;

                        var (myMessage, pageIsLoaded) = topicQueue.Topic.MessagesContentCache.TryGetMessage(messageId);

                        
                        if (!pageIsLoaded)
                            return FillPageIterationResult.LoadPage;
                        

                        if (myMessage == null)
                        {
                            _log.AddLog(LogLevel.Warning, topicQueue,
                                $"Message #{messageId} with AttemptNo:{nextMessage.attemptNo} is not found. Skipping it...");
                            continue;
                        }

                        subscriber.AddMessage(myMessage, nextMessage.attemptNo);

                        if (subscriber.Session.Disconnected)
                        {
                            _log.AddLog(LogLevel.Warning, topicQueue,
                                $"Disconnected while we were Filling package with Messages for the Session: {subscriber.Session.SubscriberId}");
                            return FillPageIterationResult.Finished;
                        }

                    } while (subscriber.MessagesSize < _myServiceBusSettings.MaxDeliveryPackageSize);

                    return FillPageIterationResult.Finished;
                });

                if (nextAction == FillPageIterationResult.Finished)
                    break;

                if (nextAction == FillPageIterationResult.LoadPage)
                    await LoadPageAsync(topicQueue.Topic.MessagesContentCache, messageId);
            }

        }

        public async ValueTask SendMessagesAsync(TopicQueue topicQueue)
        {
            var leasedSubscriber = topicQueue.QueueSubscribersList.LeaseSubscriber();
            
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
                    topicQueue.LockAndGetWriteAccess(writeAccess =>
                    {
                        writeAccess.CancelDelivery(leasedSubscriber);    
                    });
                }
                _log.AddLog(LogLevel.Error, topicQueue, ex.Message);
                Console.WriteLine(ex);
            }
            finally
            {
                topicQueue.QueueSubscribersList.UnLease(leasedSubscriber);
            }
        }

        public async ValueTask SendMessagesAsync(MyTopic topic)
        {
            foreach (var topicQueue in topic.GetQueues())
                await SendMessagesAsync(topicQueue);
        }
    }
}