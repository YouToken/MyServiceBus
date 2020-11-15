using System.Collections.Generic;

namespace MyServiceBus.Domains.Persistence
{
    public interface ITopicPersistence
    {
        string TopicId { get; }
        long MessageId { get; }
        IReadOnlyList<IQueueSnapshot> QueueSnapshots { get; }
    }

    public struct TopicPersistence : ITopicPersistence
    {
        public string TopicId { get; set; }
        public long MessageId { get; set; }
        public int MaxMessagesInCache { get; set; }
        public IReadOnlyList<IQueueSnapshot> QueueSnapshots { get; set; }

    }

}