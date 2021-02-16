using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Abstractions;

namespace MyServiceBus.TcpClient
{
    
    public class SubscriberInfo
    {
        public SubscriberInfo(string topicId, string queueId, bool deleteOnDisconnect,
            Func<IMyServiceBusMessage, ValueTask> callbackAsOneMessage, Func<IReadOnlyList<IMyServiceBusMessage>, ValueTask> callbackAsAPackage)
        {
            TopicId = topicId;
            QueueId = queueId;
            DeleteOnDisconnect = deleteOnDisconnect;
            CallbackAsOneMessage = callbackAsOneMessage;
            CallbackAsAPackage = callbackAsAPackage;
        }

        public string TopicId { get; }
        public string QueueId { get; }
        public bool DeleteOnDisconnect { get; }
        public Func<IMyServiceBusMessage, ValueTask> CallbackAsOneMessage { get; }
        public Func<IReadOnlyList<IMyServiceBusMessage>, ValueTask> CallbackAsAPackage { get; }

    }
}