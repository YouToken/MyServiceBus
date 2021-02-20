using System;
using System.Collections.Generic;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Server.Tcp;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

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
                Topics = tcpContext.Session == null 
                    ? Array.Empty<string>() 
                    : tcpContext.Session.GetTopicsToPublish()
            };
        }
    }
}