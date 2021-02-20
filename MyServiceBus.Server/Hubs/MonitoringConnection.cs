using Microsoft.AspNetCore.SignalR;

namespace MyServiceBus.Server.Hubs
{
    public class MonitoringConnection
    {
        
        public IClientProxy ClientProxy { get; }
        
        public string Id { get; }

        public MonitoringConnection(string id, IClientProxy clientProxy)
        {
            Id = id;
            ClientProxy = clientProxy;
        }
    }
}