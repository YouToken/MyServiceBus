using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.TcpContracts;

namespace MyServiceBus.Server.Tcp
{
    public static class Mappers
    {

        public static NewMessageContract.NewMessageData ToMessageData(this IMessageContent messageContent, int attemptId)
        {
            
            return new ()
            {
                Id = messageContent.MessageId,
                Data = messageContent.Data,
                AttemptNo = attemptId
            };
        }
        
    }
}