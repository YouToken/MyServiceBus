using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Server.Models;
using MyServiceBus.Domains.MessagesContent;

namespace MyServiceBus.Server.Controllers
{
    public class MonitoringController : Controller
    {
        
        [HttpGet("/Status")]
        public MonitoringModel Index()
        {
            return MonitoringModel.Create();
        }

        [HttpGet("/ActivePages")]
        public Dictionary<string, IEnumerable<long>> GetActivePages()
        {
            var topics = ServiceLocator.TopicsList.Get();

            var result = new Dictionary<string, IEnumerable<long>>();
            foreach (var myTopic in topics)
            {
                var dict = myTopic.GetActiveMessagePages();
                
                if (dict.Count==0)
                    continue;
                
                result.Add(myTopic.TopicId, dict.Keys.ToList());
            }

            return result;
        }
        
    }
    
}