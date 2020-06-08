using MyServiceBus.Persistence.AzureStorage;
using MyServiceBus.Persistence.AzureStorage.PageBlob;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestBlobFeatures
    {


        [Test]
        public void TestBasicBlobWriting()
        {

            var src = new byte[500];

            for(var i=0; i<src.Length; i++)
                src[i] = (byte) i;
            
            var blobInMem = new PageBlobInMem();

            blobInMem.WriteBytesAsync(0, src.ToMemoryStream());
            
            Assert.AreEqual(512, blobInMem.GetBlobSizeAsync().Result);
            
            var src2 = new byte[3000];
            for(var i=0; i<src2.Length; i++)
                src2[i] = (byte) (500+i);
            
            
            blobInMem.WriteBytesAsync(500, src2.ToMemoryStream());
            
            Assert.AreEqual(3584, blobInMem.GetBlobSizeAsync().Result);

            var data = blobInMem.DownloadAsync().Result.ToArray();

            for (var i = 0; i < 3500; i++)
            {
                Assert.AreEqual(data[i], (byte) i);
            }
        }
        
        
        [Test]
        public void TestWritingByteByByte()
        {
            
            var blobInMem = new PageBlobInMem();

            for (int i = 0; i < 3500; i++)
            {
                blobInMem.WriteBytesAsync(i, new [] {(byte)i}.ToMemoryStream());
            }
            
            Assert.AreEqual(3584, blobInMem.GetBlobSizeAsync().Result);
            
            var data = blobInMem.DownloadAsync().Result.ToArray();

            for (var i = 0; i < 3500; i++)
            {
                Assert.AreEqual(data[i], (byte) i);
            }
        }
        
        [Test]
        public void TestRandomWritingReading()
        {

            const int MaxArraySize = 1000;
            
            var blobInMem = new PageBlobInMem();

            var offset = 0;
            for (var i = 1; i < MaxArraySize; i++)
            {
                var data = new byte[i*3];

                for (var j = 0; j < data.Length; j++)
                {
                    data[j] = (byte) i;
                }

                blobInMem.WriteBytesAsync(offset, data.ToMemoryStream());

                offset += data.Length;
            }

            var resultData = blobInMem.DownloadAsync().Result.ToArray();

            offset = 0;
            
            for (var i = 1; i < MaxArraySize; i++)
            {
                for (var j = 0; j < i*3; j++)
                {
                    Assert.AreEqual(resultData[offset], (byte) i);
                    offset++;
                }
            }
            
        }
    }
}