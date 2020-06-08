using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Grpc.Contracts;
using MyServiceBus.Grpc.Models;

namespace MyServiceBus.Server
{
    public static class MyExtensions
    {

        public static SubscribeGrpcResponse CreateGrpcResponse(this IReadOnlyList<IMessageContent> messagesToDeliver, long confirmationId)
        {
            return new SubscribeGrpcResponse
            {
                Status = GrpcResponseStatus.Ok,
                ConfirmationId = confirmationId,
                NewMessages = messagesToDeliver.Select(itm => new MessageGrpcModel
                {
                    MessageId = itm.MessageId,
                    Message = itm.Data
                })
            };
        }
        
    }
}