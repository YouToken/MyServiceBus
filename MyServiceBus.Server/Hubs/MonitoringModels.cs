using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Server.Models;

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
    
    public class QueueSliceHubModel
    {
        public long From { get; set; }
        public long To { get; set; }

        public static QueueSliceHubModel Create(IQueueIndexRange src)
        {
            return new ()
            {
                From = src.FromId,
                To = src.ToId
            };
        }
    }

    public class TcpConnectionSubscribeHubModel
    {
        public string TopicId { get; set; }
        public string QueueId { get; set; }
        
        public IEnumerable<QueueSliceHubModel> Leased { get; set; }

        public static TcpConnectionSubscribeHubModel Create(TopicQueue topicQueue, IEnumerable<IQueueIndexRange> src)
        {
            return new ()
            {
                TopicId = topicQueue.Topic.TopicId,
                QueueId = topicQueue.QueueId,
                Leased = src.Select(QueueSliceHubModel.Create)
            };
        }
    }

    public class TcpConnectionHubModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Connected { get; set; }
        public string Recv { get; set; }
        public long ReadBytes { get; set; }
        public long SentBytes { get; set; }
        public int ProtocolVersion { get; set; }
        
        
        public IEnumerable<string> Topics { get; set; }
        
        public IEnumerable<TcpConnectionSubscribeHubModel> Queues { get; set; }
    }
    


}