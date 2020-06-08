using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestMockDbGarbageCollection
    {

        [Test]
        public  void TestMockGarbageCollect()
        {
            
            /*

            var mockStorage = new MessagesPersistentStorageMock();

            var topic = "mytopic";
            var data = new byte[4];
            var message = MessageContent.Create(99999, data, true, DateTime.UtcNow, true);
            mockStorage.SaveAsync(topic, new[]{message}).Wait();
            
            Assert.IsTrue(mockStorage.HasPage(topic, 0));
            Assert.IsFalse(mockStorage.HasPage(topic, 1));
            
            message = MessageContent.Create(100000, data, true, DateTime.UtcNow, true);
            mockStorage.SaveAsync(topic, new[]{message}).Wait();

            
            Assert.IsTrue(mockStorage.HasPage(topic, 0));
            Assert.IsTrue(mockStorage.HasPage(topic, 1));

            mockStorage.GarbageCollectAsync(topic, 99999).AsTask().Wait();
            
            Assert.IsTrue(mockStorage.HasPage(topic, 0));
            Assert.IsTrue(mockStorage.HasPage(topic, 1));
            
            mockStorage.GarbageCollectAsync(topic, 100000).AsTask().Wait();
            
            Assert.IsFalse(mockStorage.HasPage(topic, 0));
            Assert.IsTrue(mockStorage.HasPage(topic, 1));
*/

        }
    }
}