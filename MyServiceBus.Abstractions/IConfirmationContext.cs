using System.Collections.Generic;

namespace MyServiceBus.Abstractions
{
    public interface IConfirmationContext
    {
        void ConfirmMessages(string topicId, string queueId, long confirmationId, IEnumerable<long> messagesToConfirm);
    }
}