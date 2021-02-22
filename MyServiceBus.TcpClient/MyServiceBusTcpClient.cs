using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.TcpClient
{
    public class MyServiceBusTcpClient
    {

        private readonly MyClientTcpSocket<IServiceBusTcpContract> _clientTcpSocket;


        private readonly List<(string topicName, int maxCachedSize)> _checkAndCreateTopics = new List<(string topicName, int maxCachedSize)>();


        private Action<object> _packetExceptionHandler;
        
        public MyServiceBusTcpClient(Func<string> getHostPort, string name)
        {
            _clientTcpSocket = new MyClientTcpSocket<IServiceBusTcpContract>(
                    getHostPort,
                    TimeSpan.FromSeconds(3)
                )
                .RegisterTcpSerializerFactory(()=>new MyServiceBusTcpSerializer())
                .RegisterTcpContextFactory(() => new MyServiceBusTcpContext(_subscribers, name, 
                    _payLoadCollector, _packetExceptionHandler,
                    ()=>_checkAndCreateTopics));
        }


        private bool _throwExceptionIfPublishNoConnection;
        public MyServiceBusTcpClient ThrowExceptionOnPublishIfNoConnection(bool throwExceptionIfPublishNoConnection)
        {
            _throwExceptionIfPublishNoConnection = throwExceptionIfPublishNoConnection;
            return this;
        }

        public MyServiceBusTcpClient PlugSocketLogs(Action<ITcpContext, object> callback)
        {
            _clientTcpSocket.AddLog(callback);
            return this;
        }


        public MyServiceBusTcpClient PlugPacketHandleExceptions(Action<object> callback)
        {
            _packetExceptionHandler = callback;
            return this;
        }
        

        public MyServiceBusTcpClient CreateTopicIfNotExists(string topicName, int maxCachedSize)
        {
            _checkAndCreateTopics.Add((topicName, maxCachedSize));
            return this;
        }

        private readonly Dictionary<string, SubscriberInfo> _subscribers = new Dictionary<string, SubscriberInfo>();


        public void Subscribe(string topicId, string queueId, bool deleteOnDisconnect,
            Func<IMyServiceBusMessage, ValueTask> callback)
        {
            var id = MyServiceBusTcpContext.GetId(topicId, queueId);

            _subscribers.Add(id,
                new SubscriberInfo(topicId, queueId, deleteOnDisconnect, callback, null));
        }
        
        public void Subscribe(string topicId, string queueId, bool deleteOnDisconnect,
            Func<IReadOnlyList<IMyServiceBusMessage>, ValueTask> callback)
        {
            var id = MyServiceBusTcpContext.GetId(topicId, queueId);

            _subscribers.Add(id,
                new SubscriberInfo(topicId, queueId, deleteOnDisconnect, null, callback));
        }


        public Task PublishAsync(string topicId, byte[] valueToPublish, bool immediatelyPersist)
        {
            var connection = (MyServiceBusTcpContext) _clientTcpSocket.CurrentTcpContext;

            if (_throwExceptionIfPublishNoConnection)
            {
                if (connection == null)
                    throw new Exception("No active connection");
            }

            var result = _payLoadCollector.AddMessage(connection.Id, topicId, valueToPublish, immediatelyPersist);

            var nextPayloadToPublish = _payLoadCollector.GetNextPayloadToPublish();
            
            if (nextPayloadToPublish != null)
                connection.Publish(nextPayloadToPublish);
            
            return result;
        }
        
        public Task PublishAsync(string topicId, IEnumerable<byte[]> valueToPublish, bool immediatelyPersist)
        {
            var connection = (MyServiceBusTcpContext)_clientTcpSocket.CurrentTcpContext;
            
            if (_throwExceptionIfPublishNoConnection)
            {
                if (connection == null)
                    throw new Exception("No active connection");
            }

            var result = _payLoadCollector.AddMessage(connection.Id, topicId, valueToPublish, immediatelyPersist);

            var nextPayloadToPublish = _payLoadCollector.GetNextPayloadToPublish();

            if (nextPayloadToPublish != null)
                connection.Publish(nextPayloadToPublish);

            return result;
        }

        private readonly PayLoadCollector _payLoadCollector = new PayLoadCollector(1024*1024*5);
        
        public void Start()
        {
            _clientTcpSocket.Start();
        }

        public void Stop()
        {
            _clientTcpSocket.Stop();
        }

    }
}