using System;
using System.Collections.Generic;
using System.Linq;

namespace MyServiceBus.Server.Hubs
{
    public class MonitoringConnectionsList
    {

        private readonly Dictionary<string, MonitoringConnection> _connections = new ();
        private IReadOnlyList<MonitoringConnection> _connectionsAsList = Array.Empty<MonitoringConnection>();
        
        public void Add(MonitoringConnection connection)
        {
            _connections.TryAdd(connection.Id, connection);
            _connectionsAsList = _connections.Values.ToList();
        }

        public void Remove(string id)
        {
            if (_connections.Remove(id))
                _connectionsAsList = _connections.Values.ToList();
        }

        public IReadOnlyList<MonitoringConnection> GetAll()
        {
            return _connectionsAsList;
        }
    }
}