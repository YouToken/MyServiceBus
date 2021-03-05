using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Server.Hubs
{
    
    
    public class MonitoringConnectionTopicContext
    {
        public MyTopic Topic { get; private set; }
        
        public int QueuesSnapshotId { get; set; }
        
        public static MonitoringConnectionTopicContext Create(MyTopic topic)
        {
            return new ()
            {
                Topic = topic
            };
        }
        
    }
}