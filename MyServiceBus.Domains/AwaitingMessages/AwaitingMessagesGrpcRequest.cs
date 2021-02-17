using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.AwaitingMessages
{

    public class AwaitingMessagesGrpcRequest  : IAwaitingMessagesRequest
    {
        public DateTime Created { get; private set; }
        
        private static readonly TimeSpan ExpirationTimeout = TimeSpan.FromSeconds(5);

        public bool Expired(DateTime now)
        {
            return now - Created >= ExpirationTimeout;
        }
        
        public void SendMessages(TopicQueue topicQueue,  IReadOnlyList<MessageContentGrpcModel> messages, long messageId)
        {
            Response.SetResult((messages, messageId));
        }
        
        public void ExpireRequest()
        {
            Response.SetResult((Array.Empty<MessageContentGrpcModel>(), -1));
        }

        public Task<(IReadOnlyList<MessageContentGrpcModel> messages, long messageId)> GetMessages()
        {
            return Response.Task;
        }

        public string Topic { get; private set; }
        public string Queue { get; private set; }
        
        
        public readonly TaskCompletionSource<(IReadOnlyList<MessageContentGrpcModel> messages, long messageId)> Response  
            = new ();
        
        public static AwaitingMessagesGrpcRequest Create(string topic, string queue, DateTime now)
        {
            return new AwaitingMessagesGrpcRequest
            {
                Topic = topic,
                Queue = queue,
                Created = now,
            };
        }
        
    }    
  
}