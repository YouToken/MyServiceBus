using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MyServiceBus.Server
{
    public static class ServiceStopper
    {

        public static void StopTcpServer()
        {
            
            var connections = ServiceLocator.TcpServer.Count;
            
            Console.WriteLine("Stopping TCP server. Connections are: "+connections);
            
            ServiceLocator.TcpServer.Stop();
            var sw = new Stopwatch();
            sw.Start();

    
            while (connections > 0)
            {
                Thread.Sleep(500);
                connections = ServiceLocator.TcpServer.Count;
                Console.WriteLine("Tcp connections are: "+connections);
            }
            sw.Stop();
            Console.WriteLine("TCP server is stopped in: " + sw.Elapsed);
        }

        public static void WaitingSessionsAreZero()
        {
            var sessionsCount = ServiceLocator.SessionsList.Count;
            
            Console.WriteLine("Waiting sessions got 0. Now sessions are: "+sessionsCount);
            var sw = new Stopwatch();
            sw.Start();

            while (sessionsCount>0)
            {
  
                Thread.Sleep(500);  
                sessionsCount = ServiceLocator.SessionsList.Count;
                Console.WriteLine("Sessions are: "+sessionsCount);
            }
            
            sw.Stop();
            Console.WriteLine("Sessions are zero in: " + sw.Elapsed);
        }


        public static async Task PersistMessagesContentAsync()
        {
            var messagesToPersistCount = ServiceLocator.MessagesToPersistQueue.Count;
            
            Console.WriteLine("Persist all the messages in Queue. Now Messages are: "+messagesToPersistCount);
            
            var sw = new Stopwatch();
            sw.Start();

            while (messagesToPersistCount>0)
            {
                try
                {
                    await ServiceLocator.MyServiceBusBackgroundExecutor.PersistMessageContentAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to persist message content in queue");
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(500);  
                messagesToPersistCount = ServiceLocator.MessagesToPersistQueue.Count;
                Console.WriteLine("Messages in queue: "+messagesToPersistCount);
            }
            
            sw.Stop();
            Console.WriteLine("Sessions are zero in: " + sw.Elapsed);
            
        }

        public static async Task PersistQueueSnapshotAsync()
        {
            Console.WriteLine("Persisting queue Snapshot before close ");
            var sw = new Stopwatch();
            sw.Start();
            
            while (true)
            {
                try
                {
                    await ServiceLocator.MyServiceBusBackgroundExecutor.PersistTopicsAndQueuesSnapshotAsync();
                    sw.Stop();
                    Console.WriteLine("Last queue snapshot is persisted in " + sw.Elapsed);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can not persist queue snapshot before close. Waiting 500ms");
                    Console.WriteLine(e.Message);
                    await Task.Delay(500);
                }
            }
        }

    }
}