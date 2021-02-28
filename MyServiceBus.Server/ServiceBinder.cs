using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using MyServiceBus.Domains;
using MyServiceBus.Persistence.Grpc;
using MyServiceBus.Server.Services;
using MyServiceBus.Server.Services.Sessions;
using ProtoBuf.Grpc.Client;

namespace MyServiceBus.Server
{
    public static class ServiceBinder
    {
        public static void BindServerServices(this IServiceCollection sr)
        {
            var prometheusMetrics = new PrometheusMetrics();
            sr.AddSingleton<IMetricCollector>(prometheusMetrics);
            sr.AddSingleton(prometheusMetrics);
            sr.AddSingleton<GrpcSessionsList>();
        }


        public static void BindGrpcServices(this IServiceCollection sr, string grpcUrl)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            
            sr.AddSingleton(GrpcChannel
                .ForAddress(grpcUrl)
                .CreateGrpcService<IMyServiceBusMessagesPersistenceGrpcService>());
            
            sr.AddSingleton(GrpcChannel
                .ForAddress(grpcUrl)
                .CreateGrpcService<IMyServiceBusQueuePersistenceGrpcService>());

        }
        
    }
}