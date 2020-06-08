using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Persistence.AzureStorage.Contracts
{

    public class TopicQueueIndexRangeContract : IQueueIndexRange
    {
        public long FromId { get; set; }
        public long ToId { get; set; }

        public static TopicQueueIndexRangeContract Create(IQueueIndexRange src)
        {
            return new TopicQueueIndexRangeContract
            {
                FromId = src.FromId,
                ToId = src.ToId
            };
        }
    }
    
    public class TopicQueueSnapshotContract : IQueueSnapshot
    {
        public string QueueId { get; set; }
        IEnumerable<IQueueIndexRange> IQueueSnapshot.Ranges => Ranges;
        
        public List<TopicQueueIndexRangeContract> Ranges { get; set; }

        public static TopicQueueSnapshotContract Create(IQueueSnapshot src)
        {
            return new TopicQueueSnapshotContract
            {
                QueueId = src.QueueId,
                Ranges = src.Ranges.Select(TopicQueueIndexRangeContract.Create).ToList()
            };
        }
        
    }
}