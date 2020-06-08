using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyServiceBus.Server.Controllers
{
    
    public class QueuesController : Controller
    {
        [HttpDelete("/Queues/")]
        public IActionResult Delete([FromQuery][Required]string topicId, [FromQuery][Required]string queueId)
        {
            var topic = ServiceLocatorApi.TopicsList.Get(topicId);
            
            if(topic == null)
                throw new Exception($"Topic {topicId} is not found");
            
            topic.DeleteQueue(queueId);

            return Content("Ok");
        }
    }
}