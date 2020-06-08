using System;
using System.IO;
using System.Threading.Tasks;
using MyServiceBus.Persistence.AzureStorage.PageBlob;

namespace MyServiceBus.Persistence.AzureStorage
{
    public static class BlobProtobufReader
    {

        private const int HeaderLen = 4;
        
        private static readonly byte[] Header = {0, 0, 0, 0};

        public static async Task<T> ReadAndDeserializeAsProtobufAsync<T>(this IPageBlob pageBlob)
        {
            try
            {
                var data = await pageBlob.DownloadAsync();
                data.Position = 0;
                var size = data.ReadInt();
                data.SetLength(size + 4);
                var result = ProtoBuf.Serializer.Deserialize<T>(data);
                return result;

            }
            catch (Exception)
            {
                return default;
            }
        }
        
        public static async Task WriteAsProtobufAsync(this IPageBlob pageBlob, object instance)
        {
            var stream = new MemoryStream();
            stream.Write(Header);
            ProtoBuf.Serializer.Serialize(stream, instance);
            stream.Position = 0;
            stream.WriteInt((int)stream.Length -4);
            await pageBlob.WriteAsync(stream.GetDataReadyForPageBlob(), 0);
        }
        
        
        
    }
}