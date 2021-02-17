using System.Collections.Generic;
using DotNetCoreDecorators;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.Tests.Utils
{
    public class MessagesToPersistQueueForTests : IMessagesToPersistQueue
    {
        
        private readonly MessagesToPersistQueue _messagesToPersistQueue = new MessagesToPersistQueue(new MetricsCollectorMock());
        
        
        public readonly Dictionary<string, List<MessageContentGrpcModel>> MessagesToPersist 
            = new Dictionary<string, List<MessageContentGrpcModel>>();
        
        public void EnqueueToPersist(string topicId, IEnumerable<MessageContentGrpcModel> messages)
        {

            var messagesAsReadOnlyList = messages.AsReadOnlyList();
            
            lock (MessagesToPersist)
            {
                if (!MessagesToPersist.ContainsKey(topicId))
                    MessagesToPersist.Add(topicId, new List<MessageContentGrpcModel>());
                    
                MessagesToPersist[topicId].AddRange(messagesAsReadOnlyList);
            }
                
            _messagesToPersistQueue.EnqueueToPersist(topicId, messagesAsReadOnlyList);
        }

        public IReadOnlyList<MessageContentGrpcModel> GetMessagesToPersist(string topicId)
        {
            return _messagesToPersistQueue.GetMessagesToPersist(topicId);
        }

        public IReadOnlyList<(string topic, int count)> GetMessagesToPersistCount()
        {
            throw new System.NotImplementedException();
        }
    }
}