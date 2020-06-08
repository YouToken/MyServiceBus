using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;

namespace MyServiceBus.Persistence.AzureStorage.PageBlob
{
    public class PageBlobInMem : IPageBlob
    {

        public class PageInMem
        {
            public PageInMem()
            {
                Data = new byte[PageBlobUtils.PageSize];
            }

            public PageInMem(byte[] src)
            {
                if (src.Length != PageBlobUtils.PageSize)
                    throw new Exception("Page data must have exactly "+PageBlobUtils.PageSize+" bytes");

                Data = src;
            }
            
            
            public readonly byte[] Data;
            
        }
        
        
        
        private readonly List<PageInMem> _mem = new List<PageInMem>();
        
        
        public ValueTask<long> GetBlobSizeAsync()
        {
            var result = _mem.Count * PageBlobUtils.PageSize;
            return new ValueTask<long>(result);
        }

        public Task<MemoryStream> ReadAsync(long pageFrom, long pages)
        {
            var result = new MemoryStream
            {
                Capacity = PageBlobUtils.PageSize
            };
            
            for (var i = 0; i < pages; i++)
            {
                result.Write(_mem[(int)pageFrom+i].Data);
            }
            return Task.FromResult(result); 
        }

        public Task<MemoryStream> DownloadAsync()
        {
            var result = new MemoryStream();

            foreach (var b in _mem)
                result.Write(b.Data);
            
            return Task.FromResult(result);
        }

        public Task WriteAsync(MemoryStream data, long pageNo, int resizeRatio)
        {
            if (data.Length % PageBlobUtils.PageSize != 0)
                throw new Exception("Page must be rounded to "+PageBlobUtils.PageSize);

            foreach (var chunk in data.ToArray().SplitToChunks(512))
            {
                var newPage = new PageInMem(chunk.ToArray());
                
                if (pageNo<_mem.Count)
                    _mem[(int)pageNo] =  newPage ;
                else
                    _mem.Add(newPage);

                pageNo++;
            }
            
            return Task.CompletedTask;
        }

    }
}