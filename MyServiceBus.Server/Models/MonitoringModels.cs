using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Server.Tcp;
using MyServiceBus.TcpContracts;

namespace MyServiceBus.Server.Models
{
    
    
    public class MonitoringModel
    {
        
        private static readonly SystemStatusMonitoringModel SystemDefault = SystemStatusMonitoringModel.CreateDefault();
        
        public TopicsMonitoringModel Topics { get; set; }
        public Dictionary<string, QueuesMonitoringModel> Queues { get; set; }
        
        public SessionsMonitoringModel Sessions { get; set; }
        public SystemStatusMonitoringModel System { get; set; } = SystemDefault;

        public static MonitoringModel Create()
        {
            var (topics, snapshotId) = ServiceLocator.TopicsList.GetWithSnapshotId();
            
            return new MonitoringModel
            {
                Topics = TopicsMonitoringModel.Create(snapshotId, topics),
                Queues = topics.ToDictionary(topic => topic.TopicId, QueuesMonitoringModel.Create),
                Sessions = SessionsMonitoringModel.Create()
                
            };
        }
    }
    

    public class QueueIndexRangeMonitoringModel
    {
        public long FromId { get; set; }
        public long ToId { get; set; }

        public static QueueIndexRangeMonitoringModel Create(IQueueIndexRange src)
        {
            return new()
            {
                FromId = src.FromId,
                ToId = src.ToId
            };
        }
    }


    public class TopicsMonitoringModel
    {
        public int SnapshotId { get; set; }
        
        public IReadOnlyList<TopicMonitoringModel> items { get; set; }

        public static TopicsMonitoringModel Create(int snapshotId, IEnumerable<MyTopic> topics)
        {
            return new TopicsMonitoringModel
            {
                SnapshotId = snapshotId,
                items = topics.Select(TopicMonitoringModel.Create).ToList()
            };
        }
    }
    

    public class TopicMonitoringModel
    {
        public string Id { get; set; }
        public long MessageId { get; set; }    
        public int PacketPerSec { get; set; }
        public int MessagesPerSec { get; set; }
        public int PersistSize { get; set; }
        public IEnumerable<int> PublishHistory { get; set; }
        public IEnumerable<TopicPageModel> Pages { get; set; }
        public static TopicMonitoringModel Create(MyTopic topic)
        {
            return new TopicMonitoringModel
            {
                Id = topic.TopicId,
                MessageId = topic.MessageId.Value,
                PacketPerSec = topic.RequestsPerSecond,
                MessagesPerSec = topic.MessagesPerSecond,
                PersistSize = ServiceLocator.MessagesToPersistQueue.GetAmount(topic.TopicId),
                Pages = topic.MessagesContentCache.GetPages().Select(itm => new TopicPageModel
                {
                    Id = itm.no,
                    Percent = itm.percent,
                    Size = itm.size
                }),
                PublishHistory = ServiceLocator.MessagesPerSecondByTopic.GetRecordsPerSecond(topic.TopicId),
            };
        }
    }

    public class TopicPageModel
    {
        public long Id { get; set; }
        public int Percent { get; set; }
        public long Size { get; set; }
    }


    public class QueuesMonitoringModel
    {
        public int SnapshotId { get; set; }
        public IEnumerable<QueueMonitoringModel> Queues { get; set; }

        public static QueuesMonitoringModel Create(MyTopic topic)
        {
            var (queues, snapshotId) = topic.GetQueuesWithSnapshotId();
            return new QueuesMonitoringModel
            {
                SnapshotId = snapshotId,
                Queues = queues.Select(QueueMonitoringModel.Create)
            };
        }
    }
    public class QueueMonitoringModel
    {
        public string Id { get; set; }
        public byte QueueType { get; set; }
        public long Size { get; set; }
        public IEnumerable<QueueIndexRangeMonitoringModel> Data { get; set; }

