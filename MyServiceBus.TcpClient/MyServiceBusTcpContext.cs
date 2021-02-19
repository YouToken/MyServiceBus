using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.TcpClient
{
    public class MyServiceBusTcpContext : ClientTcpContext<IServiceBusTcpContract>
    {
        
        private readonly Dictionary<long, TaskCompletionSource<int>> _tasks 
            = new Dictionary<long, TaskCompletionSource<int>>();

        private readonly Dictionary<string, SubscriberInfo> _subscribers;
        private readonly string _name;
        private readonly Func<IReadOnlyList<(string topicName, int maxCachedSize)>> _checkAndCreateTopicOnConnect;

        private readonly Action<object> _packetExceptionHandler;
        public MyServiceBusTcpContext(Dictionary<string, SubscriberInfo> subscribers, string name, 
            Action<object> packetExceptionHandler,
            Func<IReadOnlyList<(string topicName, int maxCachedSize)>> checkAndCreateTopicOnConnect)
        {
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

        private IReadOnlyList<KeyValuePair<long, TaskCompletionSource<int>>> GetTasks()
        {
            lock (_lockObject)
            {
                _disconnected = true;
                return _tasks.ToList();
            }
        }

        private bool _disconnected;
        
        protected override ValueTask OnDisconnectAsync()
        {

            var tasks = GetTasks();

            foreach (var (key, value) in tasks)
            {
                value.SetException(new Exception("Disconnected"));
                
                lock (_lockObject)
                    if (_tasks.ContainsKey(key))
                        _tasks.Remove(key);
            }


            return new ValueTask();
        }

        protected override async ValueTask HandleIncomingDataAsync(IServiceBusTcpContract data)
        {

            try
            {
                switch (data)
                {
                
                    case NewMessageContract newMsg:
                        await NewMessageAsync(newMsg);
                        return;
                
                    case PublishResponseContract pr:
                        PublishResponse(pr);
                        return;
                
                    case RejectConnectionContract rejectContract:
                        throw new Exception("Reject from server: "+rejectContract.Message);
                
                }
            }
            catch (Exception e)
            {
                _packetExceptionHandler?.Invoke(e);
            }
        }
        
        private void PublishResponse(PublishResponseContract pr)
        {
            var task = RemoveTask(pr.RequestId);

            task?.SetResult(0);

        }
        
        private void SendSubscribe(string topicId, string queueId, bool deleteOnDisconnect)
        {
            var contract = new SubscribeContract
            {
                TopicId = topicId,
                QueueId = queueId,
                DeleteOnDisconnect = deleteOnDisconnect
            };
            
            SendPacketAsync(contract);
        }

        
        public static string GetId(string topicId, string queueId)
        {
            return topicId + "|" + queueId;
        }


        private void CallbackAsPackage(NewMessageContract newMsg,
            Func<IReadOnlyList<IMyServiceBusMessage>, ValueTask> callback)
        {

            Task.Run(async () =>
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
            });
        }


        private void CallbackOneByOne(NewMessageContract newMsg,
            Func<IMyServiceBusMessage, ValueTask> callback)
        {

            Task.Run(async () =>
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

                SendMessageConfirmation(newMsg);
            });

        }


        private ValueTask NewMessageAsync(NewMessageContract newMsg)
        {
            var id = GetId(newMsg.TopicId, newMsg.QueueId);

            var subscriber = _subscribers[id];

            if (subscriber.CallbackAsAPackage != null)
                CallbackAsPackage(newMsg, subscriber.CallbackAsAPackage);

            if (subscriber.CallbackAsOneMessage != null)
                CallbackOneByOne(newMsg, subscriber.CallbackAsOneMessage);

            return new ValueTask();
        }
        
        private void SendMessageConfirmation(NewMessageContract messages)
        {
            var contract = new NewMessageConfirmationContract
            {
                TopicId = messages.TopicId,
                QueueId = messages.QueueId,
                ConfirmationId = messages.ConfirmationId
            };
            
            SendPacketAsync(contract);
        }
        
        private void SendMessageReject(NewMessageContract messages)
        {
            var contract = new MessagesConfirmationAsFailContract
            {
                TopicId = messages.TopicId,
                QueueId = messages.QueueId,
                ConfirmationId = messages.ConfirmationId
            };
            
            SendPacketAsync(contract);
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

            SendPacketAsync(contract);
        }


        private void SendPacketVersions()
        {
            var packetVersions = new PacketVersionsContract();
            packetVersions.SetPacketVersion(CommandType.NewMessage, 1);
            SendPacketAsync(packetVersions);
        }

        private void SetCreateTopicIfNotExistsOnConnect(string topicId, int maxCachedSize)
        {
            var contract = new CreateTopicIfNotExistsContract
            {
                TopicId = topicId,
                MaxMessagesInCache = maxCachedSize
            };

            SendPacketAsync(contract);
        }


        protected override IServiceBusTcpContract GetPingPacket()
        {
            return PingContract.Instance;
        }


        private static long _requestId;


        private (long taskId, TaskCompletionSource<int> Task) GetNewTask()
        {
            lock (_lockObject)
            {
                
                if (_disconnected)
                    throw new Exception("Socket is disconnected");
                
                var requestId = _requestId++;
                
                var tc = new TaskCompletionSource<int>();
                _tasks.TryAdd(requestId, tc);

                return (requestId, tc);
            }
        }
        


        private TaskCompletionSource<int> RemoveTask(long confirmationId)
        {
            lock (_lockObject)
            {

                if (_fireAndForgetTopics.ContainsKey(confirmationId))
                {
                    var topicId = _fireAndForgetTopics[confirmationId];
                    _fireAndForgetTopics.Remove(confirmationId);

                    var state = _publishAndForgetStates[topicId];

                    state.Confirmed(confirmationId);
                    SendNextDataChunk(state, topicId);
                }

                if (_tasks.ContainsKey(confirmationId))
                {
                    var task = _tasks[confirmationId];

                    if (!_tasks.ContainsKey(confirmationId)) return task;
                    _tasks.Remove(confirmationId);

                    return task;
                }
            }

            return null;
        }
        
        
        private readonly object _lockObject = new object();

        private static long GetNewRequestId()
        {
            _requestId++;
            return _requestId;
        }
        
        private readonly Dictionary<string, PublishAndForgetState> _publishAndForgetStates = new Dictionary<string, PublishAndForgetState>();
        private readonly Dictionary<long, string> _fireAndForgetTopics = new Dictionary<long, string>();
        
        private PublishAndForgetState GetFireAndForgetStateOrCreate(string topicId)
        {
            if (_publishAndForgetStates.ContainsKey(topicId))
                return _publishAndForgetStates[topicId];

            var newItem = new PublishAndForgetState();
            _publishAndForgetStates.Add(topicId, newItem);
            return newItem;
        }


        private void SendNextDataChunk(PublishAndForgetState state, string topicId)
        {

            var data = state.GetMessagesToSend();
            
            if (data.Count == 0)
                return;
            
            var requestId = GetNewRequestId();
                
            var contract = new PublishContract
            {
                TopicId = topicId,
                RequestId = requestId,
                Data = data,
                ImmediatePersist = 0
            };
                
            SendPacketAsync(contract);

            state.SetOnRequest(requestId);
            
            _fireAndForgetTopics.Add(requestId, topicId);
        }
        
        
        public void PublishFireAndForget(string topicId, IEnumerable<byte[]> data)
        {
            lock (_lockObject)
            {
                var state = GetFireAndForgetStateOrCreate(topicId);
                state.Enqueue(data);
                if (state.HasFireAndForgetRequests())
                    return;
                
                SendNextDataChunk(state, topicId);
            }
        }
        
        
        public async Task PublishAsync(string topicId, IReadOnlyList<byte[]> data, bool immediatelyPersist)
        {
            
            var contract = new PublishContract
            {
                TopicId = topicId,
                RequestId = _requestId,
                Data = data,
                ImmediatePersist = immediatelyPersist ? (byte)1 : (byte)0
            };

            var task = GetNewTask();

            try
            {
                await SendPacketAsync(contract);
                await task.Task.Task;
            }
            catch (Exception)
            {
                RemoveTask(contract.RequestId);
                throw;
            }
            
        }
        
    }
    
}