using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MyServiceBus.Persistence.AzureStorage.PageBlob
{
    public class MyPageBlob : IPageBlob
    {
        private readonly CloudStorageAccount _cloudStorageAccount;

        private readonly string _containerName;
        private readonly string _fileName;


        private CloudPageBlob _pageBlob;

        public MyPageBlob(CloudStorageAccount cloudStorageAccount, string containerName, string fileName)
        {
            _cloudStorageAccount = cloudStorageAccount;
            _containerName = containerName;
            _fileName = fileName;
            CreateCloudBlobContainerAsync().Wait();
        }

        private async Task CreateCloudBlobContainerAsync()
        {
            var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = blobClient.GetContainerReference(_containerName);
            
            await cloudBlobContainer.CreateIfNotExistsAsync();
            _pageBlob = cloudBlobContainer.GetPageBlobReference(_fileName);
            if (!await _pageBlob.ExistsAsync())
                await _pageBlob.CreateAsync(PageBlobUtils.PageSize);
        }


        private long _blobSize = -1;

        private async Task<long> GetBlobSizeFromStorageAsync()
        {
            
            await _pageBlob.FetchAttributesAsync();
            _blobSize =  _pageBlob.Properties.Length;
            return _blobSize;
        }

        public ValueTask<long> GetBlobSizeAsync()
        {
            
            if (_blobSize<0)
                return new ValueTask<long>(GetBlobSizeFromStorageAsync());
            
            return new ValueTask<long>(_blobSize);
        }

        public async Task ResizeBlobAsync(long blobSize)
        {
            await _pageBlob.ResizeAsync(blobSize);
            _blobSize = blobSize;
        }

        public async Task<MemoryStream> ReadAsync(long pageFrom, long pages)
        {
            var memStream = new MemoryStream();
            await _pageBlob.DownloadRangeToStreamAsync(memStream, pageFrom*PageBlobUtils.PageSize, pages*PageBlobUtils.PageSize);
            return memStream;
        }

        public async Task<MemoryStream> DownloadAsync()
        {
            var mem = new MemoryStream();
            await _pageBlob.DownloadToStreamAsync(mem);
            return mem;
        }


        public async Task WriteAsync(MemoryStream data, long pageNo, int resizeRatio)
        {
            var neededAmount = data.Length / PageBlobUtils.PageSize;

            if (neededAmount * PageBlobUtils.PageSize < data.Length)
                neededAmount = (neededAmount+1) * PageBlobUtils.PageSize;
            else
                neededAmount = data.Length;

            neededAmount += pageNo * PageBlobUtils.PageSize * resizeRatio;
            
            var blobSize = await GetBlobSizeAsync();

            if (neededAmount>blobSize)
            {
                await ResizeBlobAsync(neededAmount);
                Console.WriteLine($"Ressizing blob {_containerName}/{_fileName} to new size: "+neededAmount);
            }

            var mt5 = MD5.Create();

            data.Position = 0;
            var hash = mt5.ComputeHash(data);
            data.Position = 0;

            try
            {
                await _pageBlob.WritePagesAsync(data, pageNo*PageBlobUtils.PageSize, Convert.ToBase64String(hash));
            }
            catch (Exception e)
            {
               Console.WriteLine($"Attempt to write page is falied {data} "+pageNo*PageBlobUtils.PageSize+" block. DataSize: "+data.Length);
               
               Console.WriteLine(e.Message);
            }
            
        }

    }
}