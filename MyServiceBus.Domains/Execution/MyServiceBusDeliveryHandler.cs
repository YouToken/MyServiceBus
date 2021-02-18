using System;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Execution
{
    
    public class MyServiceBusDeliveryHandler
    {
        private readonly MessageContentReader _messageContentReader;
        private readonly IMyServiceBusSettings _myServiceBusSettings;

        public MyServiceBusDeliveryHandler(MessageContentReader messageContentReader, IMyServiceBusSettings myServiceBusSettings)
        {
            _messageContentReader = messageContentReader;
            _myServiceBusSettings = myServiceBusSettings;
        }

        private ValueTask FillMessagesAsync(TopicQueue topicQueue, TheQueueSubscriber subscriber)
        {
            
            return topicQueue.LockAndGetWriteAccessAsync(async topicDequeue =>
            {
                   
                var msg = topicDequeue.DequeAndLease();
            
                if (msg.messageId<0)
                    return;

                while (msg.messageId >= 0)
                {
                    var myMessage =
                        await _messageContentReader.GetAsync(topicQueue.Topic, msg.messageId);

                    subscriber.AddMessage(myMessage, msg.attemptNo);

                    if (subscriber.QueueSubscriber.Disconnected)
                    {
                        Console.WriteLine("Disconnected in the Leased State. Messages Size: "+subscriber.MessagesSize);
                        Console.WriteLine("First Message: "+subscriber.MessagesOnDelivery[0].message.MessageId);
                        Console.WriteLine("Last Message: "+subscriber.MessagesOnDelivery[^1].message.MessageId);
                        break;
                    }

                    if (subscriber.MessagesSize >= _myServiceBusSettings.MaxDeliveryPackageSize)
                        break;

                    msg = topicDequeue.DequeAndLease();
                }
            });
        }

        public async ValueTask SendMessagesAsync(TopicQueue topicQueue)
        {
            var leasedSubscriber = topicQueue.QueueSubscribersList.LeaseSubscriber();
            
            if (leasedSubscriber == null)
                return;

            try
            {
                await FillMessagesAsync(topicQueue, leasedSubscriber);
                
            }
            catch (Exception ex)
            {
                if (leasedSubscriber.MessagesSize > 0)
                {
                    topicQueue.NotDelivered(leasedSubscriber.MessagesOnDelivery, 0);
                    leasedSubscriber.ClearMessages();
                }
                Console.WriteLine(ex);
            }
            finally
            {
                topicQueue.QueueSubscribersList.UnLease(leasedSubscriber);
            }

        }

        public async ValueTask SendMessagesAsync(MyTopic topic)
        {
            foreach (var topicQueue in topic.GetQueues())
                await SendMessagesAsync(topicQueue);
        }

    }
}