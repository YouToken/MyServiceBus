using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Domains.QueueSubscribers;

namespace MyServiceBus.Server.Controllers
{
    
    public class QueuesController : Controller
    {
        [HttpDelete("/Queues/")]
        public IActionResult Delete([FromQuery][Required]string topicId, [FromQuery][Required]string queueId)
        {
            var topic = ServiceLocator.TopicsList.TryGet(topicId);
            
            if(topic == null)
                return Conflict($"Topic {topicId} is not found");
            
            topic.DeleteQueue(queueId);

            return Content("Ok");
        }

        [HttpPost("/Queues/SetMessageId")]
        public IActionResult SetMessageId([FromQuery] [Required] string topicId, [FromQuery] [Required] string queueId,
            [FromQuery] [Required] long messageId)
        {
            var topic = ServiceLocator.TopicsList.TryGet(topicId);

            if (topic == null)
                return Conflict($"Topic {topicId} is not found");

            topic.SetQueueMessageId(queueId, messageId);

            return Content("Ok");
        }

        
        private static TimeSpan TenSeconds = TimeSpan.FromSeconds(10);
        [HttpPost("/Queues/PushMessageAgain")]
        public IActionResult PushMessageAgain([FromQuery] [Required] string topicId, 
            [FromQuery] [Required] string queueId)
        {
            var topic = ServiceLocator.TopicsList.TryGet(topicId);

            if (topic == null)
                return Conflict($"Topic {topicId} is not found");

            var queue = topic.GetQueue(queueId);
            
            if (queue == null)
                    return Conflict($"Queue {queueId} is not found");


            var result = new StringBuilder();


            queue.SubscribersList.GetReadAccess(readAccess =>
            {
                foreach (var subscriber in readAccess.GetSubscribers())
                {
                    if (subscriber.Status == SubscriberStatus.OnDelivery &&
                        DateTime.UtcNow - subscriber.OnDeliveryStart > TenSeconds)
                    {
                        subscriber.SendMessages();

                        result.AppendLine("Push message to the subscriber: " + subscriber.Session.SubscriberId);
                    }
                }
            });

            return Content(result.ToString());
        }
    }
}