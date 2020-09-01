using System.Collections.Generic;
using System.Linq;
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
            var toSave
                = topics
                    .Select(itm => itm.GetQueuesSnapshot())
                    .ToList();
            await _topicPersistenceStorage.SaveAsync(toSave);
        }
        
    }
    
}