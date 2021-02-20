using System.Linq;

namespace MyServiceBus.Domains.Queues
{
    public static class TopicQueueExtensions
    {

        public static int GetLeasedMessagesCount(this TopicQueue topicQueue)
        {
            return topicQueue.SubscribersList.GetReadAccess(readAccess =>
            {
                return readAccess.GetSubscribers().Sum(subscriber => subscriber.MessagesOnDelivery.Count);
            });
        }
        
        public static long GetMinMessageId(this TopicQueue topicQueue)
        {
            return topicQueue.SubscribersList.GetReadAccess(readAccess =>
            {
                long result = -1;
                
                foreach (var subscriber in readAccess.GetSubscribers().Where(subscriber => subscriber.MessagesOnDelivery.Count > 0))
                {
                    if (result == -1)
                        result = subscriber.LeasedQueue.GetMinId();
                    else
                    {
                        var newResult = subscriber.LeasedQueue.GetMinId();

                        if (newResult < result)
                            result = newResult;
                    }
                }

                return result;
            });
        }
        
    }
}