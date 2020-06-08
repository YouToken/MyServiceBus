using System;
using MyServiceBus.Tests.Utils;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestMixedDisconnects
    {


        [Test]
        public void TestThreePublishesFirstAndThirdDisconnect()
        {
            var ioc = TestIoc.CreateForTests()
                .WithSessionTimeOut(TimeSpan.FromSeconds(5));

            const string topicName = "testtopic";
            const string queueName = "testqueue";


            var nowTime = DateTime.Parse("2019-01-01T00:00:00");
            var session1 = ioc.ConnectSession("MySession1", nowTime);
            session1.CreateTopic(topicName);
            session1.Subscribe(topicName, queueName);

            session1.PublishMessage(topicName, new byte[] {1}, nowTime);
            
            Assert.AreEqual(1, session1.GetSentPackagesCount());

            var session2 = ioc.ConnectSession("MySession2", nowTime);
            session2.Subscribe(topicName, queueName);
            
            Assert.AreEqual(0, session2.GetSentPackagesCount());
            
            session1.Disconnect();
            
            Assert.AreEqual(1, session2.GetSentPackagesCount());
            
            var session3 = ioc.ConnectSession("MySession3", nowTime);
            session3.Subscribe(topicName, queueName);
            Assert.AreEqual(0, session3.GetSentPackagesCount());

        }

    }
}