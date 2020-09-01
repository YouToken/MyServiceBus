using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Persistence.AzureStorage.PageBlob;

namespace MyServiceBus.Persistence.AzureStorage
{
    
    public class TopicsPersistentStorageAzureBlobs : ITopicPersistenceStorage
    {
        private readonly IPageBlob _topicAndQueuesPageBlob;
        public TopicsPersistentStorageAzureBlobs(IPageBlob topicAndQueuesPageBlob)
        {
            _topicAndQueuesPageBlob = topicAndQueuesPageBlob;
        }


        public async Task SaveAsync(IEnumerable<ITopicPersistence> snapshot)
        {
            var dataToSave = new List<TopicAndQueuesBlobContract>();
            foreach (var topicData in snapshot)
            {
                var topicDataToSave = TopicAndQueuesBlobContract.Create(topicData); 
                dataToSave.Add(topicDataToSave);
            }

            await _topicAndQueuesPageBlob.WriteAsProtobufAsync(dataToSave);
        }

        public async Task<IReadOnlyList<ITopicPersistence>> GetSnapshotAsync()
        {
            var result = await _topicAndQueuesPageBlob.ReadAndDeserializeAsProtobufAsync<List<TopicAndQueuesBlobContract>>();

            foreach (var itm in result)
                itm.Snapshots ??= Array.Empty<QueueSnapshotBlobContract>();
            
            return result;
        }
    }
    
}