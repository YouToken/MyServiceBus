using System.IO;

namespace MyServiceBus.Persistence.AzureStorage
{

    public interface ISequenceReader
    {
        byte ReadByte();
    }
    
    
    public static class ArrayUtils
    {
        public static MemoryStream ToMemoryStream(this byte[] src)
        {
            var result = new MemoryStream();
            result.Write(src);
            result.Position = 0;
            return result;
        }
    }
}