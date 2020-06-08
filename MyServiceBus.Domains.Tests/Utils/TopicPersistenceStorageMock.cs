using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Domains.Tests.Utils
{
    /*
    public class TopicPersistenceStorageMock : ITopicPersistenceStorage
    {
        private IEnumerable<ITopicPersistence> _topicData;
        private Dictionary<string, IReadOnlyList<IQueueSnapshot>> _queueIndices;
        public Task SaveAsync(IEnumerable<ITopicPersistence> topicsData, Dictionary<string, IReadOnlyList<IQueueSnapshot>> queueIndices)
        {
            _topicData = topicsData.ToList();
            _queueIndices = queueIndices;
            return Task.CompletedTask;
        }

        public Task<(IEnumerable<ITopicPersistence> topicsData, Dictionary<string, IReadOnlyList<IQueueSnapshot>> snapshot)> GetSnapshotAsync()
        {
            return Task.FromResult((_topicData, _queueIndices));
        }
    }
    */
}