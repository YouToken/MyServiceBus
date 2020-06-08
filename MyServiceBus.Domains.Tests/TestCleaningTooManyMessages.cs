using System;
using MyServiceBus.Tests.Utils;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestCleaningTooManyMessages
    {
        [Test]
        public void TestCleaningMessages()
        {
            /*
            var ioc = TestIoc.CreateForTests()
                .WithEventuallyPersistenceTimeOut(TimeSpan.FromSeconds(5));
            
            const string topicName = "testtopic";
            const string queueName = "testqueue";

            
            var nowTime = DateTime.Parse("2019-01-01T00:00:00");
            var session = ioc.ConnectSession("MySession", nowTime);
            
            session.CreateTopic(topicName);
            session.Subscribe(topicName, queueName);

            
            session.PublishMessage(topicName, new byte[] {0}, nowTime);
            session.PublishMessage(topicName, new byte[] {1}, nowTime);
            session.PublishMessage(topicName, new byte[] {2}, nowTime);
            session.PublishMessage(topicName, new byte[] {3}, nowTime);            
            session.PublishMessage(topicName, new byte[] {4}, nowTime);            
            session.PublishMessage(topicName, new byte[] {5}, nowTime);
            session.PublishMessage(topicName, new byte[] {6}, nowTime);

            Assert.AreEqual(5, ioc.GetCachedMessagesCount(topicName));

            nowTime = nowTime.AddSeconds(6);
            ioc.SetCurrentTime(nowTime);

            var storedMessages = ioc.GetMessagesFromPersistentStorage(topicName);
            Assert.AreEqual(7, storedMessages.Count);
            Assert.AreEqual(5, ioc.GetCachedMessagesCount(topicName));
            
            */
        }
    }
}