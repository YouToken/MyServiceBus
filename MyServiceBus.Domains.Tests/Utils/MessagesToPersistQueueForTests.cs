using System.Collections.Generic;
using DotNetCoreDecorators;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.Tests.Utils
{
    public class MessagesToPersistQueueForTests : IMessagesToPersistQueue
    {
        
        private readonly MessagesToPersistQueue _messagesToPersistQueue = new (new MetricsCollectorMock());

        private readonly Dictionary<string, List<MessageContentGrpcModel>> _messagesToPersist 
            = new ();
        
        public void EnqueueToPersist(string topicId, IEnumerable<MessageContentGrpcModel> messages)
        {

            var messagesAsReadOnlyList = messages.AsReadOnlyList();
            
            lock (_messagesToPersist)
            {
                if (!_messagesToPersist.ContainsKey(topicId))
                    _messagesToPersist.Add(topicId, new List<MessageContentGrpcModel>());
                    
                _messagesToPersist[topicId].AddRange(messagesAsReadOnlyList);
            }
                
            _messagesToPersistQueue.EnqueueToPersist(topicId, messagesAsReadOnlyList);
        }

        public IReadOnlyList<MessageContentGrpcModel> GetMessagesToPersist(string topicId)
        {
            return _messagesToPersistQueue.GetMessagesToPersist(topicId);
        }

        public IReadOnlyList<(string topic, int count)> GetMessagesToPersistCount()
        {
            return _messagesToPersistQueue.GetMessagesToPersistCount();
        }

        public int Count => _messagesToPersistQueue.Count;
    }
}