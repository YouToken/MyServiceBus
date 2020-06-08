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
                    _packetExceptionHandler, ()=>_checkAndCreateTopics));
        }

        public MyServiceBusTcpClient PlugSocketLogs(Action<object> callback)
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
                new SubscriberInfo(topicId, queueId, deleteOnDisconnect, callback));
        }


        public void PublishFireAndForget(string topicId, byte[] valueToPublish)
        {
            var item = (MyServiceBusTcpContext) _clientTcpSocket.CurrentTcpContext;

            if (item == null)
                throw new Exception("No active connection");


            item.PublishFireAndForget(topicId, new[] {valueToPublish});
        }
        
        public void PublishFireAndForget(string topicId, IEnumerable<byte[]> valueToPublish)
        {
            var item = (MyServiceBusTcpContext) _clientTcpSocket.CurrentTcpContext;

            if (item == null)
                throw new Exception("No active connection");


            item.PublishFireAndForget(topicId, valueToPublish);
        }

        public Task PublishAsync(string topicId, byte[] valueToPublish, bool persistImmediately)
        {

            return PublishAsync(topicId, new[] {valueToPublish}, persistImmediately);
        }
        
        public Task PublishAsync(string topicId, IReadOnlyList<byte[]> valueToPublish, bool persistImmediately)
        {

            var item = (MyServiceBusTcpContext)_clientTcpSocket.CurrentTcpContext;
            
            if (item == null)
                throw new Exception("No active connection");


            return item.PublishAsync(topicId, valueToPublish, persistImmediately);
        }
        
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