using System.Collections.Generic;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Server.Hubs
{
    public class InitHubResponseModel
    {
        public string Version { get; set; }
    }

    public class TopicHubModel
    {
        public string Id { get; set; }
        
        public IEnumerable<long> Pages { get; set; }
        
        
        public IEnumerable<TopicHubModel> Queues { get; set; }
    }


    public class TopicQueueHubModel
    {
        public string Id { get; set; }
        
        public bool DeleteOnDisconnect { get; set; }
        
        public int Connections { get; set; }

        public static TopicQueueHubModel Create(TopicQueue topicQueue)
        {
            return new ()
            {
                Id = topicQueue.QueueId,
                Connections = topicQueue.SubscribersList.GetCount(),
                DeleteOnDisconnect = topicQueue.DeleteOnDisconnect,
            };
        }
    }


    public class TcpConnectionHubModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        
        public IEnumerable<string> Topics { get; set; }
    }
    
    
    public class TopicConnectionHubModel
    {
        public string Name { get; set; }
        public string Ip { get; set; }
    }
    
    
    public class TopicAndQueueMonitoringDataHubModel
    {
        public string Id { get; set; }
        public IEnumerable<long> Pages { get; set; }
        public IEnumerable<TopicConnectionHubModel> TopicConnections { get; set; }
    }


}