        public static QueueMonitoringModel Create(TopicQueue queue)
        {

            var queueSnapshot = queue.GetReadyQueueSnapshot();

            return new QueueMonitoringModel
            {
                Id = queue.QueueId,
                Size = queue.GetMessagesCount(),
                QueueType = (byte)queue.TopicQueueType,
                Data = queueSnapshot.Select(QueueIndexRangeMonitoringModel.Create)
            };
        }
    }

    public class SessionsMonitoringModel
    {
        public long SnapshotId { get; set; }
        public IEnumerable<SessionMonitoringModel> Items { get; set; }
        public static SessionsMonitoringModel Create()
        {
            var sessions = ServiceLocator.TcpServer.GetConnections().Cast<MyServiceBusTcpContext>().ToList();

            var snapshotId = sessions.Count == 0 ? -1 : sessions.Max(itm => itm.Id);
            return new SessionsMonitoringModel
            {
                SnapshotId = snapshotId,
                Items = sessions.Select(SessionMonitoringModel.Create)
            };
        }
    }


    public class SessionMonitoringModel
    {
        public long Id { get; set; }
        public string Ip { get; set; }
        public string Version { get; set; }
        
        public string Name { get; set; }
        
        public string Connected { get; set; }
        
        public string LastIncoming { get; set; }
        
        public long ReadSize { get; set; }
        
        public long WrittenSize { get; set; }
        
        public long ReadPerSec { get; set; }
        
        public long WrittenPerSec { get; set; }
        
        public Dictionary<string, byte> Publishers { get; set; } 
        
        public IEnumerable<SubscriberMonitoringModel> Subscribers { get; set; }


        public static SessionMonitoringModel Create(MyServiceBusTcpContext ctx)
        {
            var now = DateTime.UtcNow;
            
            return new SessionMonitoringModel
            {
                Id = ctx.Id,
                Ip = GetIp(ctx),
                Version = ctx.ClientVersion,
                Name = ctx.Name,
                Connected = (now - ctx.SocketStatistic.ConnectionTime).ToString("G"),
                ReadSize = ctx.SocketStatistic.Received,
                WrittenSize = ctx.SocketStatistic.Sent,
                ReadPerSec = ctx.SocketStatistic.ReceivedPerSecond,
                WrittenPerSec = ctx.SocketStatistic.SentPerSecond,
                LastIncoming = (now - ctx.SocketStatistic.LastReceiveTime).ToString("G"),
                Publishers = GetActivities(now, ctx.SessionContext.PublisherInfo.GetTopicsToPublish()),
                Subscribers = ctx.SessionContext.GetQueueSubscribers().Select(itm => SubscriberMonitoringModel.Create(itm, GetActive(now, ctx.SessionContext.SubscriberInfo.GetSubscriberLastPacketDateTime(itm.Topic.TopicId, itm.QueueId))))
            };
        }


        private static Dictionary<string, byte> GetActivities(DateTime now, IReadOnlyDictionary<string, DateTime> src)
        {
            return src.ToDictionary(
                itm => itm.Key,
                itm => GetActive(now, itm.Value));
        }

        private static byte GetActive(DateTime now, DateTime lastActive)
        {
            return (now - lastActive).TotalSeconds < 2 ? (byte)1 : (byte)0;
        }

        private static string GetIp(MyServiceBusTcpContext ctx)
        {
            try
            {
                return ctx.TcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

    }

    public class SubscriberMonitoringModel
    {
        public string TopicId { get; set; }
        public string QueueId { get; set; }
        
        public byte Active { get; set; }
        public IEnumerable<int> DeliveryHistory { get; set; }

        public static SubscriberMonitoringModel Create(TopicQueue queue, byte active)
        {
            return new SubscriberMonitoringModel
            {
                TopicId = queue.Topic.TopicId,
                QueueId = queue.QueueId,
                Active = active,
                DeliveryHistory = queue.GetExecutionDuration()
            };
        }
    }

    public class SystemStatusMonitoringModel {
        public int Usedmem { get; set; }
        public int Totalmem { get; set; }

        public static SystemStatusMonitoringModel CreateDefault()
        {
            return new SystemStatusMonitoringModel
            {
                Totalmem = 0,
                Usedmem = 0
            };
        }
    }
}