using System.IO;
using System.Linq;
using MyServiceBus.TcpContracts;
using MyTcpSockets.Extensions;
using NUnit.Framework;

namespace MyServiceBus.TcpContracts.Tests
{
    public class TestTcpContracts
    {

        private static void TestValues(object t1, object t2)
        {
            var ps1 = t1.GetType().GetProperties().ToDictionary(itm => itm.Name);
            var ps2 = t2.GetType().GetProperties().ToDictionary(itm => itm.Name);

            foreach (var pi1 in ps1)
            {
                var v1 = pi1.Value.GetValue(t1);
                var v2 = ps2[pi1.Key].GetValue(t2);

                Assert.AreEqual(v1, v2);
            }
        }

        [Test]
        public void TestPing()
        {

            var serializer = new MyServiceBusTcpSerializer();


            var pingContract = new PingContract();

            var rawData = serializer.Serialize(pingContract);

            var memStream = new MemoryStream(rawData.ToArray())
            {
                Position = 0
            };

            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());


            var result
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            Assert.IsTrue(typeof(PingContract) == result.GetType());
        }

        
        [Test]
        public void TestPong()
        {

            var serializer = new MyServiceBusTcpSerializer();


            var inContract = new PongContract();

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};

            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());
            
            var result
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            Assert.IsTrue(inContract.GetType() == result.GetType());   
        }

        [Test]
        public void TestGreeting()
        {

            var serializer = new MyServiceBusTcpSerializer();

            var inContract = new GreetingContract
            {
                Name = "MyName"
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};

            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());
            
            var result
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            Assert.IsTrue(inContract.GetType() == result.GetType());
            TestValues(inContract, result);
        }

        [Test]
        public void TestPublishContract()
        {
            var serializer = new MyServiceBusTcpSerializer();

            var inContract = new PublishContract
            {
                TopicId = "MyName",
                RequestId = 5,
                Data = new [] {new byte[] {1, 2, 3}, new byte[] {4, 5, 6}, new byte[] {7, 8, 9}},
                ImmediatePersist = 1
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray())
            {
                Position = 0
            };
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());

            var res 
                = serializer.DeserializeAsync(dataReader).AsTestResult();
           
           
           var result = (PublishContract) res;
           Assert.AreEqual(inContract.TopicId, result.TopicId);
           Assert.AreEqual(inContract.RequestId, result.RequestId);
           Assert.AreEqual(inContract.ImmediatePersist, result.ImmediatePersist);
           var d1 = inContract.Data.ToArray();
           var d2 = result.Data.ToArray();

           Assert.AreEqual(d1.Length, d2.Length);
           Assert.AreEqual(d1[0].ToArray()[0], d2[0].ToArray()[0]);
           Assert.AreEqual(d1[1].ToArray()[0], d2[1].ToArray()[0]);
           Assert.AreEqual(d1[2].ToArray()[0], d2[2].ToArray()[0]); 



 
        }

        [Test]
        public void TestPublishResponseContract()
        {

            var serializer = new MyServiceBusTcpSerializer();

            var inContract = new PublishResponseContract
            {
                RequestId = 66
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};

            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());
            
            var result
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            Assert.IsTrue(inContract.GetType() == result.GetType());
            TestValues(inContract, result);

        }

        [Test]
        public void TestSubscribeContract()
        {

            var serializer = new MyServiceBusTcpSerializer();


            var inContract = new SubscribeContract
            {
                TopicId = "aaa",
                QueueId = "bbb",
                DeleteOnDisconnect = true
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());

            var result 
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            Assert.IsTrue(inContract.GetType() == result.GetType());
            TestValues(inContract, result);
        }

        [Test]
        public void TestSubscribeResponseContract()
        {

            var serializer = new MyServiceBusTcpSerializer();


            var inContract = new SubscribeResponseContract
            {
                TopicId = "aaa",
                QueueId = "bbb"

            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());

            var result 
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            Assert.IsTrue(inContract.GetType() == result.GetType());
            TestValues(inContract, result);

        }


        [Test]
        public void TestNewMessageContract()
        {

            var serializer = new MyServiceBusTcpSerializer();


            var inContract = new NewMessageContract
            {
                TopicId = "aaa",
                QueueId = "bbb",
                Data = new[]
                {
                    new NewMessageContract.NewMessageData
                    {
                        Id = 5,
                        Data = new byte[] {1, 2, 3}
                    },
                    new NewMessageContract.NewMessageData
                    {
                        Id = 6,
                        Data = new byte[] {4, 5, 6}
                    },

                }
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};

            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());

            var res = serializer
                .DeserializeAsync(dataReader)
                .AsTestResult();
            
            var result = (NewMessageContract) res;
            Assert.AreEqual(inContract.TopicId, result.TopicId);
            Assert.AreEqual(inContract.QueueId, result.QueueId);
            var d1 = inContract.Data.ToArray();
            var d2 = result.Data.ToArray();

            Assert.AreEqual(d1.Length, d2.Length);
            Assert.AreEqual(d1[0].Id, d2[0].Id);
            Assert.AreEqual(d1[1].Id, d2[1].Id);

            Assert.AreEqual(d1[0].Data.ToArray()[0], d2[0].Data.ToArray()[0]);
            Assert.AreEqual(d1[1].Data.ToArray()[0], d2[1].Data.ToArray()[0]);

        }

        [Test]
        public void TestNewMessageConfirmationContract()
        {

            var serializer = new MyServiceBusTcpSerializer();


            var inContract = new NewMessageConfirmationContract
            {
                TopicId = "234",
                QueueId = "ggg",
                ConfirmationId = 555
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());            

            var res
                = serializer.DeserializeAsync(dataReader)
                    .AsTestResult();

             var result = (NewMessageConfirmationContract) res;
             Assert.AreEqual(inContract.TopicId, result.TopicId);
             Assert.AreEqual(inContract.QueueId, result.QueueId);
             Assert.AreEqual(inContract.ConfirmationId, result.ConfirmationId);

        }

        [Test]
        public void TestCreateTopicIfNotExists()
        {

            var serializer = new MyServiceBusTcpSerializer();


            var inContract = new CreateTopicIfNotExistsContract
            {
                TopicId = "234",
                MaxMessagesInCache = 243432
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());              

            var res
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            var result = (CreateTopicIfNotExistsContract) res;
            Assert.AreEqual(inContract.TopicId, result.TopicId);
            Assert.AreEqual(inContract.MaxMessagesInCache, result.MaxMessagesInCache);
        }

        [Test]
        public void SendConfirmationsPacket()
        {
            var serializer = new MyServiceBusTcpSerializer();


            var inContract = new MessagesConfirmationContract
            {
                TopicId = "234",
                QueueId = "567",
                Ok = new []{new MessagesInterval{FromId = 5, ToId = 7}},
                NotOk = new []{new MessagesInterval{FromId = 10, ToId = 10}, new MessagesInterval{FromId = 15, ToId = 16} },
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());             

            var res
                = serializer
                    .DeserializeAsync(dataReader)
                    .AsTestResult();

            var result = (MessagesConfirmationContract) res;
            Assert.AreEqual(inContract.TopicId, result.TopicId);
            Assert.AreEqual(inContract.QueueId, result.QueueId); 
            Assert.AreEqual(1, result.Ok.Count);
            Assert.AreEqual(2, result.NotOk.Count);
            Assert.AreEqual(10, result.NotOk[0].FromId);
            Assert.AreEqual(10, result.NotOk[0].ToId);
            
            Assert.AreEqual(15, result.NotOk[1].FromId);
            Assert.AreEqual(16, result.NotOk[1].ToId);
            
        }
        
        
        [Test]
        public void TestPacketsVersionsContract()
        {
            var serializer = new MyServiceBusTcpSerializer();
            var deserializer = new MyServiceBusTcpSerializer();

            var inContract = new PacketVersionsContract();
            inContract.SetPacketVersion(CommandType.Publish, 1);
            inContract.SetPacketVersion(CommandType.NewMessage, 2);

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());            

            var res
                = deserializer.DeserializeAsync(dataReader)
                    .AsTestResult();

            var result = (PacketVersionsContract) res;
            var inPackets = inContract.GetPackets().ToDictionary(itm => itm.Key);
            var resultPackets = result.GetPackets().ToDictionary(itm => itm.Key);
            
            Assert.AreEqual(inPackets.Count, resultPackets.Count);

            foreach (var kvp in inPackets)
            {
                Assert.AreEqual(inPackets[kvp.Key], resultPackets[kvp.Key]);
            }

        }        
        
        
        [Test]
        public void TestRejectConnectionContract()
        {
            var serializer = new MyServiceBusTcpSerializer();

            var inContract = new RejectConnectionContract
            {
                Message = "MyMessage"
            };

            var rawData = serializer.Serialize(inContract);

            var memStream = new MemoryStream(rawData.ToArray()) {Position = 0};
            
            var dataReader = new TcpDataReader();
            dataReader.NewPackage(memStream.ToArray());            

            var res
                = serializer.DeserializeAsync(dataReader)
                    .AsTestResult();

            var result = (RejectConnectionContract) res;
            Assert.AreEqual(inContract.Message, result.Message);


        }          
        
    }
}