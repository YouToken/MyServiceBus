using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyServiceBus.Domains.Persistence
{
    public interface ITopicPersistence
    {
        string TopicId { get; }
        long MessageId { get; }
        int MaxMessagesInCache { get; }
    }

    public interface ITopicPersistenceStorage
    {
        Task SaveAsync(IEnumerable<ITopicPersistence> topicsData, Dictionary<string, IReadOnlyList<IQueueSnapshot>> queueIndices);

        Task<(IEnumerable<ITopicPersistence> topicsData, Dictionary<string, IReadOnlyList<IQueueSnapshot>> snapshot)>
            GetSnapshotAsync();

    }
}