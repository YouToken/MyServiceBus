using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyServiceBus.Domains;
using MyServiceBus.Domains.Execution;
using MyServiceBus.Domains.Metrics;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Domains.Sessions;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Server.Hubs;
using MyServiceBus.Server.Services;
using MyServiceBus.Server.Services.Sessions;
using MyServiceBus.TcpContracts;
using MyTcpSockets;

namespace MyServiceBus.Server
{
    public static class ServiceLocator
    {

        public static int TcpConnectionsSnapshotId { get; set; }
        
        
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

        public static string AppName { get; }
        public static string AppVersion { get;}

        public static DateTime StartedAt { get; }

        public static string AspNetEnvironment { get;  }
        public static string Host { get; }
        public static SessionsList SessionsList { get; private set; }
        public static TopicsManagement TopicsManagement { get; private set; }
        public static TopicsList TopicsList { get; private set; }
        public static GlobalVariables MyGlobalVariables { get; private set; }
        public static MyServiceBusPublisher MyServiceBusPublisher { get; private set; }
        public static MyServiceBusSubscriber Subscriber { get; private set; }

        private static MyServiceBusBackgroundExecutor _myServiceBusBackgroundExecutor;
        public static MyServerTcpSocket<IServiceBusTcpContract> TcpServer { get; internal set; }
        public static IMessagesToPersistQueue MessagesToPersistQueue { get; private set; }
        public static MessagesPerSecondByTopic MessagesPerSecondByTopic { get; private set; }
        public static PrometheusMetrics PrometheusMetrics { get; private set; }
        
        public static GrpcSessionsList GrpcSessionsList { get; private set; }

        public static void Init(IServiceProvider serviceProvider)
        {

            GrpcSessionsList = serviceProvider.GetRequiredService<GrpcSessionsList>();
            
            MessagesPerSecondByTopic = serviceProvider.GetRequiredService<MessagesPerSecondByTopic>();
            
            TopicsManagement = serviceProvider.GetRequiredService<TopicsManagement>();
            TopicsList = serviceProvider.GetRequiredService<TopicsList>();
            
            MyGlobalVariables = serviceProvider.GetRequiredService<GlobalVariables>();

            MyServiceBusPublisher = serviceProvider.GetRequiredService<MyServiceBusPublisher>();
            Subscriber = serviceProvider.GetRequiredService<MyServiceBusSubscriber>();
            
            SessionsList = serviceProvider.GetRequiredService<SessionsList>();

            MessagesToPersistQueue = serviceProvider.GetRequiredService<IMessagesToPersistQueue>();
            
            _myServiceBusBackgroundExecutor = serviceProvider.GetRequiredService<MyServiceBusBackgroundExecutor>();

            PrometheusMetrics = serviceProvider.GetRequiredService<PrometheusMetrics>();

            DataInitializer.InitAsync(serviceProvider).Wait();
        }
        
        
        private static readonly MyTaskTimer TimerGarbageCollector = new (1000);
        
        private static readonly MyTaskTimer TimerPersistent = new (1000);

        private static readonly MyTaskTimer TimerStatistic = new (1000);
        
        
        


        public static void Start()
        {

            TimerGarbageCollector.Register("Persist Topics And Queues",
                _myServiceBusBackgroundExecutor.PersistTopicsAndQueuesAsync);
            
            TimerGarbageCollector.Register("Persist Messages",
                _myServiceBusBackgroundExecutor.PersistMessages);
            
            TimerPersistent.Register("Sessions List GC",
                ()=>
                {
                    GrpcSessionsList.GarbageCollectSessions(DateTime.UtcNow);
                    return new ValueTask();
                });

            TimerStatistic.Register("Metrics timer", () =>
            {
                TopicsList.KickMetricsTimer();
                
                foreach (var myTopic in TopicsList.Get())
                    MessagesPerSecondByTopic.PutData(myTopic.TopicId, myTopic.MessagesPerSecond);

                return new ValueTask(MonitoringHub.BroadCasMetricsAsync());
            });

            TimerGarbageCollector.Start();
            TimerPersistent.Start();
            TimerStatistic.Start();
        }


        public  static void Stop()
        {
            
            MyGlobalVariables.ShuttingDown = true;
            
            Console.WriteLine("Stopping TCP server");
            var sw = new Stopwatch();
            sw.Start();
            TcpServer.Stop();
            while (TcpServer.Count > 0)
                Thread.Sleep(100);
            sw.Stop();
            Console.WriteLine("TCP server is stopped in: " + sw.Elapsed);
            
            NotCompiled

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