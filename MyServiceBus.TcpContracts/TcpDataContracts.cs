using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MyServiceBus.Abstractions;
using MyTcpSockets.Extensions;

namespace MyServiceBus.TcpContracts
{

    public class UnsupportedPacketVersionException : Exception
    {

    }

    
    public interface IServiceBusTcpContract
    {
        void Serialize(Stream stream, int protocolVersion, int packetVersion);
        ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion);
    }

    public class PingContract : IServiceBusTcpContract
    {
        
        public static readonly PingContract Instance = new PingContract();
            
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
        }

        public ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            return new ValueTask(); 
        }
    }

    public class PongContract : IServiceBusTcpContract
    {
        public static readonly PongContract Instance = new PongContract();
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
         
        }

        public ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            return new ValueTask(); 
        }
    }

    
    public class GreetingContract : IServiceBusTcpContract
    {
        public string Name { get; set; }
        public int ProtocolVersion { get; set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(Name);
            stream.WriteInt(ProtocolVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            Name = await dataReader.ReadPascalStringAsync();
            ProtocolVersion = await dataReader.ReadIntAsync();
        }
    }
    
    public class PublishContract : IServiceBusTcpContract
    {
        public string TopicId { get; set; }
        public long RequestId { get; set; }
        
        public byte ImmediatePersist { get; set; }
        
        public IReadOnlyList<byte[]> Data { get; set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WriteLong(RequestId, protocolVersion);
            stream.WriteListOfByteArray(Data);
            stream.WriteByte(ImmediatePersist);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            RequestId = await dataReader.ReadLongAsync(protocolVersion);
            Data = await dataReader.ReadListOfByteArrayAsync();
            ImmediatePersist = await dataReader.ReadByteAsync();
        }
    }    
    
    
    public class PublishResponseContract : IServiceBusTcpContract
    {
        public long RequestId { get; set; }

        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WriteLong(RequestId, protocolVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {

            RequestId = await dataReader.ReadLongAsync(protocolVersion);
        }
    } 
    
    public class SubscribeContract : IServiceBusTcpContract
    {
        public string TopicId { get; set; }
        public string QueueId { get; set; }
        public bool DeleteOnDisconnect { get; set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WritePascalString(QueueId);
            stream.WriteByte(DeleteOnDisconnect ? (byte)1 : (byte)0);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            QueueId = await dataReader.ReadPascalStringAsync();
            DeleteOnDisconnect = await dataReader.ReadByteAsync() == 1;
        }
    }    
    
    public class SubscribeResponseContract : IServiceBusTcpContract
    {
        public string TopicId { get; set; }
        public string QueueId { get; set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WritePascalString(QueueId);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            QueueId = await dataReader.ReadPascalStringAsync();
        }
    }



    
    public class NewMessageContract : IServiceBusTcpContract
    {
        public class NewMessageData : IServiceBusTcpContract, IMyServiceBusMessage
        {
            public long Id { get; set; }
            public int AttemptNo { get; set; }
            
            public ReadOnlyMemory<byte> Data { get; set; }
            
            
            public void Serialize(Stream stream, int protocolVersion, int packetVersion)
            {
                if (packetVersion == 0)
                {
                    stream.WriteLong(Id, protocolVersion);
                    stream.WriteByteArray(Data.Span);
                }
                else if (packetVersion == 1)
                {
                    stream.WriteLong(Id, protocolVersion);
                    stream.WriteInt(AttemptNo);
                    stream.WriteByteArray(Data.Span);
                }
                else
                {
                    throw new UnsupportedPacketVersionException();
                }
            }

            public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
            {
                if (packetVersion == 0)
                {
                    Id = await dataReader.ReadLongAsync(protocolVersion);
                    Data = await dataReader.ReadByteArrayAsync();
                }
                else
                if (packetVersion == 1)
                {
                    Id = await dataReader.ReadLongAsync(protocolVersion);
                    AttemptNo = await dataReader.ReadIntAsync();
                    Data = await dataReader.ReadByteArrayAsync();
                }
                else
                {
                    throw new UnsupportedPacketVersionException();
                }

            }

        }
        
        public string TopicId { get; set; }
        public string QueueId { get; set; }
        
        public long ConfirmationId { get; set; }
        public IEnumerable<NewMessageData> Data { get; set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WritePascalString(QueueId);
            stream.WriteLong(ConfirmationId, protocolVersion);
            stream.WriteArrayOfItems(Data, protocolVersion, packetVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            QueueId = await dataReader.ReadPascalStringAsync();
            ConfirmationId = await dataReader.ReadLongAsync(protocolVersion);
            Data = await dataReader.ReadArrayOfItemsAsync<NewMessageData>(protocolVersion, packetVersion);
        }
    }


    
    public class NewMessageConfirmationContract : IServiceBusTcpContract
    {
        public string TopicId { get;  set; }
        public string QueueId { get;  set; }
 
        public long ConfirmationId { get;  set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WritePascalString(QueueId);
            stream.WriteLong(ConfirmationId, protocolVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            QueueId = await dataReader.ReadPascalStringAsync();
            ConfirmationId = await dataReader.ReadLongAsync(protocolVersion);
        }
    }


    public class CreateTopicIfNotExistsContract : IServiceBusTcpContract
    {
        public string TopicId { get;  set; }
        public long MaxMessagesInCache { get; set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WriteLong(MaxMessagesInCache, protocolVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            MaxMessagesInCache = await dataReader.ReadLongAsync(protocolVersion);
        }
    }

    public class MessagesInterval : IServiceBusTcpContract
    {
        public long FromId { get; set; }
        public long ToId { get; set; }

        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WriteLong(FromId, protocolVersion);
            stream.WriteLong(ToId, protocolVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            FromId = await dataReader.ReadLongAsync(protocolVersion);
            ToId = await dataReader.ReadLongAsync(protocolVersion);
        }
    }
    
    public class MessagesConfirmationContract : IServiceBusTcpContract
    {
        public string TopicId { get;  set; }
        public string QueueId { get;  set; }
        
        public IReadOnlyList<MessagesInterval> Ok { get; set; }
        public IReadOnlyList<MessagesInterval> NotOk { get; set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WritePascalString(QueueId);
            stream.WriteArrayOfItems(Ok, protocolVersion, packetVersion);
            stream.WriteArrayOfItems(NotOk, protocolVersion, packetVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            QueueId = await dataReader.ReadPascalStringAsync();

            Ok = await dataReader.ReadArrayOfItemsAsync<MessagesInterval>(protocolVersion,packetVersion);
            NotOk = await dataReader.ReadArrayOfItemsAsync<MessagesInterval>(protocolVersion,packetVersion);
        }
    }

    
    public class MessagesConfirmationAsFailContract : IServiceBusTcpContract
    {
        public string TopicId { get;  set; }
        public string QueueId { get;  set; }
        public long ConfirmationId { get;  set; }
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            stream.WritePascalString(TopicId);
            stream.WritePascalString(QueueId);
            stream.WriteLong(ConfirmationId, protocolVersion);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            TopicId = await dataReader.ReadPascalStringAsync();
            QueueId = await dataReader.ReadPascalStringAsync();
            ConfirmationId = await dataReader.ReadLongAsync(protocolVersion);
        }
    }

    
    public class PacketVersionsContract : IServiceBusTcpContract
    {
        
        private readonly Dictionary<byte, int> _versions = new Dictionary<byte, int>();

        public void SetPacketVersion(CommandType type, int version)
        {
            _versions.Add((byte)type, version);
        }


        public IEnumerable<KeyValuePair<byte, int>> GetPackets()
        {
            return _versions;
        }
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
           stream.WriteByteFromStack((byte) _versions.Count);

           foreach (var version in _versions)
           {
               stream.WriteByteFromStack(version.Key);
               stream.WriteInt(version.Value);
               
           }
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            var count = await dataReader.ReadByteAsync();

            for (byte i = 0; i < count; i++)
            {
                var key = await dataReader.ReadByteAsync();
                var value = await dataReader.ReadIntAsync();
                _versions.Add(key, value);
            }
        }
        
    }


    public class RejectConnectionContract : IServiceBusTcpContract
    {
        
        public string Message { get; set; }
        
        
        public void Serialize(Stream stream, int protocolVersion, int packetVersion)
        {
            
            stream.WritePascalString(Message);
        }

        public async ValueTask DeserializeAsync(TcpDataReader dataReader, int protocolVersion, int packetVersion)
        {
            Message = await dataReader.ReadPascalStringAsync();
        }


        public static RejectConnectionContract Create(string message)
        {
            return new RejectConnectionContract
            {
                Message = message
            };
        }
    }
    
}