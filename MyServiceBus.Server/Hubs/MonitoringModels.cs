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

    public class TcpConnectionSubscribeHubModel
    {
        public string TopicId { get; set; }
        public string QueueId { get; set; }

        public static TcpConnectionSubscribeHubModel Create(TopicQueue topicQueue)
        {
            return new ()
            {
                TopicId = topicQueue.Topic.TopicId,
                QueueId = topicQueue.QueueId
            };
        }
    }

    public class TcpConnectionHubModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        
        public IEnumerable<string> Topics { get; set; }
        
        public IEnumerable<TcpConnectionSubscribeHubModel> Queues { get; set; }
    }
    


}