using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.MessagesContent
{
    public class MessageContentPersistentProcessor
    {
        private readonly MessagesPageLoader _messagesPageLoader;
        private readonly IMyServiceBusMessagesPersistenceGrpcService _messagesPersistenceGrpcService;
        private readonly IMessagesToPersistQueue _messagesToPersistQueue;
        private readonly IMyServiceBusSettings _myServiceBusSettings;

        public MessageContentPersistentProcessor(MessagesPageLoader messagesPageLoader,
            IMyServiceBusMessagesPersistenceGrpcService messagesPersistenceGrpcService,
            IMessagesToPersistQueue messagesToPersistQueue, IMyServiceBusSettings myServiceBusSettings)
        {
            _messagesPageLoader = messagesPageLoader;
            _messagesPersistenceGrpcService = messagesPersistenceGrpcService;
            _messagesToPersistQueue = messagesToPersistQueue;
            _myServiceBusSettings = myServiceBusSettings;
        }

        public async Task PersistMessageContentAsync(MyTopic myTopic)
        {
            var messagesToPersist = _messagesToPersistQueue.GetMessagesToPersist(myTopic.TopicId);
            
            if (messagesToPersist.Count ==0)
                return;

            try
            {
                await _messagesPersistenceGrpcService.SaveMessagesAsync(myTopic.TopicId, messagesToPersist.ToArray(), _myServiceBusSettings.MaxPersistencePackage);
            }
            catch (Exception)
            {
                _messagesToPersistQueue.EnqueueToPersist(myTopic.TopicId, messagesToPersist);
            }
        }


        public async Task LoadActivePagesAsync(MyTopic topic, IReadOnlyList<long> pages)
        {

                var contentByTopic = topic.MessagesContentCache;


                foreach (var pageId in pages)
                {

                    try
                    {
                        if (contentByTopic.HasCacheLoaded(pageId))
                            continue;

                        var now = DateTime.UtcNow;

                        Console.WriteLine(
                            $"Restoring messages for topic {topic.TopicId} with PageId: {pageId} from Persistent Storage");

                        await _messagesPageLoader.LoadPageAsync(topic, new MessagesPageId(pageId));

                        Console.WriteLine(
                            $"Restored content for topic {topic.TopicId} with PageId: {pageId} from Persistent Storage in {DateTime.UtcNow - now:g}.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Can not restore the page {pageId} for the topic [{topic.TopicId}]");
                        Console.WriteLine(e);
                    }

                }
        }

        public async ValueTask GarbageCollectOrWarmUpAsync(MyTopic topic)
        {
            var activePages = topic.GetActiveMessagePages();

            await LoadActivePagesAsync(topic, activePages.Keys.ToList());
            
            topic.MessagesContentCache.GarbageCollect(activePages);
        }

    }
}