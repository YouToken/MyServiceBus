using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Domains.Execution;

namespace MyServiceBus.Server.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MyServiceBusSubscriber _myServiceBusSubscriber;

        public MessagesController(MyServiceBusSubscriber myServiceBusSubscriber)
        {
            _myServiceBusSubscriber = myServiceBusSubscriber;
        }
        
        
        [HttpPost("Messages/ReplayMessage")]
        public async ValueTask<IActionResult> ReplayMessage([FromQuery][Required]string topicId, 
            [FromQuery][Required]string queueId, [FromQuery][Required]long messageId)
        {
            
            var topic = ServiceLocator.TopicsList.TryGet(topicId);
            
            if(topic == null)
                return Conflict($"Topic {topicId} is not found");

            var queue = topic.TryGetQueue(topicId);
            
            if (queue == null)
                return Conflict($"Queue {topicId}/{queueId} is not found");

            var result = await _myServiceBusSubscriber.ReplayMessageAsync(queue, messageId);

            
            if (result == ReplayMessageResult.MessageNotFound)
                return Conflict($"Message {messageId} is not found for topic {topicId}/{queueId}");

            return Content("OK");
        }
    }
}