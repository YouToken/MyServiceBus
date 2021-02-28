using System.Collections.Generic;
using MyServiceBus.Abstractions;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.Persistence
{
    

    public interface IQueueSnapshot
    {
        string QueueId { get;}
        
        IEnumerable<IQueueIndexRange> Ranges { get; }
        
        TopicQueueType TopicQueueType { get; }
    }

    public class QueueSnapshot : IQueueSnapshot
    {
        public string QueueId { get; set; }
        public IReadOnlyList<QueueIndexRangeReadOnly> RangesData { get; set; }
        public IEnumerable<IQueueIndexRange> Ranges => RangesData;
        
        public TopicQueueType TopicQueueType { get; set; }
    }
    


}