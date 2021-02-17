using System;
using System.Threading.Tasks;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.MessagesContent
{
    public class MessageContentReader
    {
        private readonly MessageContentCacheByTopic _messageContentCacheByTopic;
        private readonly IMyServiceBusMessagesPersistenceGrpcService _messagesPersistenceGrpcService;

        public MessageContentReader(MessageContentCacheByTopic messageContentCacheByTopic,
            IMyServiceBusMessagesPersistenceGrpcService messagesPersistenceGrpcService)
        {
            _messageContentCacheByTopic = messageContentCacheByTopic;
            _messagesPersistenceGrpcService = messagesPersistenceGrpcService;
        }

        private async Task<MessageContentGrpcModel> LoadMessageAsync(MessagesContentCache cache, long messageId)
        {
            var pageId = messageId.GetMessageContentPageId();
            
            var attemptNo = 0;

            while (true)
            {
                if (attemptNo >= 5)
                    return null;

                try
                {
                    Console.WriteLine(
                        $"Trying to restore message for topic {cache.TopicId} with messageId:{messageId} during LoadMessageAsync");

                    var page =
                        await _messagesPersistenceGrpcService.GetPageAsync(cache.TopicId, pageId.Value)
                            .ToPageInMemoryAsync(pageId);

                    cache.UploadPage(page);

                    return cache.TryGetMessage(messageId);
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

        public ValueTask<MessageContentGrpcModel> GetAsync(string topicId, long id)
        {
            var cache = _messageContentCacheByTopic.GetTopic(topicId);

            var message = cache.TryGetMessage(id);
            
            return message != null 
                ? new ValueTask<MessageContentGrpcModel>(message) 
                : new ValueTask<MessageContentGrpcModel>(LoadMessageAsync(cache, id));
        }
        
    }
    
}