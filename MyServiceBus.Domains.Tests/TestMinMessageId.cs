using System;
using MyServiceBus.Domains.Tests.Utils;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestMinMessageId
    {

        [Test]
        public void TestMinMessageIdCleaning()
        {

            var ioc = TestIoc.CreateForTests();
            
            const string topicName = "testtopic";
            const string queueName = "testqueue";

            
            var nowTime = DateTime.Parse("2019-01-01T00:00:00");
            var session = ioc.ConnectSession("MySession", nowTime);
            
            session.CreateTopic(topicName);
            var queue = session.Subscribe(topicName, queueName);

            
            session.PublishMessage(topicName, new byte[] {0}, nowTime);
            Assert.AreEqual(0, queue.GetMessagesCount());
            Assert.AreEqual(1, queue.GetLeasedMessagesCount());
            var firstSent = session.GetLastSentMessage();
            
            session.PublishMessage(topicName, new byte[] {1}, nowTime);
            
            Assert.AreEqual(1, queue.GetMessagesCount());
            Assert.AreEqual(1, queue.GetLeasedMessagesCount());

            session.ConfirmDelivery(firstSent.topicQueue, firstSent.confirmationId);
            
            Assert.AreEqual(0, queue.GetMessagesCount());
            Assert.AreEqual(1, queue.GetLeasedMessagesCount());
            
       
        }
   
    }
}