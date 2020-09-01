using System;
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

                var topicSizeGauge = Metrics.CreateGauge(topicId.Replace('-', '_') + "_qsize",
                    "Topic " + topicId + " size of the queue",
                    new GaugeConfiguration
                    {
                        LabelNames = new[] {"topicId"}
                    });
                TopicQueueSizeMetrics.Add(topicId, topicSizeGauge);
                return topicSizeGauge;
            }
        }

        public void TopicQueueSize(string topicId, long queueSize)
        {
            try
            {
                var topicQueueSizeGauge = GetQueueSizeMetric(topicId);
                topicQueueSizeGauge.Set(queueSize);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}