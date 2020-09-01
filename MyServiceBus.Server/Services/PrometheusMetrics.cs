using System;
using System.Collections.Generic;
using MyServiceBus.Domains;
using Prometheus;

namespace MyServiceBus.Server.Services
{
    public class PrometheusMetrics : IMetricCollector
    {
        public readonly Dictionary<string,Gauge> TopicQueueSizeMetrics = new Dictionary<string, Gauge>();
        
        private Gauge GetQueueSizeMetric(string topicId, string suffix)
        {
            lock (TopicQueueSizeMetrics)
            {
                var key = topicId + suffix;
                if (TopicQueueSizeMetrics.ContainsKey(key))
                    return TopicQueueSizeMetrics[key];

                var topicSizeGauge = Metrics.CreateGauge(topicId.Replace('-', '_') + suffix,
                    "Topic " + topicId + " size of the queue",
                    new GaugeConfiguration
                    {
                        LabelNames = new[] {"topicId"}
                    });
                TopicQueueSizeMetrics.Add(key, topicSizeGauge);
                return topicSizeGauge;
            }
        }

        public void TopicQueueSize(string topicId, long queueSize)
        {
            try
            {
                var topicQueueSizeGauge = GetQueueSizeMetric(topicId, "_qsize");
                topicQueueSizeGauge.Set(queueSize);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ToPersistSize(string topicId, long queueSize)
        {
            try
            {
                var topicQueueSizeGauge = GetQueueSizeMetric(topicId, "_persist_size");
                topicQueueSizeGauge.Set(queueSize);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}