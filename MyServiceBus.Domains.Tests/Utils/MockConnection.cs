using System;
using System.Collections.Generic;
using System.Linq;
using MyDependencies;
using MyServiceBus.Domains.Execution;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Tests.Utils
{
    public class MockConnection : IQueueSubscriber
    {

        private readonly TopicsManagement _topicsManagement;

        private readonly TopicsList _topicsList;
        
        public MySession MySession { get; }
        
        public MyServiceBusSubscriber Subscriber { get; }
        
        public MyServiceBusPublisher Publisher { get; }

        public MockConnection(IServiceResolver sr, string sessionsName, DateTime dt)
        {
            SubscriberId = sessionsName;
            var sessionsList = sr.GetService<SessionsList>();
            _topicsList = sr.GetService<TopicsList>();
            _topicsManagement = sr.GetService<TopicsManagement>();


            Subscriber = sr.GetService<MyServiceBusSubscriber>();
            Publisher = sr.GetService<MyServiceBusPublisher>();
            
            MySession = sessionsList.NewSession(SubscriberId, "10.0.0.0", dt, TimeSpan.FromMinutes(1), 0);
        }
        
        public readonly List<(TopicQueue topicQueue, IReadOnlyList<IMessageContent> messages, long confirmationId)> Messages 
            = new List<(TopicQueue topicQueue, IReadOnlyList<IMessageContent> messages, long confirmationId)>();



        public (TopicQueue topicQueue, IReadOnlyList<IMessageContent> messages, long confirmationId) GetLastSentMessage()
        {
            return Messages.Last();
        }
        
        
        public void SendMessagesAsync(TopicQueue topicQueue, IReadOnlyList<IMessageContent> messages, long confirmationId)
        {
            Messages.Add((topicQueue, messages, confirmationId));
        }

        public string SubscriberId { get; }
        public bool Disconnected { get; private set; }


        public int GetSentPackagesCount()
        {
            return Messages.Count;
        }


        public ExecutionResult PublishMessage( string topicName, byte[] message, DateTime dateTime, bool persistImmediately = false)
        {
            topicName = topicName.ToLower();
            return Publisher.PublishAsync(MySession, topicName, new[] {message}, dateTime, persistImmediately).Result;
        }
        
        public MyTopic CreateTopic(string topicName)
        {
            return _topicsManagement.AddIfNotExistsAsync(topicName).Result;
        }


        public TopicQueue Subscribe(string topicId, string queueId, bool deleteOnDisconnect = true)
        {
            var topic = _topicsList.Get(topicId);

            var queue = topic.CreateQueueIfNotExists(queueId, deleteOnDisconnect);
            
            var task = Subscriber.SubscribeToQueueAsync(queue, this);
                
            task.AsTask().Wait();

            return queue;
        }


        public void Disconnect()
        {
            Disconnected = true;
            Subscriber.DisconnectSubscriberAsync(this).AsTask().Wait();
        }

        public void ConfirmDelivery(TopicQueue queue, long confirmationId)
        {
            Subscriber.ConfirmDeliveryAsync(queue.Topic, queue.QueueId, confirmationId, true);
        }
    }
}