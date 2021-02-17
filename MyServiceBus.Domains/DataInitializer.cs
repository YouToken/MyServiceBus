using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains
{
    public static class DataInitializer
    {

        private static async Task InitTopicsAsync(IServiceProvider serviceProvider)
        {
            
            var topicsList = serviceProvider.GetRequiredService<TopicsList>();
            var storage = serviceProvider.GetRequiredService<IMyServiceBusQueuePersistenceGrpcService>();

            var data = await storage.GetTopicsAndQueuesSnapshotAsync();

            topicsList.Restore(data);
        }
        


        private static async Task RestoreMessagesAsync(IServiceProvider serviceProvider)
        { 
            var topicsList = serviceProvider.GetRequiredService<TopicsList>();

            var persistentProcessor = serviceProvider.GetRequiredService<MessageContentPersistentProcessor>();

            var tasks = new List<Task>();
            foreach (var topic in topicsList.Get())
            {
                var activePages = topic.GetActiveMessagePages();
                var task = persistentProcessor.LoadActivePagesAsync(topic, activePages.Keys.ToList());
                tasks.Add(task);
            }


            await Task.WhenAll(tasks);

        }

        public static async Task InitAsync(IServiceProvider serviceProvider)
        {
            await InitTopicsAsync(serviceProvider);
            await RestoreMessagesAsync(serviceProvider);
        }
        
    }
}