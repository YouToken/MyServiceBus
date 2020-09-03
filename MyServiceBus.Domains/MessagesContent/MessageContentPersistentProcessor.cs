using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.MessagesContent
{
    public class MessageContentPersistentProcessor
    {
        private readonly IMessagesPersistentStorage _messagesPersistentStorage;
        private readonly IMessagesToPersistQueue _messagesToPersistQueue;
        private readonly MessageContentCacheByTopic _messageContentCacheByTopic;

        public MessageContentPersistentProcessor(IMessagesPersistentStorage messagesPersistentStorage,
            IMessagesToPersistQueue messagesToPersistQueue, MessageContentCacheByTopic messageContentCacheByTopic)
        {
            _messagesPersistentStorage = messagesPersistentStorage;
            _messagesToPersistQueue = messagesToPersistQueue;
            _messageContentCacheByTopic = messageContentCacheByTopic;
        }

        public async Task PersistMessageContentInBackgroundAsync(MyTopic myTopic)
        {
            var messagesToPersist = _messagesToPersistQueue.GetMessagesToPersist(myTopic.TopicId);
            try
            {
                await _messagesPersistentStorage.SaveAsync(myTopic.TopicId, messagesToPersist);
            }
            catch (Exception)
            {
                _messagesToPersistQueue.EnqueueToPersist(myTopic.TopicId, messagesToPersist);
            }
        }


        public async Task LoadActivePagesAsync(MyTopic topic, IReadOnlyList<long> pages)
        {


                var contentByTopic = _messageContentCacheByTopic.TryGetTopic(topic.TopicId) 
                                     ?? _messageContentCacheByTopic.Create(topic.TopicId);


                foreach (var pageId in pages)
                {

                    try
                    {
                        if (contentByTopic.HasCacheLoaded(pageId))
                            continue;

                        var now = DateTime.UtcNow;

                        Console.WriteLine(
                            $"Restoring messages for topic {topic.TopicId} with PageId: {pageId} from Persistent Storage");

                        var messages =
                            await _messagesPersistentStorage.GetMessagesPageAsync(topic.TopicId,
                                new MessagesPageId(pageId));
                        
                        contentByTopic.UploadPage(messages);

                        Console.WriteLine(
                            $"Restored content for topic {topic.TopicId} with PageId: {pageId} from Persistent Storage in {DateTime.UtcNow - now:g}. Messages: {messages.Count}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Can not restore the page {pageId} for the topic [{topic.TopicId}]");
                        Console.WriteLine(e);
                    }

                }

        }

        public async ValueTask GarbageCollectAsync(MyTopic topic)
        {
            var minMessageId = topic.GetMinMessageId();
            
            var activePages = topic.GetActiveMessagePages();

            await LoadActivePagesAsync(topic, activePages.Keys.ToList());
            
            _messageContentCacheByTopic.GarbageCollect(topic.TopicId, activePages);
            
            await _messagesPersistentStorage.GarbageCollectAsync(topic.TopicId, minMessageId);
        }
    }
}