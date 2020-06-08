using System.IO;

namespace MyServiceBus.Persistence.AzureStorage
{
    public static class StreamUtils
    {

        public static void WriteLong(this Stream stream, long value)
        {
            var b = (byte) value;
            stream.WriteByte(b);

            value >>= 8;
            b = (byte)value;
            stream.WriteByte(b);

            value >>= 8;
            b = (byte)value;
            stream.WriteByte(b);

            value >>= 8;
            b = (byte)value;
            stream.WriteByte(b);
        }

        public static void WriteInt(this Stream stream, int value)
        {
            var b = (byte) value;
            stream.WriteByte(b);

            value >>= 8;
            b = (byte)value;
            stream.WriteByte(b);

            value >>= 8;
            b = (byte)value;
            stream.WriteByte(b);

            value >>= 8;
            b = (byte)value;
            stream.WriteByte(b);
        }


        public static void WriteFromStream(this MemoryStream destStream, MemoryStream srcStream, int copySize)
        {
            var buffer = srcStream.GetBuffer();
                
            destStream.Write(buffer, (int)srcStream.Position, copySize);

            srcStream.Position += copySize;
        }
        
        public static int ReadInt(this Stream data)
        {
            return data.ReadByte() + data.ReadByte() * 256 + data.ReadByte() * 65536 + data.ReadByte() * 16777216;
        }
    }
}