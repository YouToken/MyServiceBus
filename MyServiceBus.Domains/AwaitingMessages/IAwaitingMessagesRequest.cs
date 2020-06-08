using System;
using System.Collections.Generic;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.AwaitingMessages
{
    public interface IAwaitingMessagesRequest
    {
        bool Expired(DateTime now);
        void SendMessages(TopicQueue topicQueue, IReadOnlyList<IMessageContent> messages, long confirmationId);
    }
}