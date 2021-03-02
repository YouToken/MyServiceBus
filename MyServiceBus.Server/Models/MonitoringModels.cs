using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Server.Services.Sessions;
using MyServiceBus.Server.Tcp;

namespace MyServiceBus.Server.Models
{

    public class QueueSlice
    {
        public long From { get; set; }
        public long To { get; set; }

        public static QueueSlice Create(IQueueIndexRange src)
        {
            return new ()
            {
                From = src.FromId,
                To = src.ToId
            };
        }
    }
    
    public class ConsumerModel
    {
        public string QueueId { get; set; }
        public int QueueType { get; set; }
        public int Connections { get; set; }
        public long QueueSize { get; set; }
        public IEnumerable<QueueSlice> ReadySlices { get; set; }
        public long LeasedAmount { get; set; }
        public IEnumerable<int> ExecutionDuration { get; set; }
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
            return new ()
            {
                Id = topic.TopicId,
                Size = topic.MessagesCount,
                MsgPerSec = topic.MessagesPerSecond,
                RequestsPerSec = topic.RequestsPerSecond,
                Consumers = topic.GetConsumers(),
                MessageId = topic.MessageId.Value,
                Publishers = connections.Where(itm => itm.SessionContext.PublisherInfo.IsTopicPublisher(topic.TopicId)).Select(itm => itm.Id),
                CachedPages = topic.MessagesContentCache.Pages,
                MessagesPerSecond = ServiceLocator.MessagesPerSecondByTopic.GetRecordsPerSecond(topic.TopicId),
            };
        }
    }

    public class UnknownConnectionModel
    {
        public string Id { get; set; }
        public string Ip { get; set; }
        public string ConnectedTimeStamp { get; set; }
        public long SentBytes { get; set; }
        public string LastSendDuration { get; set; }
        public long ReceivedBytes { get; set; }
        public string SentTimeStamp { get; set; }
        public string ReceiveTimeStamp { get; set; }

    }


    public class ConnectionQueueInfoModel
    {
        public string Id { get; set; }
        public IEnumerable<QueueSlice> Leased { get; set; }
    }

    public class TopicHubModel
    {
        public string Id { get; set; }
        public bool Light { get; set; }
        private static TimeSpan TwoSeconds = TimeSpan.FromSeconds(2);
        public static TopicHubModel Create(KeyValuePair<string, DateTime> src)
        {
            return new ()
            {
                Id = src.Key,
                Light = DateTime.UtcNow - src.Value < TwoSeconds
            };
        }
    }

    public class ConnectionModel : UnknownConnectionModel
    {
        public string Name { get; set; }

        public int ProtocolVersion { get; set; }
        
        public int PublishPacketsPerSecond { get; set; }
        public int DeliveryPacketsPerSecond { get; set; }
        public IEnumerable<TopicHubModel> Topics { get; set; }
        public IEnumerable<ConnectionQueueInfoModel> Queues { get; set; }



        private void Init(MyServiceBusSessionContext myServiceBusSessionContext, IMyServiceBusSubscriberSession subscriberSession)
        {
            PublishPacketsPerSecond = myServiceBusSessionContext.PublisherInfo.PublishMetricPerSecond.Value;
            DeliveryPacketsPerSecond = myServiceBusSessionContext.MessagesDeliveryMetricPerSecond.Value;

            var now = DateTime.UtcNow;

            Topics = myServiceBusSessionContext.PublisherInfo.GetTopicsToPublish().Select(TopicHubModel.Create);

            if (subscriberSession != null)
            {
                Queues = myServiceBusSessionContext.GetQueueSubscribers().Select(topicQueue => new ConnectionQueueInfoModel
                {
                    Id = topicQueue.Topic.TopicId + ">>>" + topicQueue.QueueId,
                    Leased = topicQueue.GetLeasedQueueSnapshot(subscriberSession).Select(QueueSlice.Create)
                });
            }
            else
            {
                Queues = Array.Empty<ConnectionQueueInfoModel>();
            }

        }

        public static ConnectionModel Create(MyServiceBusTcpContext context)
        {
            var now = DateTime.UtcNow;

            var result = new ConnectionModel
            {
                Name = context.ContextName,
                Ip = context.TcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown",
                ProtocolVersion = context.ProtocolVersion,
                Id = context.Id.ToString(),
                ConnectedTimeStamp = (now - context.SocketStatistic.ConnectionTime).FormatTimeStamp(),
                SentBytes = context.SocketStatistic.Sent,
                ReceivedBytes = context.SocketStatistic.Received,
                SentTimeStamp = (now - context.SocketStatistic.LastSendTime).FormatTimeStamp(),
                ReceiveTimeStamp = (now - context.SocketStatistic.LastReceiveTime).FormatTimeStamp(),
                LastSendDuration = context.SocketStatistic.LastSendToSocketDuration.FormatTimeStamp(),
            };
            
            
            if (context.SessionContext != null)
                result.Init(context.SessionContext, context);

            return result;
        }
        
        public static ConnectionModel Create(GrpcSession context)
        {        
            var now = DateTime.UtcNow;

            var result = new ConnectionModel
            {
                Name = context.Name,
                Topics = context.SessionContext.PublisherInfo.GetTopicsToPublish().Select(TopicHubModel.Create),
                Queues = Array.Empty<ConnectionQueueInfoModel>(),
                PublishPacketsPerSecond = context.SessionContext.PublisherInfo.PublishMetricPerSecond.Value,
                DeliveryPacketsPerSecond = context.SessionContext.MessagesDeliveryMetricPerSecond.Value,
                ProtocolVersion = 0,
                ConnectedTimeStamp = (now - context.Created).FormatTimeStamp(),
                ReceiveTimeStamp = (now - context.LastAccess).FormatTimeStamp()
            };
            
   

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
        public IEnumerable<QueueToPersist> QueueToPersist { get; set; }
        public int TcpConnections { get; set; }
    }


    public static class MonitoringHelpers
    {
        public static IEnumerable<ConsumerModel> GetConsumers(this MyTopic topic)
        {
            return topic.GetQueues().Select(topicQueue => new ConsumerModel
            {
                QueueId = topicQueue.QueueId,
                QueueType = (int)topicQueue.TopicQueueType,
                Connections = topicQueue.SubscribersList.GetCount(),
                QueueSize = topicQueue.GetMessagesCount(),
                LeasedAmount = topicQueue.GetLeasedMessagesCount(),
                ReadySlices = topicQueue.GetReadyQueueSnapshot().Select(QueueSlice.Create),
                ExecutionDuration = topicQueue.GetExecutionDuration()
            });
        }
    }


    public class SnapshotsContract
    {
        public int TopicSnapshotId { get; set; }
        public int TcpConnections { get; set; }
    }
}