using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Server.Tcp;

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

        public static TopicMonitoringModel Create(MyTopic topic, IReadOnlyList<MyServiceBusTcpContext> connections)
        {
            return new TopicMonitoringModel
            {
                Id = topic.TopicId,
                Size = topic.MessagesCount,
                MsgPerSec = topic.MessagesPerSecond,
                RequestsPerSec = topic.RequestsPerSecond,
                Consumers = topic.GetConsumers(),
                MessageId = topic.MessageId,
                Publishers = connections.Where(itm => itm.Session.IsTopicPublisher(topic.TopicId)).Select(itm => itm.Id),
                CachedPages = ServiceLocator.CacheByTopic.GetPagesByTopic(topic.TopicId),
                MessagesPerSecond = ServiceLocator.MessagesPerSecondByTopic.GetRecordsPerSecond(topic.TopicId)
            };
        }
    }

    public class UnknownConnectionModel
    {
        public long Id { get; set; }
        public string Ip { get; set; }
        
        public string ConnectedTimeStamp { get; set; }
        public long SentBytes { get; set; }
        public string LastSendDuration { get; set; }
        public long ReceivedBytes { get; set; }
        public string SentTimeStamp { get; set; }
        public string ReceiveTimeStamp { get; set; }

        internal void Init(MyServiceBusTcpContext context)
        {
            var now = DateTime.UtcNow;
            Id = context.Id;
            Ip = context.TcpClient.Client.RemoteEndPoint.ToString();
            ConnectedTimeStamp = (now - context.SocketStatistic.ConnectionTime).FormatTimeStamp();
            SentBytes = context.SocketStatistic.Sent;
            SentBytes = context.SocketStatistic.Sent;
            ReceivedBytes = context.SocketStatistic.Received;
            SentTimeStamp = (now - context.SocketStatistic.LastSendTime).FormatTimeStamp();
            ReceiveTimeStamp = (now - context.SocketStatistic.LastReceiveTime).FormatTimeStamp();
            LastSendDuration = context.SocketStatistic.LastSendToSocketDuration.FormatTimeStamp();
        }
        
        internal void Init(MyServiceBusSession session)
        {
            var now = DateTime.UtcNow;
            Id = session.Id;
            Ip = session.Ip;
            ConnectedTimeStamp = (now - session.Created).FormatTimeStamp();
            SentBytes = 0;
            SentBytes = 0;
            ReceivedBytes = 0;
            SentTimeStamp = "---";
            ReceiveTimeStamp = (now - session.LastAccess).FormatTimeStamp();
            LastSendDuration = "---";
        }

        public static UnknownConnectionModel Create(MyServiceBusTcpContext ctx)
        {
            var result = new UnknownConnectionModel();
            result.Init(ctx);

            return result;
        }


    }

    public class ConnectionModel : UnknownConnectionModel
    {
        public string Name { get; set; }

        public int ProtocolVersion { get; set; }
        
        public int PublishPacketsPerSecond { get; set; }
        public int SubscribePacketsPerSecond { get; set; }
        public int PacketsPerSecondInternal { get; set; }
        public IEnumerable<string> Topics { get; set; }
        
        public IEnumerable<string> Queues { get; set; }

        public new static ConnectionModel Create(MyServiceBusTcpContext context)
        {
            var result = new ConnectionModel
            {
                Name = context.ContextName,
                Topics = context.Session.GetTopicsToPublish(),
                Queues = context.Session.GetQueueSubscribers().Select(itm => itm.Topic.TopicId+">>>"+itm.QueueId),
                PublishPacketsPerSecond = context.Session.PublishPacketsPerSecond,
                SubscribePacketsPerSecond = context.Session.SubscribePacketsInternal,
                PacketsPerSecondInternal = context.Session.PacketsPerSecond,
                ProtocolVersion = context.Session.ProtocolVersion,
            };
            
            result.Init(context);

            return result;
        }
        
        public static ConnectionModel Create(MyServiceBusSession session)
        {
            var result = new ConnectionModel
            {
                Name = session.SessionType+"-"+session.Name,
                Topics = session.GetTopicsToPublish(),
                Queues = session.GetQueueSubscribers().Select(itm => itm.Topic.TopicId+">>>"+itm.QueueId),
                PublishPacketsPerSecond = session.PublishPacketsPerSecond,
                SubscribePacketsPerSecond = session.SubscribePacketsInternal,
                PacketsPerSecondInternal = session.PacketsPerSecond,
                ProtocolVersion = session.ProtocolVersion,
            };
            
            result.Init(session);

            return result;
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
        
        public List<ConnectionModel> Connections { get; set; }
        public IEnumerable<UnknownConnectionModel> UnknownConnections { get; set; }
        
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