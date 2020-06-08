using System.Collections.Generic;
using DotNetCoreDecorators;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Domains.Tests.Utils
{
    public class MessagesToPersistQueueForTests : IMessagesToPersistQueue
    {
        
        private readonly MessagesToPersistQueue _messagesToPersistQueue = new MessagesToPersistQueue();
        
        
        public readonly Dictionary<string, List<MessageContent>> MessagesToPersist 
            = new Dictionary<string, List<MessageContent>>();
        
        public void EnqueueToPersist(string topicId, IEnumerable<MessageContent> messages)
        {

            var messagesAsReadOnlyList = messages.AsReadOnlyList();
            
            lock (MessagesToPersist)
            {
                if (!MessagesToPersist.ContainsKey(topicId))
                    MessagesToPersist.Add(topicId, new List<MessageContent>());
                    
                MessagesToPersist[topicId].AddRange(messagesAsReadOnlyList);
            }
                
            _messagesToPersistQueue.EnqueueToPersist(topicId, messagesAsReadOnlyList);
        }

        public IReadOnlyList<MessageContent> GetMessagesToPersist(string topicId)
        {
            return _messagesToPersistQueue.GetMessagesToPersist(topicId);
        }

        public IReadOnlyList<(string topic, int count)> GetMessagesToPersistCount()
        {
            throw new System.NotImplementedException();
        }
    }
}