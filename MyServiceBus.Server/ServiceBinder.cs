using Grpc.Net.Client;
using MyDependencies;
using MyServiceBus.Domains;
using MyServiceBus.Persistence.Grpc;
using MyServiceBus.Server.Services;
using ProtoBuf.Grpc.Client;

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


        public static void BindGrpcServices(this IServiceRegistrator sr, string grpcUrl)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            
            sr.Register(GrpcChannel
                .ForAddress(grpcUrl)
                .CreateGrpcService<IMyServiceBusMessagesPersistenceGrpcService>());
            
            sr.Register(GrpcChannel
                .ForAddress(grpcUrl)
                .CreateGrpcService<IMyServiceBusQueuePersistenceGrpcService>());

        }
        
    }
}