using System;
using MyServiceBus.Domains.Tests.Utils;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestDeleteQueueOnDisconnect
    {
        [Test]
        public void TestDeleteOnDisconnect()
        {
            var ioc = TestIoc.CreateForTests();

            const string topicName = "testtopic";
            const string queueName = "testqueue";

            var nowTime = DateTime.Parse("2019-01-01T00:00:00");
            
            var session = ioc.ConnectSession("MySession", nowTime);
            var topic = session.CreateTopic(topicName);
            session.Subscribe(topicName, queueName);

            var queues = topic.GetQueues();
            Assert.AreEqual(1, queues.Count);
            
            session.Disconnect();
            
            queues = topic.GetQueues();
            Assert.AreEqual(0, queues.Count);
            
        }
        
        [Test]
        public void TestNotDeleteOnDisconnect()
        {
            var ioc = TestIoc.CreateForTests();

            const string topicName = "testtopic";
            const string queueName = "testqueue";

            var nowTime = DateTime.Parse("2019-01-01T00:00:00");
            
            var session = ioc.ConnectSession("MySession", nowTime);
            var topic = session.CreateTopic(topicName);
            session.Subscribe(topicName, queueName, false);

            var queues = topic.GetQueues();
            Assert.AreEqual(1, queues.Count);
            
            session.Disconnect();
            
            queues = topic.GetQueues();
            Assert.AreEqual(1, queues.Count);
            
        }
    }
}