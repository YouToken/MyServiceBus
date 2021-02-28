using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Domains.Sessions;

namespace MyServiceBus.Server.Controllers
{
    public class GreetingController : Controller
    {
        [HttpPost("Greeting")]
        public long Index([FromForm][Required]string name)
        {
            var grpcSession = ServiceLocator.GrpcSessionsList.GenerateNewSession(name);

            var session = ServiceLocator.SessionsList.NewSession("HTTP-" + grpcSession.Id, name, SessionType.Http);

            grpcSession.Session = session;
            return grpcSession.Id;
        }
        
        [HttpPost("Greeting/Ping")]
        public IActionResult Index([FromForm][Required]long sessionId)
        {
            var session = ServiceLocator.GrpcSessionsList.TryGetSession(sessionId, DateTime.UtcNow);
            
            if (session == null)
                return Forbid();
            
            return Content(session.Id.ToString());
        }
    }
}