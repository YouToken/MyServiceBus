using System;
using System.Collections.Generic;
using System.Threading;
using DotNetCoreDecorators;

namespace MyServiceBus.Domains.MessagesContent
{

    public class MessagesContentCache
    {
        private readonly Dictionary<long, MessagesPageInMemory> _messages = new Dictionary<long, MessagesPageInMemory>();
        public string TopicId { get; }
        
        private readonly ReaderWriterLockSlim _lockSlim 
            = new ReaderWriterLockSlim();


        public IReadOnlyList<long> Pages { get; private set; } = Array.Empty<long>();
        
        public MessagesContentCache(string topicId)
        {
            TopicId = topicId;
        }

        public void AddMessages(IEnumerable<IMessageContent> messages)
        {
            
            _lockSlim.EnterWriteLock();
            try
            {
                foreach (var message in messages)
                {
                    var pageId = message.GetMessageContentPageId();

                    if (!_messages.ContainsKey(pageId.Value))
                    {
                        Console.WriteLine($"Added page to MessagesCache for Topic {TopicId} with #"+pageId);
                        _messages.Add(pageId.Value, new MessagesPageInMemory(pageId));

                        Pages = _messages.Keys.AsReadOnlyList();
                    }
                    
                    _messages[pageId.Value].Add(message);
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
            
        }

        public IMessageContent TryGetMessage(in long messageId)
        {
            
            _lockSlim.EnterReadLock();
            try
            {
                var pageId = messageId.GetMessageContentPageId();
                
                if (!_messages.ContainsKey(pageId.Value))
                    return null;

                return _messages[pageId.Value].Get(messageId);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public bool HasCacheLoaded(in long pageId)
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _messages.ContainsKey(pageId);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            } 
        }

        internal int GetMessagesCount()
        {
            
            _lockSlim.EnterReadLock();
            try
            {
                return _messages.Count;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public void UploadPage(MessagesPageInMemory page)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (_messages.ContainsKey(page.PageId.Value))
                    _messages[page.PageId.Value] = page;
                else
                {
                    Console.WriteLine($"Added page during upload procedure to MessagesCache for Topic {TopicId} with #"+page.PageId);
                    _messages.Add(page.PageId.Value, page);

                }
            }
            finally
            {
                Pages = _messages.Keys.AsReadOnlyList();
                _lockSlim.ExitWriteLock();
            }
        }

        private IReadOnlyList<long> GetKeysToGarbageCollect(IDictionary<long, long> activePages)
        {
            List<long> result = null;
            
            _lockSlim.EnterReadLock();
            try
            {
                foreach (var pageId in _messages.Keys)
                {
                    
                    if (!activePages.ContainsKey(pageId))
                    {
                        if (result == null)
                            result = new List<long>();
                    
                        result.Add(pageId);
                    }
                }

            }
            finally
            {
                _lockSlim.ExitReadLock();
            }            

            return result;
        }

        public void GarbageCollect(IDictionary<long, long> activePages)
        {
            
            var pagesToGc = GetKeysToGarbageCollect(activePages);
                            
            if (pagesToGc == null)
                return;
            
            _lockSlim.EnterWriteLock();
            try
            {

                foreach (var pageToGc in pagesToGc)
                {
                    Console.WriteLine($"Garbage collecting page for Topic {TopicId} from MessagesCache with #"+pageToGc);
                    _messages.Remove(pageToGc);
                }
            }
            finally
            {
                Pages = _messages.Keys.AsReadOnlyList();
                _lockSlim.ExitWriteLock();
            }
        }
        
    }

    public class MessageContentCacheByTopic
    {
        private readonly Dictionary<string, MessagesContentCache> _messagesCache 
            = new Dictionary<string, MessagesContentCache>();

        private readonly ReaderWriterLockSlim _lockSlim 
            = new ReaderWriterLockSlim();

        public MessagesContentCache Create(string topicId)
        {
            _lockSlim.EnterWriteLock();

            try
            {
                if (!_messagesCache.ContainsKey(topicId))
                {
                    var result = new MessagesContentCache(topicId);
                    _messagesCache.Add(topicId, result);
                    return result;
                }

                return _messagesCache[topicId];
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            } 
        }
        
        public MessagesContentCache TryGetTopic(string topicId)
        {
            _lockSlim.EnterReadLock();
            try
            {

                if (_messagesCache.ContainsKey(topicId))
                    return _messagesCache[topicId];

                return null;

            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
           
        }


        public int GetMessagesAmount(string topicId)
        {
            var topic = TryGetTopic(topicId);
            return topic?.GetMessagesCount() ?? 0;
        }


        public IReadOnlyList<long> GetPagesByTopic(string topicId)
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _messagesCache.ContainsKey(topicId) 
                    ? _messagesCache[topicId].Pages 
                    : Array.Empty<long>();
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public MessagesContentCache GetTopic(string topicId)
        {
            _lockSlim.EnterReadLock();
            try
            {
                if (_messagesCache.ContainsKey(topicId))
                    return _messagesCache[topicId];
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }

            throw new Exception($"Messages Cache for topic {topicId} is not found");
        }


        public void GarbageCollect(string topicId, IDictionary<long, long> activePages)
        {
            _lockSlim.EnterReadLock();
            try
            {
                if (!_messagesCache.ContainsKey(topicId))
                    return;

                _messagesCache[topicId].GarbageCollect(activePages);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

    }
    
}