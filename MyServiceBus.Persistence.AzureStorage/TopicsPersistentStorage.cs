using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
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


        public async Task SaveAsync(IEnumerable<ITopicPersistence> topicsData, Dictionary<string, IReadOnlyList<IQueueSnapshot>> queueDataDict)
        {
            var dataToSave = new List<TopicAndQueuesBlobContract>();
            foreach (var topicData in topicsData)
            {
                var queueData = queueDataDict[topicData.TopicId];

                var topicDataToSave = TopicAndQueuesBlobContract.Create(topicData, queueData); 
                
                dataToSave.Add(topicDataToSave);
            }

            await _topicAndQueuesPageBlob.WriteAsProtobufAsync(dataToSave);
        }

        public async Task<(IEnumerable<ITopicPersistence> topicsData, Dictionary<string, IReadOnlyList<IQueueSnapshot>> snapshot)> GetSnapshotAsync()
        {
            var result = await _topicAndQueuesPageBlob.ReadAndDeserializeAsProtobufAsync<List<TopicAndQueuesBlobContract>>();

            foreach (var itm in result)
            {
                if (itm.Snapshots == null)
                    itm.Snapshots = Array.Empty<QueueSnapshotBlobContract>();
            }

            var result2 = result.ToDictionary(itm => itm.TopicId,
                itm => itm.Snapshots.Cast<IQueueSnapshot>().AsReadOnlyList());
            
            return (result, result2);
        }
    }
    
}