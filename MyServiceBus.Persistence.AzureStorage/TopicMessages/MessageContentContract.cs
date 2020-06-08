using System;
using System.IO;
using System.Runtime.Serialization;
using MyServiceBus.Domains.MessagesContent;

namespace MyServiceBus.Persistence.AzureStorage.TopicMessages
{
    [DataContract]
    public class MessageContentBlobContract : IMessageContent
    {
        [DataMember(Order = 1)]
        public long MessageId { get; set; }

        [DataMember(Order = 2)]
        public DateTime Created { get; set; }
        
        [DataMember(Order = 3)]
        public byte[] Data { get; set; }

        public static MessageContentBlobContract Create(IMessageContent src)
        {
            return new MessageContentBlobContract
            {
                MessageId = src.MessageId,
                Created = src.Created,
                Data = src.Data
            };
        }

    }


    public static class MessageContentContractSerializer
    {
        
        private static readonly byte[] Header = new byte[4];

        public static byte[] SerializeContract(this IMessageContent src)
        {
            var contract = MessageContentBlobContract.Create(src);
            
            var memStream = new MemoryStream();
            memStream.Write(Header);
            
            ProtoBuf.Serializer.Serialize(memStream, contract);

            var result = memStream.ToArray();
            
            result.WriteInt(0, result.Length-4);

            return result;
        }
        
    }
}