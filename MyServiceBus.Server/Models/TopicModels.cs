using MyServiceBus.Domains.Topics;

namespace MyServiceBus.Server.Models
{
    public class TopicInfoModel
    {
        
        public string Id { get; set;  }
        public long MessageId { get; set; }
        public int MaxMessagesInCache { get; set; }

        public static TopicInfoModel Create(MyTopic topic)
        {
            return new TopicInfoModel
            {
                Id = topic.TopicId,
                MaxMessagesInCache = topic.MaxMessagesInCache,
                MessageId = topic.MessageId
            };
        }
        
    }
}