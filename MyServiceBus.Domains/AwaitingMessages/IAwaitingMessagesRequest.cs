using System;
using System.Collections.Generic;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.AwaitingMessages
{
    public interface IAwaitingMessagesRequest
    {
        bool Expired(DateTime now);
        void SendMessages(TopicQueue topicQueue, IReadOnlyList<MessageContentGrpcModel> messages, long confirmationId);
    }
}