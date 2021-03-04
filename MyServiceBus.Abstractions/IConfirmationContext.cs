using System.Collections.Generic;

namespace MyServiceBus.Abstractions
{
    public interface IConfirmationContext
    {
        string TopicId { get; }
        string QueueId { get; }
        long ConfirmationId { get; }
        void ConfirmMessages(IEnumerable<long> messagesToConfirm);
    }
}