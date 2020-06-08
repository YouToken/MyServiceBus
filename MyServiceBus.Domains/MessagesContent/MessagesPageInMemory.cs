using System.Collections.Generic;

namespace MyServiceBus.Domains.MessagesContent
{
    
    public struct MessagesPageId
    {

        public MessagesPageId(long pageId)
        {
            Value = pageId;
        }
    
        public long Value { get; set; }

        public bool Equals(long pageId)
        {
            return Value == pageId;
        }
        
        public bool Equals(MessagesPageId pageId)
        {
            return Value == pageId.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
    public class MessagesPageInMemory
    {

        private readonly Dictionary<long, IMessageContent> _messages;
        
        public MessagesPageId PageId { get;  }

        public MessagesPageInMemory(MessagesPageId pageId)
        {
            PageId = pageId;
            _messages = new Dictionary<long, IMessageContent>();
        }

        public int Count => _messages.Count;
        
        public MessagesPageInMemory(MessagesPageInMemory src)
        {
            PageId = src.PageId;
            
            _messages = new Dictionary<long, IMessageContent>(src._messages);
        }


        public bool Add(IMessageContent message)
        {
            if (!_messages.ContainsKey(message.MessageId))
            {
                _messages.Add(message.MessageId, message);
                return true;
            }

            return false;
        }

        public IEnumerable<IMessageContent> Get()
        {
            return _messages.Values;
        }
        
        public IMessageContent Get(long messageId)
        {
            return _messages.ContainsKey(messageId) ? _messages[messageId] : null;
        }


        public MessagesPageInMemory Clone()
        {
            return new MessagesPageInMemory(this);
        }

    }
    
}