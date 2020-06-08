using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Domains.Persistence
{
    
    public class TopicsAndQueuesPersistenceProcessor
    {

        private readonly ITopicPersistenceStorage _topicPersistenceStorage;

        public TopicsAndQueuesPersistenceProcessor(ITopicPersistenceStorage topicPersistenceStorage)
        {
            _topicPersistenceStorage = topicPersistenceStorage;
        }

        public async Task PersistTopicsAndQueuesInBackgroundAsync(IReadOnlyList<MyTopic> topics)
        {
            
            var queuesData = new Dictionary<string, IReadOnlyList<IQueueSnapshot>>();

            foreach (var topic in topics)
            {
                var snapshot = topic.GetQueuesSnapshot();
                queuesData.Add(topic.TopicId, snapshot);
            }

            await _topicPersistenceStorage.SaveAsync(topics, queuesData);
        }
    }
    
}