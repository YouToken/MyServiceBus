using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Server.Models
{


    public class QueueSlice
    {
        public long From { get; set; }
        public long To { get; set; }

        public static QueueSlice Create(IQueueIndexRange src)
        {
            return new QueueSlice
            {
                From = src.FromId,
                To = src.ToId
            };
        }
    }
    
    public class ConsumerModel
    {
        public string QueueId { get; set; }
        public bool DeleteOnDisconnect { get; set; }
        public int Connections { get; set; }
        public long QueueSize { get; set; }
        
        public IEnumerable<QueueSlice> ReadySlices { get; set; }
        
        public IEnumerable<QueueSlice> LeasedSlices { get; set; }
        
    }

    public class TopicMonitoringModel
    {
        public string Id { get; set; }
        public int MsgPerSec { get; set; }
        public int RequestsPerSec { get; set; }
        
        public long Size { get; set; }
        
        public long MessageId { get; set; }
        public IEnumerable<ConsumerModel> Consumers { get; set; }
        
        public IEnumerable<long> Publishers { get; set; }
        
        public IEnumerable<long> CachedPages { get; set; }
        
        public IEnumerable<int> MessagesPerSecond { get; set; }

        public static TopicMonitoringModel Create(MyTopic topic, IReadOnlyList<MySession> sessions)
        {
            return new TopicMonitoringModel
            {
                Id = topic.TopicId,
                Size = topic.GetMessagesCount(),
                MsgPerSec = topic.MessagesPerSecond,
                RequestsPerSec = topic.RequestsPerSecond,
                Consumers = topic.GetConsumers(),
                MessageId = topic.MessageId,
                Publishers = sessions.Where(itm => itm.IsTopicPublisher(topic.TopicId)).Select(itm => itm.Id),
                CachedPages = ServiceLocatorApi.CacheByTopic.GetPagesByTopic(topic.TopicId),
                MessagesPerSecond = ServiceLocatorApi.MessagesPerSecondByTopic.GetRecordsPerSecond(topic.TopicId)
            };
        }
    }

    public class ConnectionModel
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public string Ip { get; set; }
        public string DateTime { get; set; }
        public int ProtocolVersion { get; set; }
        
        public int PublishPacketsPerSecond { get; set; }
        public int SubscribePacketsPerSecond { get; set; }
        
        public int PacketsPerSecondInternal { get; set; }
        
        public IEnumerable<string> Topics { get; set; }
        
        public IEnumerable<string> Queues { get; set; }

        public static ConnectionModel Create(MySession ctx)
        {
            return new ConnectionModel
            {
                Id = ctx.Id,
                Ip = ctx.Ip,
                Name = ctx.Name,
                DateTime = ctx.LastAccess.ToString("s"),
                Topics = ctx.GetTopicsToPublish(),
                Queues = ctx.GetQueueSubscribers().Select(itm => itm.Topic.TopicId+">>>"+itm.QueueId),
                PublishPacketsPerSecond = ctx.PublishPacketsPerSecond,
                SubscribePacketsPerSecond = ctx.SubscribePacketsInternal,
                PacketsPerSecondInternal = ctx.PacketsPerSecond,
                ProtocolVersion = ctx.ProtocolVersion
            };
        }
    }


    public class QueueToPersist
    {
        public string TopicId { get; set; }
        public int Count { get; set; }
    }
    
    
    public class MonitoringModel
    {
        public IEnumerable<TopicMonitoringModel> Topics { get; set; }
        
        public IEnumerable<ConnectionModel> Connections { get; set; }
        
        public IEnumerable<QueueToPersist> QueueToPersist { get; set; }
        
        public int TcpConnections { get; set; }
    }


    public static class MonitoringHelpers
    {
        public static IEnumerable<ConsumerModel> GetConsumers(this MyTopic topic)
        {
            foreach (var topicQueue in topic.GetQueues())
            {

                var intervals = topicQueue.GetQueueIntervals();
                
                yield return new ConsumerModel
                {
                    QueueId = topicQueue.QueueId,
                    DeleteOnDisconnect = topicQueue.DeleteOnDisconnect,
                    Connections = topicQueue.QueueSubscribersList.GetCount(),
                    QueueSize = topicQueue.GetMessagesCount(),
                    LeasedSlices = intervals.leased.Select(QueueSlice.Create),
                    ReadySlices = intervals.queues.Select(QueueSlice.Create)
                };
            }
        }
    }
}