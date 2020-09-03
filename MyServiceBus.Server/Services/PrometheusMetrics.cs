using MyServiceBus.Domains;
using Prometheus;

namespace MyServiceBus.Server.Services
{
    public class PrometheusMetrics : IMetricCollector
    {
        public readonly Gauge ServiceBusPersistSize = CreatePersistGauge();
        public readonly Gauge ServiceBusQSize = CreateQSizeGauge();

        public void TopicQueueSize(string topicId, long queueSize)
        {
            ServiceBusQSize.WithLabels(topicId).Set(queueSize);
        }

        public void ToPersistSize(string topicId, long queueSize)
        {
            ServiceBusPersistSize.WithLabels(topicId).Set(queueSize);
        }
        
        private static Gauge CreatePersistGauge()
        {
            return Metrics.CreateGauge("service_bus_persist_queue_size",
                "Topics size of the queue", new GaugeConfiguration
                {
                    LabelNames = new[] {"topicId"}
                });
        }

        private static Gauge CreateQSizeGauge()
        {
            return Metrics.CreateGauge("service_bus_qsize_queue_size",
                "Topics size of the queue", new GaugeConfiguration
                {
                    LabelNames = new[] {"topicId"}
                });
        }
    }
}