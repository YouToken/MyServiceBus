using System;
using System.Linq;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Server.Models;
using MyServiceBus.Server.Tcp;


namespace MyServiceBus.Server.Hubs
{
    public static class HubModelsMappers
    {

        internal static TopicHubModel ToTopicHubModel(this MyTopic topic)
        {
            return new ()
            {
                Id = topic.TopicId,
                Pages = topic.MessagesContentCache.Pages
            };
        }

        
        internal static TcpConnectionHubModel ToTcpConnectionHubModel(this MyServiceBusTcpContext tcpContext)
        {
            return new ()
            {
                Id = tcpContext.Id.ToString(),
                Name = tcpContext.ContextName,
                Ip = tcpContext.TcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown",
                Connected = (DateTime.UtcNow - tcpContext.SocketStatistic.ConnectionTime).FormatTimeStamp(),
                Recv = (DateTime.UtcNow - tcpContext.SocketStatistic.LastReceiveTime).FormatTimeStamp(),
                ReadBytes = tcpContext.SocketStatistic.Received,
                SentBytes = tcpContext.SocketStatistic.Sent,
                ProtocolVersion = tcpContext.Session?.ProtocolVersion ?? 0,
                Topics =  tcpContext.Session == null 
                    ? Array.Empty<string>() 
                    : tcpContext.Session.GetTopicsToPublish(),
                Queues = tcpContext.Session == null 
                    ? Array.Empty<TcpConnectionSubscribeHubModel>() 
                    : tcpContext.Session.GetQueueSubscribers().Select(queue => TcpConnectionSubscribeHubModel.Create(queue, queue.GetLeasedQueueSnapshot(tcpContext)))

            };
        }
    }
}