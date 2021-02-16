using System;
using System.Collections.Generic;
using DotNetCoreDecorators;
using MyServiceBus.Domains.Queues;

namespace MyServiceBus.Domains.Sessions
{
    public enum SessionType
    {
        Tcp, Http
    }
    
    
    public class MyServiceBusSession
    {
        public long Id { get; private set; }
        public string Ip { get; private set; }
        public string Name { get; private set; }
        public DateTime LastAccess { get; internal set; }
        public TimeSpan SessionTimeout { get; private set; }
        
        public DateTime Created { get; } = DateTime.UtcNow;
        
        public int ProtocolVersion { get; private set; }
        
        public bool Disconnected { get; private set; }

        public void Disconnect()
        {
            Disconnected = true;
            _onDisconnect?.Invoke(this);
        }
        
        
        public bool IsExpired(DateTime now)
        {
            return Disconnected || now - LastAccess >= SessionTimeout;
        }
        
        private Dictionary<string, string> _topicsPublishers = new Dictionary<string, string>();

        private IReadOnlyList<string> _topicsPublishersAsReadOnlyList = Array.Empty<string>();

        public void PublishToTopic(string topic)
        {
            lock (this)
            {
                if (_topicsPublishers.ContainsKey(topic))
                    return;

                var newTopics = new Dictionary<string, string>(_topicsPublishers) {{topic, topic}};
                _topicsPublishers = newTopics;

                _topicsPublishersAsReadOnlyList = _topicsPublishers.Keys.AsReadOnlyList();
            }
        }

        public bool IsTopicPublisher(string topicName)
        {
            return _topicsPublishers.ContainsKey(topicName);
        }
        
        public IReadOnlyList<string> GetTopicsToPublish()
        {
            return _topicsPublishersAsReadOnlyList;
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

        private Action<MyServiceBusSession> _onDisconnect;

        
        public SessionType SessionType { get; private set; }
        public static MyServiceBusSession Create(long id, string name, string ip, DateTime nowTime, in TimeSpan timeout, Action<MyServiceBusSession> onDisconnect, int protocolVersion,
            SessionType sessionType)
        {
            return new MyServiceBusSession
            {
                Id = id,
                Name = name,
                LastAccess = nowTime,
                Ip = ip,
                SessionTimeout = timeout,
                _onDisconnect = onDisconnect,
                ProtocolVersion = protocolVersion,
                SessionType = sessionType
            };
        }

        public override string ToString()
        {
            return "Session: " + Name;
        }

        public void UpdateLastAccess()
        {
            LastAccess = DateTime.UtcNow;
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