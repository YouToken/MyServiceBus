using System.Collections.Generic;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.QueueSubscribers
{
    public interface IQueueSubscriber
    {
        void SendMessagesAsync(TopicQueue topicQueue, IReadOnlyList<IMessageContent> messages, long confirmationId);
        
        string SubscriberId { get; }
        
        bool Disconnected { get; }
    }
}