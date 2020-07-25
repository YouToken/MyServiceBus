using System;
using System.Threading.Tasks;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Domains.MessagesContent
{
    public class MessageContentReader
    {
        private readonly MessageContentCacheByTopic _messageContentCacheByTopic;
        private readonly IMessagesPersistentStorage _messagesPersistentStorage;


        public MessageContentReader(MessageContentCacheByTopic messageContentCacheByTopic, 
            IMessagesPersistentStorage messagesPersistentStorage)
        {
            _messageContentCacheByTopic = messageContentCacheByTopic;
            _messagesPersistentStorage = messagesPersistentStorage;
        }

        private async Task<IMessageContent> LoadMessageAsync(MessagesContentCache cache, long messageId)
        {

            var now = DateTime.UtcNow;
            while (true)
            {

                var pageId = messageId.GetMessageContentPageId();
                
                var messages = 
                    await _messagesPersistentStorage.GetMessagesPageAsync(cache.TopicId, pageId);
                
                Console.WriteLine($"Trying to restore message for topic {cache.TopicId} with messageId:{messageId} during LoadMessageAsync");

                cache.UploadPage(messages);
                
                var result = cache.TryGetMessage(messageId);

                if (result != null)
                    return result;

                if ((DateTime.UtcNow - now).TotalSeconds > 3)
                {
                    Console.WriteLine($"Can not load message {messageId} from DB");
                }

                await Task.Delay(200);
            }
        }
        
        public ValueTask<IMessageContent> GetAsync(string topicId, long id)
        {
            var cache = _messageContentCacheByTopic.GetTopic(topicId);

            var message = cache.TryGetMessage(id);
            
            if (message != null)
                return new ValueTask<IMessageContent>(message);
            
            return new ValueTask<IMessageContent>(LoadMessageAsync(cache, id));
        }
        
    }
    
}