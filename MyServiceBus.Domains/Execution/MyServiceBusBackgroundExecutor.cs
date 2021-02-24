using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Execution
{
    public class MyServiceBusBackgroundExecutor
    {

        private readonly MyServiceBusDeliveryHandler _myServiceBusDeliveryHandler;
        private readonly TopicsList _topicsList;
        private readonly TopicsAndQueuesPersistenceProcessor _topicsAndQueuesPersistenceProcessor;
        private readonly MessageContentPersistentProcessor _messageContentPersistentProcessor;

        public MyServiceBusBackgroundExecutor(MyServiceBusDeliveryHandler myServiceBusDeliveryHandler,
            TopicsList topicsList, 
            TopicsAndQueuesPersistenceProcessor topicsAndQueuesPersistenceProcessor,
            MessageContentPersistentProcessor messageContentPersistentProcessor)
        {
            _myServiceBusDeliveryHandler = myServiceBusDeliveryHandler;
            _topicsList = topicsList;
            _topicsAndQueuesPersistenceProcessor = topicsAndQueuesPersistenceProcessor;
            _messageContentPersistentProcessor = messageContentPersistentProcessor;
        }

        public async ValueTask ExecuteAsync()
        {
            
            var topics = _topicsList.Get();

            foreach (var topic in topics)
            {
                await _messageContentPersistentProcessor.GarbageCollectAsync(topic);
                await _myServiceBusDeliveryHandler.SendMessagesAsync(topic);
            }
            
        }


        public async ValueTask PersistAsync()
        {
            var topics = _topicsList.Get();
            
            await _topicsAndQueuesPersistenceProcessor.PersistTopicsAndQueuesInBackgroundAsync(topics);
            
            foreach (var topic in topics)
            {
                await _messageContentPersistentProcessor.PersistMessageContentAsync(topic);
            }
        }
        
        
    }
}