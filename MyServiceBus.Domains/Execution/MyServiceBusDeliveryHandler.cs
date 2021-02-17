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


        private async ValueTask FillMessagesAsync(TopicQueue topicQueue, TheQueueSubscriber subscriber)
        {
            
            var messageId = topicQueue.DequeAndLease();
            
            if (messageId<0)
                return;

            while (messageId >= 0)
            {
                var myMessage =
                    await _messageContentReader.GetAsync(topicQueue.Topic.TopicId, messageId);

                subscriber.AddMessage(myMessage);

                if (subscriber.QueueSubscriber.Disconnected)
                {
                    Console.WriteLine("Disconnected in the Leased State. Messages Size: "+subscriber.MessagesSize);
                    Console.WriteLine("First Message: "+subscriber.MessagesOnDelivery[0].MessageId);
                    Console.WriteLine("Last Message: "+subscriber.MessagesOnDelivery[^1].MessageId);
                    break;
                }

                if (subscriber.MessagesSize >= _myServiceBusSettings.MaxDeliveryPackageSize)
                    break;

                messageId = topicQueue.DequeAndLease();
            }
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
            catch (Exception)
            {
                if (leasedSubscriber.MessagesSize > 0)
                {
                    topicQueue.NotDelivered(leasedSubscriber.MessagesOnDelivery);
                    leasedSubscriber.ClearMessages();
                }
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