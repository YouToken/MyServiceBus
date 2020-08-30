using System;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Persistence.AzureStorage.PageBlob;
using MyServiceBus.Persistence.AzureStorage.TopicMessages;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestSavingMessageContentToBlob
    {

        [Test]
        public void TestBasicSavingMessageContent()
        {
            var blobInMem = new PageBlobInMem();

            var storageProcessor = new BlobMessagesStorageProcessor("test", blobInMem, new MessagesPageId(0));

            for (var i = 0; i < 50000; i++)
            {
                byte b = (byte) i;
                var messageContent = MessageContent.Create(i,0, new[] {b, b, b, b, b, b, b, b, b, b}, DateTime.UtcNow);
                storageProcessor.SaveMessagesAsync(new[] {messageContent}).Wait();
            }

            Console.WriteLine(blobInMem.GetBlobSizeAsync().Result);

            var resultStorageProcessor = new BlobMessagesStorageProcessor("test", blobInMem, new MessagesPageId(0));

            var pageMessages = resultStorageProcessor.GetPageMessagesAsync().Result;

            Assert.AreEqual(50000, pageMessages.Count);
        }
        

    }
    
}