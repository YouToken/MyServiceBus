using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Persistence.AzureStorage.PageBlob;

namespace MyServiceBus.Persistence.AzureStorage.TopicMessages
{
    public class BlobMessagesStorageProcessor
    {
        private readonly IPageBlob _pageBlob;
        public MessagesPageId PageId { get; }

        public BlobMessagesStorageProcessor(IPageBlob pageBlob, MessagesPageId pageId)
        {
            _pageBlob = pageBlob;
            PageId = pageId;
        }
        
        public long DataLength { get; private set; }

        private async IAsyncEnumerable<IMessageContent> LoadDataFromBlobAsync()
        {
            var parser = new PageBlobParser(_pageBlob);

            await foreach (var content in parser.ReadMemoryContentMemoryChunksAsync())
            {
                content.Position = 0;
                var item = ProtoBuf.Serializer.Deserialize<MessageContentBlobContract>(content);
                yield return item;
            }
        }

        private async Task<MessagesPageInMemory> LoadPageMessagesAsync()
        {
            var result = new MessagesPageInMemory(PageId);
            await foreach (var itm in LoadDataFromBlobAsync())
            {
                result.Add(itm);
            }

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