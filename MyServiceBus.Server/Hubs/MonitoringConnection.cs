using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Server.Hubs
{
    
    
    public class MonitoringConnection
    {
        
        public IClientProxy ClientProxy { get; }
        
        public string Id { get; }


        
        public int TopicsSnapshotId { get; set; }
        public Dictionary<string, MonitoringConnectionTopicContext> TopicContexts = new();

        public object LockObject = new ();

        public MonitoringConnection(string id, IClientProxy clientProxy)
        {
            Id = id;
            ClientProxy = clientProxy;
        }
    }
}