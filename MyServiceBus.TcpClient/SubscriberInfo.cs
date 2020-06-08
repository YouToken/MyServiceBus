using System;
using System.Threading.Tasks;
using MyServiceBus.Abstractions;

namespace MyServiceBus.TcpClient
{
    
    public class SubscriberInfo
    {
        public SubscriberInfo(string topicId, string queueId, bool deleteOnDisconnect,
            Func<IMyServiceBusMessage, ValueTask> callback)
        {
            TopicId = topicId;
            QueueId = queueId;
            DeleteOnDisconnect = deleteOnDisconnect;
            Callback = callback;
        }

        public string TopicId { get; }
        public string QueueId { get; }
        public bool DeleteOnDisconnect { get; }
        public Func<IMyServiceBusMessage, ValueTask> Callback { get; }

    }
}