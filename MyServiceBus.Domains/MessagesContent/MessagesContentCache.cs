using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCoreDecorators;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.MessagesContent
{

    public class MessagesContentCache
    {
        private Dictionary<long, MessagesPageInMemory> _messages = new ();
        public string TopicId { get; }
        
        private readonly object _lockObject = new ();

        public IReadOnlyList<long> Pages { get; private set; } = Array.Empty<long>();
        
        public MessagesContentCache(string topicId)
        {
            TopicId = topicId;
        }

        public void AddMessages(IEnumerable<MessageContentGrpcModel> messages)
        {

            lock (_lockObject)
            {
                foreach (var message in messages)
                {
                    var pageId = message.GetMessageContentPageId();

                    var result =
                        _messages.AddIfNotExistsByCreatingNewDictionary(pageId.Value,
                            () => new MessagesPageInMemory(pageId));

                    if (result.added)
                    {
                        Console.WriteLine($"Added page to MessagesCache for Topic {TopicId} with #"+pageId);
                        _messages = result.newDictionary;
                        Pages = _messages.Keys.OrderBy(key => key).AsReadOnlyList();
                    }
                    
                    result.value.Add(message);
                    
                }
            }
            
        }

        public MessageContentGrpcModel TryGetMessage(in long messageId)
        {
            var pageId = messageId.GetMessageContentPageId();

            return _messages.TryGetValue(pageId.Value, out var page) 
                ? page.Get(messageId) 
                : null;
        }

        public bool HasCacheLoaded(in long pageId)
        {
            return _messages.ContainsKey(pageId);
 
        }

        internal int GetMessagesCount()
        {
            return _messages.Count;
        }

        public void UploadPage(MessagesPageInMemory page)
        {

            lock (_lockObject)
            {
                if (_messages.ContainsKey(page.PageId.Value))
                {
                    _messages[page.PageId.Value] = page;
                    return;
                }
                
                _messages.AddIfNotExistsByCreatingNewDictionary(page.PageId.Value, ()=>page);
                Pages = _messages.Keys.OrderBy(key => key).AsReadOnlyList();
            }

        }

        private IReadOnlyList<long> GetKeysToGarbageCollect(IDictionary<long, long> activePages)
        {
            List<long> result = null;
            
            foreach (var pageId in _messages.Keys)
            {
                if (!activePages.ContainsKey(pageId))
                {
                    result ??= new List<long>();
                    result.Add(pageId);
                }
            }

            return result;
        }

        public void GarbageCollect(IDictionary<long, long> activePages)
        {
            var pagesToGc = GetKeysToGarbageCollect(activePages);
                            
            if (pagesToGc == null)
                return;


            lock (_lockObject)
            {
                try
                {
                    foreach (var pageToGc in pagesToGc)
                    {
                        Console.WriteLine($"Garbage collecting page for Topic {TopicId} from MessagesCache with #"+pageToGc);
                        var result = _messages.RemoveIfExistsByCreatingNewDictionary(pageToGc, (k1, k2) => k1 == k2);

                        if (result.removed)
                            _messages = result.result;
                    }
                }
                finally
                {
                    Pages = _messages.Keys.OrderBy(key => key).AsReadOnlyList();
                }
            }
        }
        
    }

    
}