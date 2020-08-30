using System.Collections.Generic;
using System.IO;
using MyServiceBus.Persistence.AzureStorage.TopicMessages;

namespace MyServiceBus.Persistence.AzureStorage.PageBlob
{
    public class PageBlobParser
    {
        private readonly IPageBlob _pageBlob;

        public PageBlobParser(IPageBlob pageBlob)
        {
            _pageBlob = pageBlob;
        }


        private const int PagesReadingAmount = 2048;

        public async IAsyncEnumerable<MemoryStream> ReadMemoryContentMemoryChunksAsync()
        {
            
            var stream = new BinaryDataReader();


            var readingInt = true;
            var len = 0;
            
            await foreach (var mem in _pageBlob.ReadBlockByBlockAsync(PagesReadingAmount))
            {
                stream.Write(mem);

                while (true)
                {
                    if (readingInt)
                    {
                        if (stream.RemainsToRead < 4)
                            break;

                        len = stream.ReadInt();

                        if (len == 0)
                            break;

                        readingInt = false;
                    }
                    else
                    {
                        if (stream.RemainsToRead < len)
                            break;

                        var itemToYield = stream.ReadArray(len);

                        yield return itemToYield;
                        readingInt = true;
                    } 
                }

            }
            
        }
        
        
    }
}