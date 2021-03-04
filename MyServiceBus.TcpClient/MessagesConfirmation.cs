using System;
using System.Collections.Generic;
using MyServiceBus.Abstractions;

namespace MyServiceBus.TcpClient
{
    public class MessagesConfirmationContext : IConfirmationContext
    {
        private readonly Action<IConfirmationContext, IEnumerable<long>> _sendConfirmationPacket;
        public string TopicId { get; }
        public string QueueId { get; }
        public long ConfirmationId { get; }

        public MessagesConfirmationContext(string topicId, string queueId, long confirmationId, 
            Action<IConfirmationContext, IEnumerable<long>> sendConfirmationPacket)
        {
            _sendConfirmationPacket = sendConfirmationPacket;
            TopicId = topicId;
            QueueId = queueId;
            ConfirmationId = confirmationId;
        }
        
        public void ConfirmMessages(IEnumerable<long> messagesToConfirm)
        {
            _sendConfirmationPacket(this, messagesToConfirm);
        }
    }
}