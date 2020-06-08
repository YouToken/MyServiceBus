using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;

namespace MyServiceBus.Domains.Persistence
{
    public interface IMessagesPersistentStorage
    {
        Task SaveAsync(string topicId, IReadOnlyList<IMessageContent> messages);
        
        /// <summary>
        /// Read the chunk of messages which contains the messageId
        /// </summary>
        /// <param name="topicId">topicId we are reading from</param>
        /// <param name="pageId">messageId</param>
        /// <returns></returns>
        ValueTask<MessagesPageInMemory> GetMessagesPageAsync(string topicId, MessagesPageId pageId);


        ValueTask GarbageCollectAsync(string topicId, long messageId);
    }
    
}