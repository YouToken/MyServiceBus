using System.Collections.Generic;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.MessagesContent
{
    public static class MessagesPagingUtils
    {

        public const long MessagesInChunk = 100000;

        public static MessagesPageId GetMessageContentPageId(this long messageId)
        {
            return new (messageId / MessagesInChunk);
        }
        
        public static MessagesPageId GetMessageContentPageId(this MessageContentGrpcModel message)
        {
            return new (message.MessageId / MessagesInChunk);
        }
        
        public static Dictionary<long, long> GetActiveMessagePages(this MyTopic topic)
        {
            var result = new Dictionary<long, long>();
            
            var maxPageId = topic.MessageId.Value.GetMessageContentPageId();
            result.Add(maxPageId.Value, maxPageId.Value);
            
            foreach (var queue in topic.GetQueues())
            {
                var pageId = queue.GetMinId().GetMessageContentPageId();

                if (!result.ContainsKey(pageId.Value))
                    result.Add(pageId.Value, pageId.Value);
            }

            return result;
        }
    }
    
}