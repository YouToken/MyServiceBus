using MyServiceBus.Domains.Topics;
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
        
        internal static TcpConnectionHubModel ToTcpConnectionHubModel(this TcpContext<IServiceBusTcpContract> tcpContext)
        {
            return new ()
            {
                Id = tcpContext.Id.ToString(),
                Ip = tcpContext.TcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown",
            };

        }
    }
}