using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Server.Models;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Server.Tcp;
using MyTcpSockets;

namespace MyServiceBus.Server.Controllers
{
    public class MonitoringController : Controller
    {
        
        [HttpGet("/Monitoring")]
        public MonitoringModel Index()
        {
            var topics = ServiceLocatorApi.TopicsList.Get();

            var sessions = ServiceLocatorApi
                .TcpServer
                .GetConnections();

            var knownConnections =
                sessions
                    .Cast<MyServiceBusTcpContext>()
                    .Where(itm => itm.Session != null)
                    .ToList();

            var unknownConnections = sessions
                .Cast<MyServiceBusTcpContext>()
                .Where(itm => itm.Session == null)
                .Select(UnknownConnectionModel.Create)
                .ToList();
            
            return new MonitoringModel
            {
                Topics = topics.Select(itm => TopicMonitoringModel.Create(itm, knownConnections)),
                Connections = knownConnections.Select(ConnectionModel.Create),
                UnknownConnections = unknownConnections,
                TcpConnections = sessions.Count,
                QueueToPersist = ServiceLocatorApi.MessagesToPersistQueue.GetMessagesToPersistCount().Select(itm => new QueueToPersist
                {
                    TopicId = itm.topic,
                    Count = itm.count
                })
            };
        }

        [HttpGet("/ActivePages")]
        public Dictionary<string, IEnumerable<long>> GetActivePages()
        {
            var topics = ServiceLocatorApi.TopicsList.Get();

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