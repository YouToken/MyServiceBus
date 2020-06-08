using System;
using System.Threading;
using System.Threading.Tasks;
using MyDependencies;
using MyServiceBus.Domains;
using MyServiceBus.Domains.Execution;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Metrics;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.Server
{
    public static class ServiceLocatorApi
    {
        public static SessionsList SessionsList { get; private set; }
        public static TopicsManagement TopicsManagement { get; private set; }
        public static TopicsList TopicsList { get; private set; }
        
        public static GlobalVariables MyGlobalVariables { get; private set; }
        
        public static MyServiceBusPublisher MyServiceBusPublisher { get; private set; }

        public static MyServiceBusSubscriber Subscriber { get; private set; }


        private static MyServiceBusBackgroundExecutor _myServiceBusBackgroundExecutor;
        
        
        public static MyServiceBusDeliveryHandler MyServiceBusDeliveryHandler { get; private set; }
        
        public static MyServerTcpSocket<IServiceBusTcpContract> TcpServer { get; internal set; }
        
        public static IMessagesToPersistQueue MessagesToPersistQueue { get; private set; }
        
        public static MessageContentCacheByTopic CacheByTopic { get; private set; }
        
        public static readonly MessagesPerSecondByTopic MessagesPerSecondByTopic = new MessagesPerSecondByTopic();
        
        private static async Task RestoreTopicsAsync(IServiceResolver serviceResolver)
        {

            var storage = serviceResolver.GetService<ITopicPersistenceStorage>();

            var data = await storage.GetSnapshotAsync();

            TopicsList.Restore(data.topicsData);

        }

        public static void Init(IServiceResolver serviceResolver)
        {
            TopicsManagement = serviceResolver.GetService<TopicsManagement>();
            TopicsList = serviceResolver.GetService<TopicsList>();
            
            MyGlobalVariables = serviceResolver.GetService<GlobalVariables>();

            MyServiceBusPublisher = serviceResolver.GetService<MyServiceBusPublisher>();
            Subscriber = serviceResolver.GetService<MyServiceBusSubscriber>();
            
            SessionsList = serviceResolver.GetService<SessionsList>();

            MessagesToPersistQueue = serviceResolver.GetService<IMessagesToPersistQueue>();


            MyServiceBusDeliveryHandler = serviceResolver.GetService<MyServiceBusDeliveryHandler>();
            
            _myServiceBusBackgroundExecutor = serviceResolver.GetService<MyServiceBusBackgroundExecutor>();

            CacheByTopic = serviceResolver.GetService<MessageContentCacheByTopic>();

            RestoreTopicsAsync(serviceResolver).Wait();
        }
        
        
        private static readonly MyTaskTimer TimerSeconds = new MyTaskTimer(1000);

        private static readonly MyTaskTimer TimerStatistic = new MyTaskTimer(1000);




        public static void Start()
        {

            TimerSeconds.Register("Long pooling subscribers GarbageCollect",
                () => _myServiceBusBackgroundExecutor.ExecuteAsync(DateTime.UtcNow));

            TimerStatistic.Register("Topics timer", () =>
            {
                foreach (var myTopic in TopicsList.Get())
                    MessagesPerSecondByTopic.PutData(myTopic.TopicId, myTopic.MessagesPerSecond);

                TopicsList.Timer();
                SessionsList.Timer();
                return new ValueTask();
            });

            TimerSeconds.Start();
            TimerStatistic.Start();

        }


        public  static void Stop()
        {
            
            MyGlobalVariables.ShuttingDown = true;

            Console.WriteLine("Stopping background timers");

            TimerSeconds.Stop();
            TimerStatistic.Stop();

            Console.WriteLine("Waiting for produce requests are being finished");

            while (MyGlobalVariables.PublishRequestsAmountAreBeingProcessed > 0)
                Thread.Sleep(500);

        }
        
    }
}