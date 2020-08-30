using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Persistence.AzureStorage.PageBlob;

namespace MyServiceBus.Persistence.AzureStorage.TopicMessages
{
    public class BlobMessagesStorageProcessor
    {
        private readonly string _topicId;
        private readonly IPageBlob _pageBlob;
        public MessagesPageId PageId { get; }

        public BlobMessagesStorageProcessor(string topicId, IPageBlob pageBlob, MessagesPageId pageId)
        {
            _topicId = topicId;
            _pageBlob = pageBlob;
            PageId = pageId;
        }
        
        public long DataLength { get; private set; }

        private async Task LoadDataFromBlobAsync(Action<IMessageContent> newItemCallback)
        {
            var parser = new PageBlobParser(_pageBlob);

            long prevMessageId = -1;

            await foreach (var content in parser.ReadMemoryContentMemoryChunksAsync())
            {
                try
                {
                    content.Position = 0;
                    var item = ProtoBuf.Serializer.Deserialize<MessageContentBlobContract>(content);
                    prevMessageId = item.MessageId;
                    newItemCallback(item);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Can not read the message from the blob {_topicId}. Prev successful message was: "+prevMessageId);
                    Console.WriteLine(e);
                }
            }
        }

        private async Task<MessagesPageInMemory> LoadPageMessagesAsync()
        {
            var result = new MessagesPageInMemory(PageId);

            await LoadDataFromBlobAsync(itm =>
            {
                result.Add(itm);
            });

            _messagesPageInMemory = result;
            return result;
        }


        private MessagesPageInMemory _messagesPageInMemory;
        
        public ValueTask<MessagesPageInMemory> GetPageMessagesAsync()
        {
            
            if (_messagesPageInMemory != null)
                return new ValueTask<MessagesPageInMemory>(_messagesPageInMemory);

            var task = LoadPageMessagesAsync();
            return new ValueTask<MessagesPageInMemory>(task);

        }

        public async Task SaveMessagesAsync(IEnumerable<IMessageContent> messagesToSave, int blobResizeRation = 1)
        {
            var messagesPageInMemory = await GetPageMessagesAsync();

            var memString = new MemoryStream();

            foreach (var messageContent in messagesToSave)
            {
                if (!messagesPageInMemory.Add(messageContent))
                    continue;

                var bytes = messageContent.SerializeContract();
                
                memString.Write(bytes);
            }
            
            if (memString.Length==0)
                return;

            var messageLength = memString.Length;
            
            await _pageBlob.WriteBytesAsync(DataLength, memString, blobResizeRation);

            DataLength += messageLength;
        }
    }
}