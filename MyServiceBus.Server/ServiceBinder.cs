using MyDependencies;
using MyServiceBus.Domains;
using MyServiceBus.Server.Services;

namespace MyServiceBus.Server
{
    public static class ServiceBinder
    {
        public static void BindServerServices(this IServiceRegistrator sr)
        {
            var prometheusMetrics = new PrometheusMetrics();
            sr.Register<IMetricCollector>(prometheusMetrics);
            sr.Register(prometheusMetrics);
        }
        
    }
}