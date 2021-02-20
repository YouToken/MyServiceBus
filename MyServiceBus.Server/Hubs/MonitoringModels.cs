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
        public string Ip { get; set; }
    }


}