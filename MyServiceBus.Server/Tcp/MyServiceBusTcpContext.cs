using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.Domains.Execution;
using MyServiceBus.Domains.Queues;
using MyServiceBus.Domains.QueueSubscribers;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Persistence.Grpc;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.Server.Tcp
{
    public class MyServiceBusTcpContext : TcpContext<IServiceBusTcpContract>, IMyServiceBusSession
    {
        private ValueTask ExecuteConfirmAsync(string topicId, string queueId, long confirmationId, bool ok)
        {

            var topic = ServiceLocator.TopicsList.TryGet(topicId);

            if (topic != null)
                return ServiceLocator.Subscriber.ConfirmDeliveryAsync(topic, queueId, confirmationId, ok);
            
            Console.WriteLine($"There is a confirmation {confirmationId} for a topic {topicId} which is not found");
            Disconnect();

            return new ValueTask();
        }

        private ValueTask ExecuteSomeMessagesAreOkSomeFail(ConfirmSomeMessagesOkSomeFail packet)
        {
            var topic = ServiceLocator.TopicsList.TryGet(packet.TopicId);

            if (topic != null)
            {
                var okMessages = new QueueWithIntervals(packet.OkMessages);
                return ServiceLocator.Subscriber.ConfirmDeliveryAsync(topic, packet.QueueId, packet.ConfirmationId, okMessages);
            }
            
            Console.WriteLine($"There is a confirmation {packet.ConfirmationId} for a topic {packet.TopicId}/{packet.QueueId} which is not found");
            Disconnect();

            return new ValueTask();
        }

        private async ValueTask PublishAsync(PublishContract contract)
        {

            if (Session == null)
            {
                Console.WriteLine($"Trying to publish to topic {contract.TopicId} with no active Session");
                Disconnect();
                return;
            }

            Session.PublishPacketsInternal++;

            var now = DateTime.UtcNow;

            var response = await ServiceLocator
                .MyServiceBusPublisher
                .PublishAsync(Session, contract.TopicId, contract.Data, now, contract.ImmediatePersist == 1);

            if (response != ExecutionResult.Ok)
            {
                Console.WriteLine("Can not publish the message. Reason: " + response);
                Disconnect();
                return;
            }

            var resp = new PublishResponseContract
            {
                RequestId = contract.RequestId
            };

            SendDataToSocket(resp);
        }

        public MyServiceBusSession Session { get; private set; }


        private static readonly Dictionary<int, int> AcceptedProtocolVersions = new ()
        {
            [1] = 1,
            [2] = 2
        };

        private static string GetAcceptedProtocolVersions()
        {
            var result = new StringBuilder();

            foreach (var key in AcceptedProtocolVersions.Keys)
            {
                if (result.Length > 0)
                    result.Append(',');
                result.Append($"{key}");
            }

            return result.ToString();
        }

        private ValueTask GreetingAsync(GreetingContract greetingContract)
        {

            if (!AcceptedProtocolVersions.ContainsKey(greetingContract.ProtocolVersion))
            {
                Console.WriteLine(greetingContract.Name + $" is attempting to connect with invalid protocol version {greetingContract.ProtocolVersion}. Acceptable versions are {GetAcceptedProtocolVersions()}");
                Disconnect();
            }

            Session = ServiceLocator.SessionsList.NewSession(greetingContract.Name,
                TcpClient.Client.RemoteEndPoint?.ToString() ?? "unknownIP", DateTime.UtcNow, TimeSpan.FromSeconds(30), greetingContract.ProtocolVersion, SessionType.Tcp);

            SetContextName(greetingContract.Name);
            ServiceLocator.TcpConnectionsSnapshotId++;
            return new ValueTask();

        }

        private void ExecuteSubscribe(SubscribeContract contract)
        {
            Console.WriteLine("Subscribed to topic: " + contract.TopicId + " with queue: " + contract.QueueId);

            if (Session == null)
            {
                Console.WriteLine($"Client with IP {TcpClient.Client.RemoteEndPoint} is trying to subscribe to topic {contract.TopicId} but it has not sent greeting message yet");
                Disconnect();
                return;
            }

            var topic = ServiceLocator.TopicsList.TryGet(contract.TopicId);

            if (topic == null)
            {
                Console.WriteLine($"Client {Session.Name} is trying to subscribe to the topic {contract.TopicId} which does not exists");
                Disconnect();
                return;
            }

            var queue = topic.CreateQueueIfNotExists(contract.QueueId, contract.DeleteOnDisconnect);
            Session?.SubscribeToQueue(queue);

            ServiceLocator.Subscriber.SubscribeToQueueAsync(queue, this);

        }

        protected override ValueTask OnConnectAsync()
        {

            Console.WriteLine("Connected: " + TcpClient.Client.RemoteEndPoint);
            ServiceLocator.TcpConnectionsSnapshotId++;
            return new ValueTask();
        }

        protected override ValueTask OnDisconnectAsync()
        {
            Session?.Disconnect();

            Console.WriteLine("Disconnected: " + ContextName);
            ServiceLocator.TcpConnectionsSnapshotId++;
            return ServiceLocator.Subscriber.DisconnectSubscriberAsync(this);

        }

        protected override ValueTask HandleIncomingDataAsync(IServiceBusTcpContract data)
        {

            try
            {
                Session?.UpdateLastAccess();


                switch (data)
                {
                    case PingContract _:
                        SendDataToSocket(PongContract.Instance);
                        return new ValueTask();

                    case SubscribeContract subscribeContract:
                        ExecuteSubscribe(subscribeContract);
                        return new ValueTask();

                    case PublishContract publishContract:
                        return PublishAsync(publishContract);

                    case GreetingContract greetingContract:
                        return GreetingAsync(greetingContract);

                    case NewMessageConfirmationContract confirmRequestContract:
                        return ExecuteConfirmAsync(confirmRequestContract.TopicId, confirmRequestContract.QueueId, confirmRequestContract.ConfirmationId, true);
                    
                    case MessagesConfirmationAsFailContract fail:
                        return ExecuteConfirmAsync(fail.TopicId, fail.QueueId, fail.ConfirmationId, false);

                    case CreateTopicIfNotExistsContract createTopicIfNotExistsContract:
                        return new ValueTask(CreateTopicIfNotExistsAsync(createTopicIfNotExistsContract));
                    
                    case ConfirmSomeMessagesOkSomeFail confirmSomeMessagesOkSomeFail:
                        return ExecuteSomeMessagesAreOkSomeFail(confirmSomeMessagesOkSomeFail);

                    default:
                        return new ValueTask();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


        }

        private Task CreateTopicIfNotExistsAsync(CreateTopicIfNotExistsContract createTopicIfNotExistsContract)
        {

            Console.WriteLine($"Attempt to create topic {createTopicIfNotExistsContract.TopicId} with max cached amount {createTopicIfNotExistsContract.MaxMessagesInCache}");


            if (Session == null)
            {
                Console.WriteLine("Session is not initialized from creating topic: " + createTopicIfNotExistsContract.TopicId);
            }

            Session?.PublishToTopic(createTopicIfNotExistsContract.TopicId);


            ServiceLocator.TopicsManagement.AddIfNotExistsAsync(createTopicIfNotExistsContract.TopicId);
            return Task.CompletedTask;
        }

        public void SendMessagesAsync(TopicQueue topicQueue,
            IReadOnlyList<(MessageContentGrpcModel message, int attemptNo)> messages, long confirmationId)
        {
            var messageData = messages.Select(
                msg => new NewMessageContract.NewMessageData
            {
                Id = msg.message.MessageId,
                Data = msg.message.Data,
                AttemptNo = msg.attemptNo
            }).ToList();

            var contract = new NewMessageContract
            {
                TopicId = topicQueue.Topic.TopicId,
                QueueId = topicQueue.QueueId,
                ConfirmationId = confirmationId,
                Data = messageData,
            };

            Session.SubscribePacketsInternal++;
            SendDataToSocket(contract);
        }

        private string _subscriberId;
        public string SubscriberId
        {
            get
            {
                if (_subscriberId != null)
                    return _subscriberId;

                _subscriberId = Id.ToString();

                return _subscriberId;
            }
        }

        public bool Disconnected => !Connected;
    }
}