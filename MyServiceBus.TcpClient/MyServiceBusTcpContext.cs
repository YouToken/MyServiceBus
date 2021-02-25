using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.TcpClient
{
    public class MyServiceBusTcpContext : ClientTcpContext<IServiceBusTcpContract>
    {


        private readonly Dictionary<string, SubscriberInfo> _subscribers;
        private readonly string _name;
        private readonly Func<IReadOnlyList<(string topicName, int maxCachedSize)>> _checkAndCreateTopicOnConnect;

        private readonly Action<object> _packetExceptionHandler;


        private PayLoadCollector _payLoadCollector;
        
        public MyServiceBusTcpContext(Dictionary<string, SubscriberInfo> subscribers, string name, 
            PayLoadCollector payLoadCollector,
            Action<object> packetExceptionHandler,
            Func<IReadOnlyList<(string topicName, int maxCachedSize)>> checkAndCreateTopicOnConnect)
        {
            _payLoadCollector = payLoadCollector;
            _packetExceptionHandler = packetExceptionHandler;
            _subscribers = subscribers;
            _name = name;
            _checkAndCreateTopicOnConnect = checkAndCreateTopicOnConnect;
        }

        protected override ValueTask OnConnectAsync()
        {
            SendGreetings(_name);

            SendPacketVersions();

            foreach (var (topicId, maxCachedSize) in _checkAndCreateTopicOnConnect())
            {
                SetCreateTopicIfNotExistsOnConnect(topicId, maxCachedSize);
            }

            foreach (var subscriber in _subscribers.Values)
            {
                SendSubscribe(subscriber.TopicId, subscriber.QueueId, subscriber.DeleteOnDisconnect);
            }
            
            return new ValueTask();
        }


        
        protected override ValueTask OnDisconnectAsync()
        {
            _payLoadCollector.Disconnect(Id);
            return new ValueTask();
        }


        private void HandlePublishResponse(PublishResponseContract pr)
        {
            _payLoadCollector.SetPublished(pr.RequestId);

            var nextPackageToPublish = _payLoadCollector.GetNextPayloadToPublish();

            if (nextPackageToPublish != null)
                Publish(nextPackageToPublish);
        }

        protected override ValueTask HandleIncomingDataAsync(IServiceBusTcpContract data)
        {

            switch (data)
            {
                case NewMessageContract newMsg:
                    HandleNewMessages(newMsg);
                    break;

                case PublishResponseContract pr:
                    HandlePublishResponse(pr);
                    break;

                case RejectConnectionContract rejectContract:
                    throw new Exception("Reject from server: " + rejectContract.Message);
            }

            return new ValueTask();
        }


        private void SendSubscribe(string topicId, string queueId, bool deleteOnDisconnect)
        {
            var contract = new SubscribeContract
            {
                TopicId = topicId,
                QueueId = queueId,
                DeleteOnDisconnect = deleteOnDisconnect
            };
            
            SendDataToSocket(contract);
        }

        
        public static string GetId(string topicId, string queueId)
        {
            return topicId + "|" + queueId;
        }


        private async Task CallbackAsPackageAsync(NewMessageContract newMsg,
            Func<IReadOnlyList<IMyServiceBusMessage>, ValueTask> callback)
        {


                try
                {
                    await callback(newMsg.Data);
                    SendMessageConfirmation(newMsg);
                }
                catch (Exception e)
                {
                    SendMessageReject(newMsg);

                    WriteLog(
                        $"Bulk Messages Reject [{newMsg.Data[0]}-{newMsg.Data[^1]}]. Topic: {newMsg.TopicId}, Queue: {newMsg.QueueId}");
                    WriteLog(e);

                    _packetExceptionHandler?.Invoke(e);
                }
        }


        private async Task CallbackOneByOneAsync(NewMessageContract newMsg,
            Func<IMyServiceBusMessage, ValueTask> callback)
        {
            foreach (var msg in newMsg.Data)
            {
                try
                {
                    await callback(msg);
                }
                catch (Exception e)
                {
                    SendMessageReject(newMsg);

                    WriteLog(
                        $"Message Reject. Topic: {newMsg.TopicId}, Queue: {newMsg.QueueId}, MessageId: {msg.Id}; AttemptNo: {msg.AttemptNo}");
                    WriteLog(e);

                    _packetExceptionHandler?.Invoke(e);
                    return;
                }

            }

        }


        private void HandleNewMessages(NewMessageContract newMsg)
        {
            var id = GetId(newMsg.TopicId, newMsg.QueueId);

            var subscriber = _subscribers[id];
            
            subscriber.InvokeNewMessages(newMsg.Data);
        }
        
        private void SendMessageConfirmation(NewMessageContract messages)
        {
            var contract = new NewMessageConfirmationContract
            {
                TopicId = messages.TopicId,
                QueueId = messages.QueueId,
                ConfirmationId = messages.ConfirmationId
            };
            
            SendDataToSocket(contract);
        }
        
        private void SendMessageReject(NewMessageContract messages)
        {
            var contract = new MessagesConfirmationAsFailContract
            {
                TopicId = messages.TopicId,
                QueueId = messages.QueueId,
                ConfirmationId = messages.ConfirmationId
            };
            
            SendDataToSocket(contract);
        }
        
        
        private const int ProtocolVersion = 2; 
        
        
        private static readonly Lazy<string> GetClientVersion = new Lazy<string>(() =>
        {
            try
            {
                return typeof(MyServiceBusTcpContext).Assembly.GetName().Version.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        });
        
        
        
        private void SendGreetings(string name)
        {

            var clientVersion = GetClientVersion.Value;

            clientVersion = clientVersion == null
                ? string.Empty
                : ";ClientVersion:"+clientVersion;

            
            var contract = new GreetingContract
            {
                Name = name +clientVersion,
                ProtocolVersion = ProtocolVersion
            };

            SendDataToSocket(contract);
        }


        private void SendPacketVersions()
        {
            var packetVersions = new PacketVersionsContract();
            packetVersions.SetPacketVersion(CommandType.NewMessage, 1);
            SendDataToSocket(packetVersions);
        }

        private void SetCreateTopicIfNotExistsOnConnect(string topicId, int maxCachedSize)
        {
            var contract = new CreateTopicIfNotExistsContract
            {
                TopicId = topicId,
                MaxMessagesInCache = maxCachedSize
            };

            SendDataToSocket(contract);
        }


        protected override IServiceBusTcpContract GetPingPacket()
        {
            return PingContract.Instance;
        }


        public void Publish(PayloadPackage payloadPackage)
        {

            var contract = new PublishContract
            {
                TopicId = payloadPackage.TopicId,
                RequestId = payloadPackage.RequestId,
                Data = payloadPackage.PayLoads,
                ImmediatePersist = payloadPackage.ImmediatelyPersist ? (byte) 1 : (byte) 0
            };

            SendDataToSocket(contract);

        }

    }
    
}