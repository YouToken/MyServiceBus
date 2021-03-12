using System;
using System.Collections.Generic;
using System.Linq;

namespace MyServiceBus.Server.Services
{

    public class ConnectionLogItem
    {
        public DateTime DateTime { get; set; }
        public long ConnectionId { get; set; }
        public string ClientName { get; set; }
        public string Ip { get; set; }
        public string Message { get; set; }
    }
    
    
    public class ConnectionsLog
    {
        public int SnapshotId { get; private set; }

        private static readonly Queue<ConnectionLogItem> Items = new ();

        private IReadOnlyList<ConnectionLogItem> _itemsAsList = Array.Empty<ConnectionLogItem>();
        
        public void AddLog(long connectionId, string clientName, string ip, string message)
        {
            Console.WriteLine($"{DateTime.UtcNow:O} Ip: {ip}; Client: {clientName}. Message:{message}");
            var item = new ConnectionLogItem
            {
                DateTime = DateTime.UtcNow,
                Ip = ip,
                Message = message,
                ClientName = clientName,
                ConnectionId = connectionId
            };
            lock (Items)
            {
                Items.Enqueue(item);

                while (Items.Count>50)
                    Items.Dequeue();

                _itemsAsList = null;
                SnapshotId++;
            }
        }

        public IReadOnlyList<ConnectionLogItem> GetList()
        {
            var result = _itemsAsList;

            if (_itemsAsList != null)
                return result;

            lock (Items)
            {
                _itemsAsList = Items.ToList();
                return _itemsAsList;
            }
        }
        
        
        
    }
}