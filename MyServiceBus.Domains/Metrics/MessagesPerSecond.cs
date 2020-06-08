using System;
using System.Collections.Generic;
using System.Linq;

namespace MyServiceBus.Domains.Metrics
{
    public class MessagesPerSecond
    {
        private readonly Queue<int> _items = new Queue<int>();

        public void PutData(int amount)
        {
            _items.Enqueue(amount);

            while (_items.Count > 120)
            {
                _items.Dequeue(); 
            }
        }

        public IReadOnlyList<int> GetItems()
        {
            return _items.ToList();
        }
    }


    public class MessagesPerSecondByTopic
    {
        
        private readonly Dictionary<string, MessagesPerSecond> _messagesPerSeconds = new Dictionary<string, MessagesPerSecond>();

        public void PutData(string topicId, int amount)
        {
            lock (_messagesPerSeconds)
            {
                if (!_messagesPerSeconds.ContainsKey(topicId))
                    _messagesPerSeconds.Add(topicId, new MessagesPerSecond());
                
                _messagesPerSeconds[topicId].PutData(amount);
            }
        }

        public IReadOnlyList<int> GetRecordsPerSecond(string topicId)
        {
            lock (_messagesPerSeconds)
            {
                return _messagesPerSeconds.ContainsKey(topicId) 
                    ? _messagesPerSeconds[topicId].GetItems() 
                    : Array.Empty<int>();
            }
        }
        
    }
    
}