using System.Collections.Generic;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.QueueSubscribers
{
    public interface IMyServiceBusSession
    {
        void SendMessagesAsync(TopicQueue topicQueue, IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> messages, long confirmationId);
        string SubscriberId { get; }
        bool Disconnected { get; }
    }
}