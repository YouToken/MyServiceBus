using System.Collections.Generic;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.QueueSubscribers
{
    public interface IQueueSubscriber
    {
        void SendMessagesAsync(TopicQueue topicQueue, IReadOnlyList<MessageContentGrpcModel> messages, long confirmationId);
        
        string SubscriberId { get; }
        
        bool Disconnected { get; }
    }
}