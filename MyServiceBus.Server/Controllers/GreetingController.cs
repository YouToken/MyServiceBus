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
            var session = ServiceLocator.SessionsList.NewSession(name, 
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", 
                DateTime.UtcNow, Startup.SessionTimeout, 0, SessionType.Http);
            
            return session.Id;
        }
        
        [HttpPost("Greeting/Ping")]
        public IActionResult Index([FromForm][Required]long sessionId)
        {
            var session = ServiceLocator.SessionsList.GetSession(sessionId, DateTime.UtcNow);
            
            if (session == null)
                return Forbid();
            
            return Content(session.Id.ToString());
        }
    }
}