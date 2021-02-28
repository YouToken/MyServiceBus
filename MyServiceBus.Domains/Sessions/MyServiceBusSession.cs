using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceBus.Abstractions;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;

namespace MyServiceBus.Domains.Sessions
{
    public enum SessionType
    {
        Tcp, Http
    }
    
    public class MyServiceBusSession : IDisposable
    {
        public string Id { get; }
        
        public string SessionName { get; }


        private readonly ConcurrentDictionaryWithNoLocksOnRead<string, string> _topicsPublishers = new ();


        public void PublishToTopic(string topic)
        {
            _topicsPublishers.Add(topic, ()=>topic);
        }

        public bool IsTopicPublisher(string topicName)
        {
            return _topicsPublishers.ContainsKey(topicName);
        }
        
        public IReadOnlyList<string> GetTopicsToPublish()
        {
            return _topicsPublishers.GetAllValues();
        }
        
        private IReadOnlyList<TopicQueue> _subscribersToQueueAsList = Array.Empty<TopicQueue>();
        
        public void SubscribeToQueue(TopicQueue queue)
        {
            lock (this)
            {
                _subscribersToQueueAsList = _subscribersToQueueAsList.AddToReadOnlyList(queue);
            }
        }

        public IReadOnlyList<TopicQueue> GetQueueSubscribers()
        {
            return _subscribersToQueueAsList;
        }
        
        public SessionType SessionType { get; }
        public MyServiceBusSession (string id, string name,
            SessionType sessionType,
            Action<MyServiceBusSession> onDispose)
        {
            Id = id;
            SessionName = name;
            SessionType = sessionType;
            _onDispose = onDispose;
        }

        public override string ToString()
        {
            return "Session: " + SessionName;
        }

        private readonly Action<MyServiceBusSession> _onDispose;
        
        public void Dispose()
        {
            _onDispose(this);
        }

        public void UpdatePacketPerSeconds()
        {
            PacketsPerSecond++;
        }
        
        public int PublishPacketsInternal { get; set; }
        public int PublishPacketsPerSecond { get; set; }
        
        public int SubscribePacketsInternal { get; set; }

        public int SubscribePacketsPerSecond { get; set; }
        
        public int PacketsPerSecondInternal { get; set; }
        
        public int PacketsPerSecond { get; set; }

        public void Timer()
        {
            PublishPacketsPerSecond = PublishPacketsInternal;
            PublishPacketsInternal = 0;

            SubscribePacketsPerSecond = SubscribePacketsInternal;
            SubscribePacketsInternal = 0;

            PacketsPerSecond = PacketsPerSecondInternal;
            PacketsPerSecondInternal = 0;
        }
    }

}