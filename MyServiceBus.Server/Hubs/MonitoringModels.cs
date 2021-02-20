using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.Topics;
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

    }


    public class TopicQueueHubModel
    {
        public string Id { get; set; }
        
        public bool DeleteOnDisconnect { get; set; }
        
        public long Size { get; set; }
        
        public int Connections { get; set; }
        
        public IEnumerable<QueueSliceHubModel> Ready { get; set; }

        public static TopicQueueHubModel Create(TopicQueue topicQueue)
        {
            return new ()
            {
                Id = topicQueue.QueueId,
                Connections = topicQueue.SubscribersList.GetCount(),
                DeleteOnDisconnect = topicQueue.DeleteOnDisconnect,
                Size = topicQueue.GetMessagesCount(),
                Ready = topicQueue.GetReadyQueueSnapshot().Select(QueueSliceHubModel.Create)
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
    

    public class TopicMetricsHubModel
    {
        public string Id { get; set; }
        public int MsgPerSec { get; set; }
        public int ReqPerSec { get; set; }
        public IEnumerable<long> Pages { get; set; }
        
        public IEnumerable<TopicQueueHubModel> Queues { get; set; }

        public static TopicMetricsHubModel Create(MyTopic topic)
        {
            return new ()
            {
                Id = topic.TopicId,
                Pages = topic.MessagesContentCache.Pages,
                MsgPerSec = topic.MessagesPerSecond,
                ReqPerSec = topic.RequestsPerSecond,
                Queues = topic.GetQueues().Select(TopicQueueHubModel.Create)
            };
        }
    
    }

    public class PersistentQueueHubModel
    {
        public string Id { get; set; }
        public int Size { get; set; }
    }

}