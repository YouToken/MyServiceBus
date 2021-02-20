using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Server.Tcp;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.Server.Hubs
{
    public static class MonitoringHubExtensions
    {

        public static Task SendInitAsync(this IClientProxy clientProxy)
        {
            var initModel = new InitHubResponseModel
            {
                Version = ServiceLocator.AppVersion
            };
            return clientProxy.SendAsync("init", initModel);
        }

        public static Task SendTopicsAsync(this IClientProxy clientProxy, IEnumerable<MyTopic> topics)
        {
            return clientProxy.SendAsync("topics", topics.Select(itm => itm.ToTopicHubModel()).OrderBy(itm => itm.Id));
        }


        public static Task SendQueuesAsync(this IClientProxy clientProxy, IEnumerable<MyTopic> topics)
        {
            var contract = new Dictionary<string, List<TopicQueueHubModel>>();

            foreach (var topic in topics)
            {
                contract.Add(topic.TopicId, new List<TopicQueueHubModel>());

                foreach (var topicQueue in topic.GetQueues())
                {
                    contract[topic.TopicId].Add(TopicQueueHubModel.Create(topicQueue));
                }
            }
            return clientProxy.SendAsync("queues", contract);
        }


        public static Task SendTopicMetricsAsync(this IClientProxy clientProxy, IEnumerable<MyTopic> topics)
        {
            
            var contract = new Dictionary<string, IReadOnlyList<int>>();

            foreach (var topic in topics)
                contract.Add(topic.TopicId, ServiceLocator.MessagesPerSecondByTopic.GetRecordsPerSecond(topic.TopicId));
            
            return clientProxy.SendAsync("topic-metrics", contract);
        }


        public static Task SendQueueMetricsAsync(this IClientProxy clientProxy, IEnumerable<MyTopic> topics)
        {
            var contract = new Dictionary<string, IReadOnlyList<int>>();

            foreach (var topic in topics)
            {
                foreach (var topicQueue in topic.GetQueues())
                {
                    contract.Add(topic.TopicId+"-"+topicQueue.QueueId, topicQueue.GetExecutionDuration());
                }
            }
            
            return clientProxy.SendAsync("queue-metrics", contract);
        }

        public static Task SendConnectionsAsync(this MonitoringConnection connection)
        {
            var connections = ServiceLocator.TcpServer.GetConnections();
            return connection.ClientProxy.SendAsync("connections", connections.Select(conn => conn.ToTcpConnectionHubModel()));
        }

    }
}