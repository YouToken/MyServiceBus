using System;

namespace MyServiceBus.Domains.MessagesContent
{
    public interface IMessageContent
    {
        long MessageId { get; }
        byte[] Data { get; }
        DateTime Created { get; }
    }
    
    public class MessageContent : IMessageContent
    {
        public long MessageId { get; private set; }

        public byte[] Data { get; private set; }
        

        public DateTime Created { get; private set; }

        public static MessageContent Create(long messageId, int attemptNo, byte[] data, DateTime created)
        {
            return new MessageContent
            {
                MessageId = messageId,
                Data = data,
                Created = created,
            };
        } 
    }
    
}