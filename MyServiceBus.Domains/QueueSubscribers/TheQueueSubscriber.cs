using System;
using System.Collections.Generic;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.QueueSubscribers
{
    public enum SubscriberStatus{
        UnLeased, Leased, OnDelivery
    }

    public class TheQueueSubscriber
    {
        private static long _nextConfirmationId;
        public long ConfirmationId { get; }
        
        private readonly TopicQueue _topicQueue;

        public TheQueueSubscriber(IMyServiceBusSubscriberSession session, TopicQueue topicQueue)
        {
            _nextConfirmationId++;
            ConfirmationId = _nextConfirmationId;
            Session = session;
            _topicQueue = topicQueue;
        }

        public IMyServiceBusSubscriberSession Session { get; }

        public readonly QueueWithIntervals LeasedQueue = new (0);

        public IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> MessagesOnDelivery { get; private set; }
        public DateTime OnDeliveryStart { get; private set; }

        public void SendMessages()
        {
            OnDeliveryStart = DateTime.UtcNow;
            Session.SendMessagesAsync(_topicQueue, MessagesOnDelivery, ConfirmationId);
        }
        
        internal void SetOnDeliveryAndSendMessages()
        {
            if (Status != SubscriberStatus.Leased)
                throw new Exception($"Only leased status can be switched to - on Deliver. Now status is: {Status}");
            MessagesOnDelivery = MessagesCollector;
            Status = SubscriberStatus.OnDelivery;
            SendMessages();
        }

        public List<(MessageContentGrpcModel message, int attemptNo)> MessagesCollector { get; private set; }
        
        public void AddMessage(MessageContentGrpcModel messageContent, int attemptNo)
        {
            if (Status != SubscriberStatus.Leased)
                throw new Exception($"Can not add message when Status is: {Status}. Status must Be Leased");
            
            MessagesCollector.Add((messageContent, attemptNo));
            LeasedQueue.Enqueue(messageContent.MessageId);
            MessagesSize += messageContent.Data.Length;
        }
        
        public void SetToLeased()
        {
            if (Status != SubscriberStatus.UnLeased)
                throw new Exception($"Can not change message to status Leased from Status: {Status}.");

            MessagesCollector = new List<(MessageContentGrpcModel message, int attemptNo)>();

            Status = SubscriberStatus.Leased;
        }
        public void SetToUnLeased()
        {
            ClearMessages();
            Status = SubscriberStatus.UnLeased;
        }


        public int MessagesSize { get; private set; }
        public SubscriberStatus Status { get; private set; }
        private void ClearMessages()
        {
            MessagesCollector = null;
            MessagesOnDelivery = Array.Empty<(MessageContentGrpcModel message, int attemptNo)>();
            LeasedQueue.Clear();
            if (MessagesSize == 0)
                return;
            MessagesSize = 0;
        }
 
    }

}