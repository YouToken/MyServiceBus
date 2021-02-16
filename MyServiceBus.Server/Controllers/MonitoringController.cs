using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Server.Models;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Server.Tcp;

namespace MyServiceBus.Server.Controllers
{
    public class MonitoringController : Controller
    {
        
        [HttpGet("/Monitoring")]
        public MonitoringModel Index()
        {
            var topics = ServiceLocator.TopicsList.Get();

            var sessions = ServiceLocator
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
            
            var result = new MonitoringModel
            {
                Topics = topics.Select(itm => TopicMonitoringModel.Create(itm, knownConnections)),
                Connections = knownConnections.Select(ConnectionModel.Create).ToList(),
                UnknownConnections = unknownConnections,
                TcpConnections = sessions.Count,
                QueueToPersist = ServiceLocator.MessagesToPersistQueue.GetMessagesToPersistCount().Select(itm => new QueueToPersist
                {
                    TopicId = itm.topic,
                    Count = itm.count
                })
            };
            
            var grpcConnections = ServiceLocator.SessionsList.GetSessions()
                .Where(itm => itm.SessionType == SessionType.Http);
            
            result.Connections.AddRange(grpcConnections.Select(ConnectionModel.Create));


            return result;
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