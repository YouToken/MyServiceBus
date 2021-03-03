using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MyServiceBus.Server.Tcp;

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

        public static ValueTask SendTopicsAsync(this MonitoringConnection connection)
        {
            var snapshot = ServiceLocator.TopicsList.GetWithSnapshotId();

            lock (connection.LockObject)
            {
                if (snapshot.snapshotId == connection.TopicsSnapshotId)
                    return new ValueTask();

                connection.TopicsSnapshotId = snapshot.snapshotId;
                connection.TopicContexts.Clear();

                foreach (var myTopic in snapshot.topics)
                    connection.TopicContexts.Add(myTopic.TopicId, MonitoringConnectionTopicContext.Create(myTopic));
            }

            var task = connection.ClientProxy.SendAsync("topics", snapshot.topics.Select(itm => itm.ToTopicHubModel()).OrderBy(itm => itm.Id));
            return new ValueTask(task);
        }

        public static ValueTask SendQueuesAsync(this MonitoringConnection connection)
        {
            Dictionary<string, List<TopicQueueHubModel>> contract = null;

            lock (connection.LockObject)
            {
                foreach (var topicCtx in connection.TopicContexts.Values)
                {
                    var snapshot = topicCtx.Topic.GetQueuesWithSnapshotId();
                    
                    if (topicCtx.QueuesSnapshotId == snapshot.snapshotId)
                        continue;

                    topicCtx.QueuesSnapshotId = snapshot.snapshotId;

                    contract ??= new Dictionary<string, List<TopicQueueHubModel>>();
                    contract.Add(topicCtx.Topic.TopicId, new List<TopicQueueHubModel>());

                    foreach (var topicQueue in topicCtx.Topic.GetQueues())
                    {
                        contract[topicCtx.Topic.TopicId].Add(TopicQueueHubModel.Create(topicQueue));
                    }
                }
            }
 
            return contract != null 
                ? new ValueTask(connection.ClientProxy.SendAsync("queues", contract)) 
                : new ValueTask();
        }


        public static Task SendTopicGraphAsync(this MonitoringConnection connection)
        {
            
            var contract = ServiceLocator.TopicsList.Get()
                .ToDictionary(topic => topic.TopicId, 
                    topic => ServiceLocator.MessagesPerSecondByTopic.GetRecordsPerSecond(topic.TopicId));

            return connection.ClientProxy.SendAsync("topic-performance-graph", contract);
        }

        public static Task SendQueueGraphAsync(this MonitoringConnection connection)
        {
            var contract = new Dictionary<string, IReadOnlyList<int>>();

            foreach (var topic in ServiceLocator.TopicsList.Get())
            {
                foreach (var topicQueue in topic.GetQueues())
                {
                    contract.Add(topic.TopicId+"-"+topicQueue.QueueId, topicQueue.GetExecutionDuration());
                }
            }
            
            return connection.ClientProxy.SendAsync("queue-duration-graph", contract);
        }

        public static Task SendConnectionsAsync(this MonitoringConnection connection)
        {
            var connections = ServiceLocator
                .TcpServer
                .GetConnections()
                .Cast<MyServiceBusTcpContext>()
                .OrderBy(itm => itm.ContextName.ToLowerInvariant());
            
            return connection.ClientProxy.SendAsync("connections", connections.Select(conn => conn.ToTcpConnectionHubModel()));
        }


        public static Task SendTopicMetricsAsync(this MonitoringConnection connection)
        {
            var contract = ServiceLocator.TopicsList.Get().Select(TopicMetricsHubModel.Create);
            return connection.ClientProxy.SendAsync("topic-metrics", contract);
        }


        public static Task SendPersistentQueueAsync(this MonitoringConnection connection)
        {
            var contract = ServiceLocator.MessagesToPersistQueue.GetMessagesToPersistCount().Select(itm =>
            new PersistentQueueHubModel
            {
                Id = itm.topic,
                Size = itm.count
            });

            return connection.ClientProxy.SendAsync("persist-queue", contract);
        }

    }
}