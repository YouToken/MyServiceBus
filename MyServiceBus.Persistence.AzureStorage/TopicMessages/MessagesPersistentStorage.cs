using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Persistence.AzureStorage.PageBlob;

namespace MyServiceBus.Persistence.AzureStorage.TopicMessages
{

    public class MessagesPersistentStorage : IMessagesPersistentStorage
    {

        private readonly Func<string, MessagesPageId, IPageBlob> _getMessagesBlob;



        private readonly Dictionary<string, BlobMessagesStorageProcessor> _currentProcessors
            = new Dictionary<string, BlobMessagesStorageProcessor>();

        public MessagesPersistentStorage(Func<string, MessagesPageId, IPageBlob> getMessagesBlob)
        {
            _getMessagesBlob = getMessagesBlob;
        }

        
        private BlobMessagesStorageProcessor TryGetCurrentMessageProcessor(string topicId, MessagesPageId pageId)
        {
            lock (_currentProcessors)
            {
                if (_currentProcessors.ContainsKey(topicId))
                {
                    var result = _currentProcessors[topicId];

                    if (result.PageId.Equals(pageId))
                        return result;
                }

                return null;

            }
        } 

        private BlobMessagesStorageProcessor GetCurrentMessageProcessorOrCreateOne(string topicId, MessagesPageId pageId)
        {
            lock (_currentProcessors)
            {
                if (_currentProcessors.ContainsKey(topicId))
                {
                    var result = _currentProcessors[topicId];

                    if (result.PageId.Equals(pageId))
                        return result;

                    _currentProcessors.Remove(topicId);
                }

                var pageProcessor = _getMessagesBlob(topicId, pageId);
                var newItem = new BlobMessagesStorageProcessor(pageProcessor, pageId);
                _currentProcessors.Add(topicId, newItem);
                return newItem;
                
            }
        }


        public async Task SaveAsync(string topicId, IReadOnlyList<IMessageContent> messages)
        {

            var groupedMessages 
                = messages.GroupBy(itm => itm.GetMessageContentPageId());

            foreach (var group in groupedMessages)
            {
                var pageId = group.Key;
                var blobProcessor = GetCurrentMessageProcessorOrCreateOne(topicId, pageId);
                await blobProcessor.SaveMessagesAsync(group, 100);
            }

        }

        public ValueTask<MessagesPageInMemory> GetMessagesPageAsync(string topicId, MessagesPageId pageId)
        {

            var messageProcessor = TryGetCurrentMessageProcessor(topicId, pageId);

            if (messageProcessor != null)
                return messageProcessor.GetPageMessagesAsync();


            var pageBlob = _getMessagesBlob(topicId, pageId);
            
            messageProcessor = new BlobMessagesStorageProcessor(pageBlob, pageId);

            return messageProcessor.GetPageMessagesAsync();
        }

        public ValueTask GarbageCollectAsync(string topicId, long messageId)
        {
            return new ValueTask();
        }


    }
}