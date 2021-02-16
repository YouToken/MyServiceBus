using System;
using System.Linq;
using System.Reflection;
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
using MyServiceBus.Server.Services;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.Server
{
    public static class ServiceLocator
    {
        static ServiceLocator()
        {
            StartedAt = DateTime.UtcNow;

            var name = Assembly.GetEntryAssembly()?.GetName();

            string appName = name?.Name ?? string.Empty;

            var nameSegments = appName.Split('.', StringSplitOptions.RemoveEmptyEntries);

            if (nameSegments.Length > 2)
            {
                appName = string.Join('.', nameSegments.Skip(1));
            }

            AppName = appName;
            AppVersion = name?.Version?.ToString();

            AspNetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Host = Environment.GetEnvironmentVariable("HOSTNAME");

            Console.WriteLine($"AppName: {AppName}");
            Console.WriteLine($"AppVersion: {AppVersion}");
            Console.WriteLine($"AspNetEnvironment: {AspNetEnvironment}");
            Console.WriteLine($"Host: {Host}");
            Console.WriteLine($"StartedAt: {StartedAt}");
            Console.WriteLine($"Port (http1 and http2): 6123");
            Console.WriteLine($"Port (http2): 6124");
            Console.WriteLine();
        }

        public static string AppName { get; private set; }
        public static string AppVersion { get; private set; }

        public static DateTime StartedAt { get; private set; }

        public static string AspNetEnvironment { get; private set; }
        public static string Host { get; private set; }
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
        
        public static readonly MessagesPerSecondByTopic MessagesPerSecondByTopic = new ();

        public static PrometheusMetrics PrometheusMetrics { get; private set; }
        


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

            PrometheusMetrics = serviceResolver.GetService<PrometheusMetrics>();

            DataInitializer.InitAsync(serviceResolver).Wait();
        }
        
        
        private static readonly MyTaskTimer TimerGarbageCollector = new MyTaskTimer(1000);
        
        private static readonly MyTaskTimer TimerPersistent = new MyTaskTimer(1000);

        private static readonly MyTaskTimer TimerStatistic = new MyTaskTimer(1000);


        public static void Start()
        {

            TimerGarbageCollector.Register("Long pooling subscribers GarbageCollect",
                _myServiceBusBackgroundExecutor.ExecuteAsync);
            
            TimerPersistent.Register("Long pooling subscribers Persist",
                _myServiceBusBackgroundExecutor.PersistAsync);

            TimerStatistic.Register("Topics timer", () =>
            {
                foreach (var myTopic in TopicsList.Get())
                    MessagesPerSecondByTopic.PutData(myTopic.TopicId, myTopic.MessagesPerSecond);

                TopicsList.Timer();
                SessionsList.Timer(DateTime.UtcNow);
                return new ValueTask();
            });

            TimerGarbageCollector.Start();
            TimerPersistent.Start();
            TimerStatistic.Start();

        }


        public  static void Stop()
        {
            
            MyGlobalVariables.ShuttingDown = true;

            Console.WriteLine("Stopping background timers");

            TimerPersistent.Stop();
            TimerGarbageCollector.Stop();
            TimerStatistic.Stop();

            Console.WriteLine("Waiting for produce requests are being finished");

            while (MyGlobalVariables.PublishRequestsAmountAreBeingProcessed > 0)
                Thread.Sleep(500);

        }
        
    }
}