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
        public readonly Dictionary<string, MonitoringConnectionTopicContext> TopicContexts = new();

        public readonly object LockObject = new ();


        #region LastQueueGraphSentSnapshot

        private readonly Dictionary<string, Dictionary<string, long>> _lastQueueGraphSnapshot
            = new Dictionary<string, Dictionary<string, long>>();

        internal void SetLastQueueGraphSentSnapshot(string topicId, string queueId, long snapshotId)
        {

            lock (LockObject)
            {
                if (!_lastQueueGraphSnapshot.ContainsKey(topicId))
                    _lastQueueGraphSnapshot.Add(topicId, new Dictionary<string, long>());

                var topicSnapshot = _lastQueueGraphSnapshot[topicId];

                if (topicSnapshot.ContainsKey(queueId))
                    topicSnapshot[queueId] = snapshotId;
                else
                    topicSnapshot.Add(queueId, snapshotId);
            }
            
        }

        internal long GetLastQueueGraphSendSnapshot(string topicId, string queueId)
        {
            lock (LockObject)
            {
                if (_lastQueueGraphSnapshot.TryGetValue(topicId, out var topicSnapshot))
                    if (topicSnapshot.TryGetValue(queueId, out var result))
                        return result;

                return -100;
            }

        }
        
        
        #endregion

        public MonitoringConnection(string id, IClientProxy clientProxy)
        {
            Id = id;
            ClientProxy = clientProxy;
        }
    }
}