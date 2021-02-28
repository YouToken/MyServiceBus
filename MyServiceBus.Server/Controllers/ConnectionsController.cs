using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MyServiceBus.Server.Controllers
{
    public class ConnectionsController : Controller
    {
        [HttpDelete("/Connections/KickTcpConnection")]
        public IActionResult KickTcpConnection([FromQuery]long id)
        {

            var connection = ServiceLocator.TcpServer.GetConnections().FirstOrDefault(itm => itm.Id == id);
            if (connection == null)
                return Content("Connection is not found");
            
            connection.Disconnect();
            return Content("Connection is kicked");

        }
    }
}