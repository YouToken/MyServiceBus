namespace MyServiceBus.Abstractions.QueueIndex
{
    public class QueueIndexRangeReadOnly : IQueueIndexRange
    {
        public long FromId { get; private set; }
        public long ToId { get; private set; }

        public static QueueIndexRangeReadOnly Create(IQueueIndexRange src)
        {
            return new QueueIndexRangeReadOnly
            {
                FromId = src.FromId,
                ToId = src.ToId
            };
        }

        public override string ToString()
        {
            return $"{FromId} - {ToId}";
        }
    }
}