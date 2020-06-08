using System.Collections.Generic;
using MyServiceBus.Abstractions.QueueIndex;

namespace MyServiceBus.Domains.Persistence
{
    

    public interface IQueueSnapshot
    {
        string QueueId { get;}
        
        IEnumerable<IQueueIndexRange> Ranges { get; }
    }

    public class QueueSnapshot : IQueueSnapshot
    {
        public string QueueId { get; set; }
        public IReadOnlyList<QueueIndexRangeReadOnly> RangesData { get; set; }
        public IEnumerable<IQueueIndexRange> Ranges => RangesData;
    }
    


}