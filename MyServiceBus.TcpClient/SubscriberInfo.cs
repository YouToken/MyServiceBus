using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Abstractions;
using MyServiceBus.Abstractions.QueueIndex;
using MyServiceBus.TcpContracts;

namespace MyServiceBus.TcpClient
{

    
    public class SubscriberInfo
    {
        private readonly IMyServiceBusLogInvoker _log;

        public SubscriberInfo(IMyServiceBusLogInvoker log, string topicId, string queueId, TopicQueueType queueType, 
            Func<IMyServiceBusMessage, ValueTask> callbackAsOneMessage, Func<IConfirmationContext, IReadOnlyList<IMyServiceBusMessage>, ValueTask> callbackAsAPackage)
        {
            _log = log;
            TopicId = topicId;
            QueueId = queueId;
            QueueType = queueType;
            CallbackAsOneMessage = callbackAsOneMessage;
            CallbackAsAPackage = callbackAsAPackage;
        }

        public string TopicId { get; }
        public string QueueId { get; }
        public TopicQueueType QueueType { get; }
        public Func<IMyServiceBusMessage, ValueTask> CallbackAsOneMessage { get; }
        public Func<IConfirmationContext, IReadOnlyList<IMyServiceBusMessage>, ValueTask> CallbackAsAPackage { get; }
        

        private async Task InvokeOneByOne(IReadOnlyList<NewMessagesContract.NewMessageData> messages,
            Action confirmAllOk, Action confirmAllReject, Action<QueueWithIntervals> confirmSomeMessagesAreOk)
        {
            
            if (CallbackAsOneMessage == null)
                return;
            
            QueueWithIntervals result = null;

            try
            {
                foreach (var message in messages)
                {
                    await CallbackAsOneMessage(message);

                    result ??= new QueueWithIntervals();
                    result.Enqueue(message.Id);
                }
            }
            catch (Exception e)
            {
                _log.InvokeLogException(e);
                if (result == null)
                    confirmAllReject();
                else
                    confirmSomeMessagesAreOk(result);
                return;
            }

            confirmAllOk();
        }

        private async Task InvokeBulkCallback(IConfirmationContext confirmationContext, IReadOnlyList<NewMessagesContract.NewMessageData> messages, 
            Action confirmAllOk, Action confirmAllReject)
        {
            if (CallbackAsAPackage == null)
                return;

            try
            {
                await CallbackAsAPackage(confirmationContext, messages);
                confirmAllOk();
            }
            catch (Exception e)
            {                
                _log.InvokeLogException(e);
                confirmAllReject();
            }

        }

        public void InvokeNewMessages(IConfirmationContext confirmationContext, IReadOnlyList<NewMessagesContract.NewMessageData> messages,
            Action confirmAllOk, Action confirmAllReject, 
            Action<QueueWithIntervals> confirmSomeMessagesAreOk)
        {
            #pragma warning disable
            InvokeOneByOne(messages, confirmAllOk, confirmAllReject, confirmSomeMessagesAreOk);
            InvokeBulkCallback(confirmationContext, messages,confirmAllOk, confirmAllReject);
            #pragma warning restore
        }

    }
}