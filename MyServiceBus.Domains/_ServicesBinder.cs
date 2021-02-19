using Microsoft.Extensions.DependencyInjection;
using MyServiceBus.Domains.Execution;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Metrics;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains
{
    public static class ServicesBinder
    {
        public static void RegisterMyNoServiceBusDomainServices(this IServiceCollection sc)
        {
            
            sc.AddSingleton<TopicsList>();
            sc.AddSingleton<GlobalVariables>();
            
            sc.AddSingleton<MyServiceBusPublisher>();
            sc.AddSingleton<MyServiceBusSubscriber>();
            sc.AddSingleton<MyServiceBusBackgroundExecutor>();
            
            sc.AddSingleton<TopicsAndQueuesPersistenceProcessor>();
            
            sc.AddSingleton<MessageContentPersistentProcessor>();
            
            sc.AddSingleton<TopicsManagement>();
            
            sc.AddSingleton<SessionsList>();
            
            sc.AddSingleton<MyServiceBusDeliveryHandler>();

            sc.AddSingleton<Log>();

            sc.AddSingleton<MessageHandlingDuration>();
            sc.AddSingleton<MessagesPerSecondByTopic>();
        }
        
        
    }
}