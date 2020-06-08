using System.IO;

namespace MyServiceBus.Persistence.AzureStorage
{

    public interface ISequenceReader
    {
        byte ReadByte();
    }
    
    
    public static class ArrayUtils
    {

        public static int ReadInt(this ISequenceReader sequenceReader)
        {
            return (sequenceReader.ReadByte() +
                          sequenceReader.ReadByte() * 256 + 
                          sequenceReader.ReadByte() * 65536 + 
                          sequenceReader.ReadByte() * 16777216);
        }
        
        
        public static uint ReadUInt(this byte[] data, int offset)
        {
            return (uint)(data[offset++] + data[offset++] * 256 + data[offset++] * 65536 + data[offset] * 16777216);
        }

        public static int ReadInt(this byte[] data, int offset)
        {
            return (int) ReadUInt(data, offset);
        }
        
        public static int ReadInt(this byte[] data, int offset, byte[] data2)
        {
            int result = data[offset++];

            if (offset >= data.Length)
            {
                data = data2;
                offset = 0;
            }
            
            result += data[offset++] * 256;
            
            if (offset >= data.Length)
            {
                data = data2;
                offset = 0;
            }


            result += data[offset++] * 65536;
            
            if (offset >= data.Length)
            {
                data = data2;
                offset = 0;
            }

            result += result + data[offset] * 16777216;
            
            return result;
            
        }

        public static long ReadLong(this byte[] data, int offset)
        {
            return (uint)(data[offset++] 
                          + data[offset++] * (long)256 
                          + data[offset++] * (long)65536 
                          + data[offset] * (long)16777216
                          + data[offset] * 4294967296
                          + data[offset] * 1099511627776
                          + data[offset] * 281474976710656
                          + data[offset] * 7.205759403792794e16
                          );
            
        }
        
        public static void WriteInt(this byte[] src, int offset, int value)
        {
            src[offset++] = (byte) value;
            
            value >>= 8;
            src[offset++] = (byte) value;
            
            value >>= 8;
            src[offset++] = (byte) value;
            
            value >>= 8;
            src[offset] = (byte) value;
        }


        public static MemoryStream ToMemoryStream(this byte[] src)
        {
            var result = new MemoryStream();
            result.Write(src);
            result.Position = 0;
            return result;
        }
    }
}