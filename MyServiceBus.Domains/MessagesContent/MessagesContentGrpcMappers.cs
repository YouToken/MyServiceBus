using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.MessagesContent
{
    public static class MessagesContentGrpcMappers
    {


        public static MessageContentGrpcModel ToGrpcModel(this IMessageContent messageContent)
        {
            return new MessageContentGrpcModel
            {
                Created = messageContent.Created,
                Data = messageContent.Data,
                MessageId = messageContent.MessageId
            };
        }


        public static ValueTask SaveMessagesAsync(this IMyServiceBusMessagesPersistenceGrpcService grpcService,
            string topicId, IEnumerable<IMessageContent> messages, int packetSize)
        {
            var grpcMessages = messages.OrderBy(itm => itm.MessageId).Select(msg => msg.ToGrpcModel());
            return grpcService.SaveMessagesAsync(topicId, grpcMessages.ToArray(), packetSize);
        }
        

        public static async ValueTask<MessagesPageInMemory> ToPageInMemoryAsync(this IAsyncEnumerable<MessageContentGrpcModel> pageMessagesAsync, MessagesPageId pageId)
        {
            var result = new MessagesPageInMemory(pageId);

            await foreach (var grpcMessage in pageMessagesAsync)
            {
                var domainMessage = MessageContent.FromGrpc(grpcMessage);
                result.Add(domainMessage);
            }

            return result;
        }
        
    }
}