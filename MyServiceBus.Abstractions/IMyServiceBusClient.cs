using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyServiceBus.Abstractions
{
    public interface IMyServiceBusClient
    {
        Task PublishAsync(string topicId, byte[] valueToPublish, bool immediatelyPersist);

        Task PublishAsync(string topicId, IEnumerable<byte[]> valueToPublish, bool immediatelyPersist);

        void Subscribe(string topicId, string queueId, bool deleteOnDisconnect,
            Func<IMyServiceBusMessage, ValueTask> callback);

        void Subscribe(string topicId, string queueId, bool deleteOnDisconnect,
            Func<IReadOnlyList<IMyServiceBusMessage>, ValueTask> callback);
    }
    
}