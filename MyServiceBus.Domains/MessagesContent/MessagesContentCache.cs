using System;
using System.Collections.Generic;
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
                        Pages = _messages.Keys.AsReadOnlyList();
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
                Pages = _messages.Keys.AsReadOnlyList();
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
                    Pages = _messages.Keys.AsReadOnlyList();
                }
            }
        }
        
    }

    public class MessageContentCacheByTopic
    {
        private Dictionary<string, MessagesContentCache> _messagesCache 
            = new ();

        private readonly object _lockObject 
            = new ();

        public MessagesContentCache Create(string topicId)
        {

            lock (_lockObject)
            {
                var (added, newDictionary, value) = _messagesCache.AddIfNotExistsByCreatingNewDictionary(topicId,
                    () => new MessagesContentCache(topicId));

                if (added)
                    _messagesCache = newDictionary;

                return value;
            }

        }

        public MessagesContentCache TryGetTopic(string topicId)
        {
            return _messagesCache.TryGetValue(topicId, out var result) 
                ? result 
                : null;
        }

        public int GetMessagesAmount(string topicId)
        {
            var cacheByTopic = TryGetTopic(topicId);
            return cacheByTopic?.GetMessagesCount() ?? 0;
        }

        public IReadOnlyList<long> GetPagesByTopic(string topicId)
        {
            return _messagesCache.TryGetValue(topicId, out var result) 
                ? result.Pages 
                : Array.Empty<long>();
        }

        public MessagesContentCache GetTopic(string topicId)
        {
            if (_messagesCache.TryGetValue(topicId, out var result))
                return result;

            throw new Exception($"Messages Cache for topic {topicId} is not found");
        }

        public void GarbageCollect(string topicId, IDictionary<long, long> activePages)
        {
            if (_messagesCache.TryGetValue(topicId, out var cache))
                cache.GarbageCollect(activePages);
        }

    }
    
}