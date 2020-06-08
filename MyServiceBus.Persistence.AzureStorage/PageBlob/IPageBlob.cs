using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MyServiceBus.Persistence.AzureStorage.PageBlob
{
    public interface IPageBlob
    {
        ValueTask<long> GetBlobSizeAsync();
        Task<MemoryStream> ReadAsync(long pageFrom, long pages);
        Task<MemoryStream> DownloadAsync();
        Task WriteAsync(MemoryStream data, long pageNo, int resizeRatio=1);

    }



    public static class PageBlobUtils
    {

        public const int PageSize = 512;

        public static long GetBlobPageNo(long offset)
        {
            return offset / PageSize;
        }

        public static ReadOnlyMemory<byte> GetDataReadyForPageBlob(this ReadOnlyMemory<byte> data)
        {
            if (data.Length % PageSize == 0)
                return data;


            var newDataLen = data.Length / 512+1;
            
            var result = new byte[newDataLen*512];
            
            data.CopyTo(result);

            return result;
        }


        private static byte[] _emptyPage = new byte[PageSize];
        
        public static MemoryStream GetDataReadyForPageBlob(this MemoryStream data)
        {
            if (data.Length % PageSize == 0)
                return data;
            
            var newDataLen = (data.Length / PageSize +1) * PageSize ;

            var bytesToAdd = (int)(newDataLen - data.Length);

            data.Position = data.Length;
            data.Write(_emptyPage, 0, bytesToAdd);

            return data;
        } 
        
        
        public static async Task WriteBytesAsync(this IPageBlob pageBlob, long offset, MemoryStream data, int blobResizeRation=1)
        {

            var pageBlobNo = GetBlobPageNo(offset);
            var pagesInBlob = await pageBlob.GetPagesAmountAsync();

            if (pagesInBlob == pageBlobNo)
            {
                await pageBlob.WriteAsync(data.GetDataReadyForPageBlob(), pageBlobNo, blobResizeRation);
                return;
            }
            
            var dataFromBlob = await pageBlob.ReadAsync(pageBlobNo, 1);

            dataFromBlob.Position = offset -pageBlobNo * PageSize;
            
            data.WriteTo(dataFromBlob);
            
            await pageBlob.WriteAsync(dataFromBlob.GetDataReadyForPageBlob(), pageBlobNo, blobResizeRation);

        }

        public static async ValueTask<long> GetPagesAmountAsync(this IPageBlob pageBlob)
        {
            var blobSize = await pageBlob.GetBlobSizeAsync();
            return  blobSize / PageSize;
        }
        
        public static async IAsyncEnumerable<MemoryStream> ReadBlockByBlockAsync(this IPageBlob pageBlob, int pagesInBlock)
        {

            var pagesRemain = await pageBlob.GetPagesAmountAsync();

            long pageFrom = 0;

            while (pagesRemain > 0)
            {
                var pagesToRead = pagesRemain < pagesInBlock ? pagesRemain : pagesInBlock;

                var memChunk = await pageBlob.ReadAsync(pageFrom, pagesToRead);

                yield return memChunk;

                pageFrom += pagesToRead;
                pagesRemain -= pagesToRead;
            }

        }
        
    }
    
}