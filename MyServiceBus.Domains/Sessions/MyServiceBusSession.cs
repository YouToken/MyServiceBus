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
    
    
    public class MyServiceBusSession : IDisposable
    {
        public string Id { get; }
        
        public string SessionName { get; }


        private Dictionary<string, string> _topicsPublishers = new ();

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