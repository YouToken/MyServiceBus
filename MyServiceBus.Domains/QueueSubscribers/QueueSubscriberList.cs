using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCoreDecorators;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.QueueSubscribers
{
    
    
    public enum SubscriberStatus{
        NonActive, Leased, OnDelivery
    
    }


    public class TheQueueSubscriber
    {

        private static long _nextConfirmationId;
        private readonly TopicQueue _topicQueue;

        public TheQueueSubscriber(IQueueSubscriber subscriber, TopicQueue topicQueue)
        {
            QueueSubscriber = subscriber;
            _topicQueue = topicQueue;
        }

        public IQueueSubscriber QueueSubscriber { get; }

        private List<(MessageContentGrpcModel message, int attemptNo)> _onDelivery = new ();

        public IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> MessagesOnDelivery => _onDelivery;
        
        public long ConfirmationId { get; private set; }
        
        public DateTime OnDeliveryStart { get; private set; }

        internal void SetOnDeliveryAndSendMessages()
        {
            if (Status != SubscriberStatus.Leased)
                throw new Exception($"Only leased status can be switched to - on Deliver. Now status is: {Status}");
            
            _nextConfirmationId++;
            ConfirmationId = _nextConfirmationId;
            Status = SubscriberStatus.OnDelivery;
            OnDeliveryStart = DateTime.UtcNow;
            QueueSubscriber.SendMessagesAsync(_topicQueue, MessagesOnDelivery, ConfirmationId);
        }


        public void AddMessage(MessageContentGrpcModel messageContent, int attemptNo)
        {
            if (Status != SubscriberStatus.Leased)
                throw new Exception($"Can not add message when Status is: {Status}. Status must Be Leased");
            
            _onDelivery.Add((messageContent, attemptNo));
            MessagesSize += messageContent.Data.Length;
        }

        public void SetToLeased()
        {
            if (Status != SubscriberStatus.NonActive)
                throw new Exception($"Can not change message to status Leased from Status: {Status}.");

            Status = SubscriberStatus.Leased;
        }

        public void SetToUnLeased()
        {
            ClearMessages();
            Status = SubscriberStatus.NonActive;
        }


        public int MessagesSize { get; private set; }
        
        public SubscriberStatus Status { get; private set; }

        public void ClearMessages()
        {
            if (MessagesSize == 0)
                return;
            MessagesSize = 0;
            _onDelivery = new List<(MessageContentGrpcModel, int)>();
        }

    }


    public class QueueSubscribersList
    {
        public TopicQueue TopicQueue { get; }

        public QueueSubscribersList(TopicQueue topicQueue, object lockObject)
        {
            TopicQueue = topicQueue;
            _lockObject = lockObject;
        }
        
        private readonly Dictionary<string, TheQueueSubscriber> _subscribers 
            = new Dictionary<string, TheQueueSubscriber>();

        private IReadOnlyList<TheQueueSubscriber>
            _subscribersAsReadOnlyList = Array.Empty<TheQueueSubscriber>();


        
        private readonly Dictionary<long, TheQueueSubscriber> _onDelivery 
            = new Dictionary<long, TheQueueSubscriber>();



        private readonly object _lockObject;

        public void Subscribe(IQueueSubscriber subscriber)
        {
            lock (_lockObject)
            {
                
                if (_subscribers.ContainsKey(subscriber.SubscriberId))
                    throw new Exception($"Subscriber to topic: {TopicQueue.Topic}  and queue: {TopicQueue.QueueId} is already exists");


                var dataCollector = new TheQueueSubscriber(subscriber, TopicQueue);
                
                _subscribers.Add(subscriber.SubscriberId, dataCollector);

                _subscribersAsReadOnlyList = _subscribers.Values.AsReadOnlyList();

            }
        }
        


        public TheQueueSubscriber Unsubscribe(IQueueSubscriber subscriber)
        {
            lock (_lockObject)
            {

                if (!_subscribers.ContainsKey(subscriber.SubscriberId))
                    return null;

                var itemToRemove = _subscribers[subscriber.SubscriberId];

                _subscribers.Remove(subscriber.SubscriberId);
                
                _subscribersAsReadOnlyList = _subscribers.Values.AsReadOnlyList();

                if (_onDelivery.ContainsKey(itemToRemove.ConfirmationId))
                    _onDelivery.Remove(itemToRemove.ConfirmationId);

                return itemToRemove;
            }

        }

        public TheQueueSubscriber LeaseSubscriber()
        {
            lock (_lockObject)
            {
                var readyToBeLeased
                    = _subscribers
                        .Values
                        .FirstOrDefault(itm => itm.Status == SubscriberStatus.NonActive);

                if (readyToBeLeased == null)
                    return null;
                
                readyToBeLeased.SetToLeased();

                return readyToBeLeased;
            }
        }

        private bool Subscribed(TheQueueSubscriber subscriber)
        {
            return _subscribers.ContainsKey(subscriber.QueueSubscriber.SubscriberId);
        }


        public void UnLease(TheQueueSubscriber subscriber)
        {
            lock (_lockObject)
            {

                if (subscriber.MessagesSize > 0)
                {
                    subscriber.SetOnDeliveryAndSendMessages();

                    if (Subscribed(subscriber))
                        _onDelivery.Add(subscriber.ConfirmationId, subscriber);
                    
                }
                else
                    subscriber.SetToUnLeased();
            }
        }


        public IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> Delivered(long confirmationId)
        {
            lock (_lockObject)
            {
                if (!_onDelivery.ContainsKey(confirmationId))
                    return null;

                var item = _onDelivery[confirmationId];

                _onDelivery.Remove(confirmationId);

                var result = item.MessagesOnDelivery;
                
                item.SetToUnLeased();
                
                return result;
            }
        }
        
        
        public int GetCount()
        {
            lock (_lockObject)
            {
                return _subscribers.Count;
            }
        }

        public IEnumerable<TheQueueSubscriber> GetSubscribers()
        {
            return _subscribersAsReadOnlyList;
        }
    }

}