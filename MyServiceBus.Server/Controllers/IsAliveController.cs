using System;
using Microsoft.AspNetCore.Mvc;

namespace MyServiceBus.Server.Controllers
{
    [ApiController]
    public class IsAliveController: Controller
    {
        private static readonly Lazy<object> Version = new Lazy<object>(() => new
        {
            Name = ServiceLocator.AppName,
            Version = ServiceLocator.AppVersion,
            StartedAt = ServiceLocator.StartedAt,
            Host = ServiceLocator.Host,
            Environment = ServiceLocator.AspNetEnvironment
        });

        private static IActionResult _isAliveResult;

        [HttpGet("api/isalive")]
        public IActionResult IsAlive()
        {
            _isAliveResult ??= Json(Version.Value);
            return _isAliveResult;
        }
    }
}