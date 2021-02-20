using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Execution
{
    
    public class MyServiceBusPublisher
    {
        private readonly TopicsList _topicsList;
        private readonly IMessagesToPersistQueue _messagesToPersistQueue;
        private readonly MyServiceBusDeliveryHandler _myServiceBusDeliveryHandler;
        private readonly TopicsAndQueuesPersistenceProcessor _topicsAndQueuesPersistenceProcessor;
        private readonly MessageContentPersistentProcessor _messageContentPersistentProcessor;

        public MyServiceBusPublisher(TopicsList topicsList, 
            IMessagesToPersistQueue messagesToPersistQueue,
            MyServiceBusDeliveryHandler myServiceBusDeliveryHandler, 
            TopicsAndQueuesPersistenceProcessor topicsAndQueuesPersistenceProcessor,
            MessageContentPersistentProcessor messageContentPersistentProcessor
            )
        {
            _topicsList = topicsList;
            _messagesToPersistQueue = messagesToPersistQueue;
            _myServiceBusDeliveryHandler = myServiceBusDeliveryHandler;
            _topicsAndQueuesPersistenceProcessor = topicsAndQueuesPersistenceProcessor;
            _messageContentPersistentProcessor = messageContentPersistentProcessor;
        }
        

        public async ValueTask<ExecutionResult> PublishAsync(MyServiceBusSession session, string topicId, IEnumerable<byte[]> messages, DateTime now, bool persistImmediately
            )
        {
            
            var topic = _topicsList.TryFindTopic(topicId);

            if (topic == null)
                return ExecutionResult.TopicNotFound;
            
            session.PublishToTopic(topicId);
            
            var addedMessages = topic.Publish(messages, now);


            _messagesToPersistQueue.EnqueueToPersist(topicId, addedMessages);

            
            if (addedMessages.Count == 0)
                return ExecutionResult.Ok;
            
            Task persistentQueueTask = null;
            Task persistMessagesTask = null;
            if (persistImmediately)
            {
                persistentQueueTask =  _topicsAndQueuesPersistenceProcessor.PersistTopicsAndQueuesInBackgroundAsync(_topicsList.Get());
                persistMessagesTask = _messageContentPersistentProcessor.PersistMessageContentInBackgroundAsync(topic);
            }

            foreach (var topicQueue in topic.GetQueues())
                topicQueue.EnqueueMessages(addedMessages);
    

            await _myServiceBusDeliveryHandler.SendMessagesAsync(topic);

            if (persistentQueueTask != null)
                await persistentQueueTask;

            if (persistMessagesTask != null)
                await persistMessagesTask;

            return ExecutionResult.Ok;
        }
        
    }
}