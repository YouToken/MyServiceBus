using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MyServiceBus.Server.Hubs
{
    public class MonitoringHub : Hub
    {
        
        private static readonly MonitoringConnectionsList Connections =
            new ();

        public static void BroadCasMetrics()
        {
            var connections = Connections.GetAll();

            var topics = ServiceLocator.TopicsList.Get();

            foreach (var connection in connections)
                connection.ClientProxy.SendTopicMetricsAsync(topics);
            
            foreach (var connection in connections)
                connection.ClientProxy.SendQueueMetricsAsync(topics);
        } 


        
        public override async Task OnConnectedAsync()
        {
            var newConnection = new MonitoringConnection(Context.ConnectionId, Clients.Caller);
            Connections.Add(newConnection);
            Console.WriteLine("Monitoring Connection: "+Context.ConnectionId);
            await InitConnectionAsync(newConnection);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("Monitoring Connection dropped: "+Context.ConnectionId);
            Connections.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }


        private async Task InitConnectionAsync(MonitoringConnection connection)
        {
            await connection.ClientProxy.SendInitAsync();

            var topics = ServiceLocator.TopicsList.Get();
            await connection.ClientProxy.SendTopicsAsync(topics);
            await connection.ClientProxy.SendQueuesAsync(topics);
            await connection.ClientProxy.SendTopicMetricsAsync(topics);
            await connection.ClientProxy.SendQueueMetricsAsync(topics);
            await connection.SendConnectionsAsync();
        } 
    }
}