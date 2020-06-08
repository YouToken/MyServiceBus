using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;

namespace MyServiceBus.Domains.Topics
{
    public class TopicsManagement
    {
        private readonly TopicsList _topicsList;
        private readonly TopicsAndQueuesPersistenceProcessor _topicsAndQueuesPersistenceProcessor;
        private readonly MessageContentCacheByTopic _messageContentCacheByTopic;


        public TopicsManagement(TopicsList topicsList, 
            TopicsAndQueuesPersistenceProcessor topicsAndQueuesPersistenceProcessor,
            MessageContentCacheByTopic messageContentCacheByTopic)
        {
            _topicsList = topicsList;
            _topicsAndQueuesPersistenceProcessor = topicsAndQueuesPersistenceProcessor;
            _messageContentCacheByTopic = messageContentCacheByTopic;
        }

        public async ValueTask<MyTopic> AddIfNotExistsAsync(string topicId)
        {
            var topic = _topicsList.AddIfNotExists(topicId);
            _messageContentCacheByTopic.Create(topicId);
            await _topicsAndQueuesPersistenceProcessor.PersistTopicsAndQueuesInBackgroundAsync(_topicsList.Get());
            return topic;
        }
        
    }
}