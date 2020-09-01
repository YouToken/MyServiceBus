namespace MyServiceBus.Domains
{
    public interface IMetricCollector
    {
        void TopicQueueSize(string topicId, long queueSize);

        void ToPersistSize(string topicId, long queueSize);
    }
}