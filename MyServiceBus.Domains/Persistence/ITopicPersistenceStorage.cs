using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyServiceBus.Domains.Persistence
{
    public interface ITopicPersistence
    {
        string TopicId { get; }
        long MessageId { get; }
        int MaxMessagesInCache { get; }
        IReadOnlyList<IQueueSnapshot> QueueSnapshots { get; }
    }

    public struct TopicPersistence : ITopicPersistence
    {
        public string TopicId { get; set; }
        public long MessageId { get; set; }
        public int MaxMessagesInCache { get; set; }
        public IReadOnlyList<IQueueSnapshot> QueueSnapshots { get; set; }

    }

    public interface ITopicPersistenceStorage
    {
        Task SaveAsync(IEnumerable<ITopicPersistence> snapshot);

        Task<IReadOnlyList<ITopicPersistence>> GetSnapshotAsync();

    }
}