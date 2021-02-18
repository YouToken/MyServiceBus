using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.QueueSubscribers
{


    public class QueueSubscribersList
    {
        public TopicQueue TopicQueue { get; }
        
        private readonly Dictionary<string, TheQueueSubscriber> _subscribers 
            = new ();
        
        private readonly Dictionary<long, TheQueueSubscriber> _subscribersByDeliveryId 
            = new ();


        private readonly object _lockObject;

        public QueueSubscribersList(TopicQueue topicQueue, object lockObject)
        {
            TopicQueue = topicQueue;
            _lockObject = lockObject;
        }


        public void Subscribe(IMyServiceBusSession session)
        {
            lock (_lockObject)
            {
                if (_subscribers.ContainsKey(session.SubscriberId))
                    throw new Exception($"Subscriber to topic: {TopicQueue.Topic}  and queue: {TopicQueue.QueueId} is already exists");

                var theSubscriber = new TheQueueSubscriber(session, TopicQueue);
                
                _subscribers.Add(session.SubscriberId, theSubscriber);
                _subscribersByDeliveryId.Add(theSubscriber.ConfirmationId, theSubscriber);
            }
        }

        public TheQueueSubscriber Unsubscribe(IMyServiceBusSession session)
        {
            lock (_lockObject)
            {
                if (_subscribers.Remove(session.SubscriberId, out var removedItem))
                {
                    _subscribersByDeliveryId.Remove(removedItem.ConfirmationId);
                    return removedItem;
                }

                return null;
            }

        }

        public TheQueueSubscriber LeaseSubscriber()
        {
            lock (_lockObject)
            {
                var readyToBeLeased
                    = _subscribers
                        .Values
                        .FirstOrDefault(itm => itm.Status == SubscriberStatus.UnLeased);

                if (readyToBeLeased == null)
                    return null;
                
                readyToBeLeased.SetToLeased();

                return readyToBeLeased;
            }
        }

        public void UnLease(TheQueueSubscriber subscriber)
        {
            lock (_lockObject)
            {
                if (subscriber.MessagesSize > 0)
                    subscriber.SetOnDeliveryAndSendMessages();
                else
                    subscriber.SetToUnLeased();
            }
        }

        public IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> Delivered(long confirmationId)
        {
            lock (_lockObject)
            {
                if (!_subscribersByDeliveryId.TryGetValue(confirmationId, out var subscriber)) 
                    return null;
                
                var result = subscriber.MessagesOnDelivery;
                
                subscriber.SetToUnLeased();
                    
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

    }

}