using System.Collections.Generic;
using MyServiceBus.Domains;
using Prometheus;

namespace MyServiceBus.Server.Services
{
    public class PrometheusMetrics : IMetricCollector
    {
        public readonly Dictionary<string,Gauge> TopicQueueSizeMetrics = new Dictionary<string, Gauge>();

        private Gauge GetQueueSizeMetric(string topicId)
        {
            lock (TopicQueueSizeMetrics)
            {
                if (TopicQueueSizeMetrics.ContainsKey(topicId)) 
                    return TopicQueueSizeMetrics[topicId];
                
                var topicSizeGauge = Metrics.CreateGauge("topic_" + topicId + "_queue_size",
                    "Topic " + topicId + " size of the queue");
                TopicQueueSizeMetrics.Add(topicId, topicSizeGauge);
                return topicSizeGauge;
            }
        }

        public void TopicQueueSize(string topicId, long queueSize)
        {
            var topicQueueSizeGauge = GetQueueSizeMetric(topicId);
            topicQueueSizeGauge.Set(queueSize);
        }
    }
}