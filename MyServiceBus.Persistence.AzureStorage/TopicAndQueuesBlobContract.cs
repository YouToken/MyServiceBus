using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Persistence.AzureStorage
{
    [DataContract]
    public class QueueIndexRangeBlobContract : IQueueIndexRange
    {
        
        [DataMember(Order = 1)]
        public long FromId { get; set; }
        
        [DataMember(Order = 2)]
        public long ToId { get; set; }

        public static QueueIndexRangeBlobContract Create(IQueueIndexRange src)
        {
            return new QueueIndexRangeBlobContract
            {
                FromId = src.FromId,
                ToId = src.ToId
            };
        }
    }

    [DataContract]
    public class QueueSnapshotBlobContract : IQueueSnapshot
    {
        [DataMember(Order = 1)]
        public string QueueId { get; set; }
        
        [DataMember(Order = 2)]
        public IEnumerable<QueueIndexRangeBlobContract> Ranges { get; set; }
        IEnumerable<IQueueIndexRange> IQueueSnapshot.Ranges => Ranges;

        public static QueueSnapshotBlobContract Create(IQueueSnapshot src)
        {
            return new QueueSnapshotBlobContract
            {
                QueueId = src.QueueId,
                Ranges = src.Ranges.Select(QueueIndexRangeBlobContract.Create).ToList()
            };
        }
    }
    
    

    [DataContract]
    public class TopicAndQueuesBlobContract : ITopicPersistence
    {
        
        [DataMember(Order = 1)]
        public string TopicId { get; set; }
        
        [DataMember(Order = 2)]
        public long MessageId { get; set; }

        [DataMember(Order = 3)]
        public int MaxMessagesInCache { get; set; }

        [DataMember(Order = 4)]
        public IEnumerable<QueueSnapshotBlobContract> Snapshots { get; set; }

        public static TopicAndQueuesBlobContract Create(ITopicPersistence topicPersistence, IEnumerable<IQueueSnapshot> snapshots)
        {
            return new TopicAndQueuesBlobContract
            {
                TopicId = topicPersistence.TopicId,
                MessageId = topicPersistence.MessageId,
                MaxMessagesInCache = topicPersistence.MaxMessagesInCache,
                Snapshots = snapshots.Select(QueueSnapshotBlobContract.Create).ToList()
            };
        }
        
    }

}