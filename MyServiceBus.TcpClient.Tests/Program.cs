using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MyServiceBus.TcpClient;

namespace MyServiceBus.TcpClientTest
{
    class Program
    {
        static void Main(string[] args)
        {


            var hostPort = "127.0.0.1:6421";

            var myServiceBusTcpClient = new MyServiceBusTcpClient(()=>hostPort, "test");

       //     var myServiceBusTcpClien2 = new MyServiceBusTcpClient(()=>hostPort, "test2");

            myServiceBusTcpClient.CreateTopicIfNotExists("test-topic", 100000);

            var i = 0;

            myServiceBusTcpClient.Subscribe("test-topic", "myqueue", false, data =>
            {
                Console.WriteLine("test-topic Length: " + data.Id);

                i++;
                
                if (i<10)
                  throw new Exception("No data");

                i = 0;
                return new ValueTask();


            });
            /*
             myServiceBusTcpClien2.Subscribe("test-topic", "myqueue", true, data =>
             {
                 //  Console.WriteLine("test-topic Length: " + data.Length);
                 return new ValueTask();
 
             });
 */
            /*
                        myServiceBusTcpClient.Subscribe("trading-account", "trading-account-test", true, data =>
                        {
                            Console.WriteLine("trading-accounts Length: " + data.Length);
                            return new ValueTask();

                        });

                        myServiceBusTcpClient.Subscribe("active-positions", "trading-account-test", true, data =>
                        {
                            Console.WriteLine("active-positions Length: " + data.Length);
                            return new ValueTask();

                        });

                        myServiceBusTcpClient.Subscribe("pending-orders", "pending-orders-test", true, data =>
                        {
                            Console.WriteLine("pending-orders Length: " + data.Length);
                            return new ValueTask();
                        });


                        myServiceBusTcpClient.Subscribe("update-active-position", "pending-orders-test", true, data =>
                        {
                            Console.WriteLine("update-active-position Length: " + data.Length);
                            return new ValueTask();
                        });

                        myServiceBusTcpClient.Subscribe("update-pending-order", "pending-orders-test", true, data =>
                        {
                            Console.WriteLine("update-pending-order Length: " + data.Length);
                            return new ValueTask();
                        });
                        */



            myServiceBusTcpClient.Start();
            //         myServiceBusTcpClien2.Start();

            var messages = new List<byte[]>();
    
            messages.Add(new byte[] { 0 });
  

            var asArray = messages.ToArray();

            Task.Delay(2000).Wait();
  
            myServiceBusTcpClient.PublishFireAndForget("test-topic", asArray);

            while (true)
            {
                //Console.WriteLine("Publishing");
                //myServiceBusTcpClient.PublishFireAndForget("test-topic", asArray);
                Task.Delay(1000).Wait();
            }

            Console.ReadLine();

            Console.WriteLine("Hello World!");
        }
    }
}