using System.Collections.Generic;
using System.Threading;
using MyServiceBus.Persistence.Grpc;

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

        private readonly Dictionary<long, MessageContentGrpcModel> _messages;

        public MessagesPageId PageId { get;  }

        public MessagesPageInMemory(MessagesPageId pageId)
        {
            PageId = pageId;
            _messages = new Dictionary<long, MessageContentGrpcModel>();
        }

        public int Count => _messages.Count;


        public bool Add(MessageContentGrpcModel message)
        {
            if (!_messages.ContainsKey(message.MessageId))
            {
                _messages.Add(message.MessageId, message);
                return true;
            }

            return false;
        }

        public IEnumerable<MessageContentGrpcModel> Get()
        {
            return _messages.Values;
        }
        
        public MessageContentGrpcModel TryGet(long messageId)
        {
            return _messages.ContainsKey(messageId) ? _messages[messageId] : null;
        }

    }
    
}