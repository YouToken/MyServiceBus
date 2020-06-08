using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyTcpSockets.Extensions;

namespace MyServiceBus.TcpContracts
{
    public static class DataContractUtils
    {

        public static void WriteLong(this Stream stream, long value, long protocolVersion)
        {
            if (protocolVersion < 2)
            {
                stream.WriteInt((int)value);
                return;
            }
            
            stream.WriteLong(value);
        }
        
        public static async ValueTask<long> ReadLongAsync(this TcpDataReader dataReader, long protocolVersion)
        {
            if (protocolVersion < 2)
                return await dataReader.ReadIntAsync();

            return await dataReader.ReadLongAsync();
        }
        
        
        public static void WriteArrayOfItems<T>(this Stream stream, IEnumerable<T> src, int protocolVersion, int packetVersion) 
            where T:IServiceBusTcpContract
        {
            var list = src.AsReadOnlyList();
            stream.WriteInt(list.Count);

            foreach (var item in list)
                item.Serialize(stream, protocolVersion, packetVersion);
        }
        
        public static async Task<IReadOnlyList<T>> ReadArrayOfItemsAsync<T>(this TcpDataReader reader,  int protocolVersion, int packetVersion) 
            where T:IServiceBusTcpContract, new()
        {
            var len = await reader.ReadIntAsync();

            var result = new List<T>();

            for (var i = 0; i < len; i++)
            {
                var itm = new T();
                await itm.DeserializeAsync(reader, protocolVersion, packetVersion);
                result.Add(itm);
            }

            return result;
        }

        public static void WriteListOfByteArray(this Stream stream, in IReadOnlyList<byte[]> src)
        {
            stream.WriteInt(src.Count);

            foreach (var itm in src)
            {
                stream.WriteByteArray(itm);
            }
        }

        public static async ValueTask<IReadOnlyList<byte[]>> ReadListOfByteArrayAsync(this TcpDataReader reader)
        {

            var dataLen = await reader.ReadIntAsync();

            var result = new List<byte[]>();
            for (var i = 0; i < dataLen; i++)
            {
                var data = await reader.ReadByteArrayAsync();
                result.Add(data.ToArray());
            }

            return result;
        }
        
        
    }
}