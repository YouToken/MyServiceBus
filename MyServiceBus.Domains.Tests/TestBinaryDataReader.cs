using MyServiceBus.Persistence.AzureStorage;
using MyServiceBus.Persistence.AzureStorage.TopicMessages;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class TestBinaryDataReader
    {
        [Test]
        public void TestBaseBinaryDataReader()
        {

            var array1 = new byte[] {1, 2, 3, 4, 5};
            
            var binaryDataReader = new BinaryDataReader();
            
            binaryDataReader.Write(array1.ToMemoryStream());
            
            Assert.AreEqual(5, binaryDataReader.Length);
            
            Assert.AreEqual(0, binaryDataReader.Position);
            Assert.AreEqual(5, binaryDataReader.RemainsToRead);
            
            
            var byteResult = binaryDataReader.ReadByte();
            
            Assert.AreEqual(1, byteResult);
            
            Assert.AreEqual(5, binaryDataReader.Length);
            Assert.AreEqual(1, binaryDataReader.Position);
            Assert.AreEqual(4, binaryDataReader.RemainsToRead);
            
            
            binaryDataReader.Write(new byte[] {11, 12, 13, 14, 15, 16}.ToMemoryStream());
            
            
            Assert.AreEqual(1, byteResult);
            
            Assert.AreEqual(11, binaryDataReader.Length);
            Assert.AreEqual(1, binaryDataReader.Position);
            Assert.AreEqual(10, binaryDataReader.RemainsToRead);


            var resultArray = binaryDataReader.ReadArray(5).ToArray();
            
            Assert.AreEqual(2, resultArray[0]);
            Assert.AreEqual(3, resultArray[1]);
            Assert.AreEqual(4, resultArray[2]);
            Assert.AreEqual(5, resultArray[3]);
            Assert.AreEqual(11, resultArray[4]);
            
            Assert.AreEqual(6, binaryDataReader.Length);
            Assert.AreEqual(1, binaryDataReader.Position);
            Assert.AreEqual(5, binaryDataReader.RemainsToRead);
            
            byteResult = binaryDataReader.ReadByte();
            Assert.AreEqual(12, byteResult);

            byteResult = binaryDataReader.ReadByte();
            Assert.AreEqual(13, byteResult);

            byteResult = binaryDataReader.ReadByte();
            Assert.AreEqual(14, byteResult);

            byteResult = binaryDataReader.ReadByte();
            Assert.AreEqual(15, byteResult);

            byteResult = binaryDataReader.ReadByte();
            Assert.AreEqual(16, byteResult);
            
            Assert.IsTrue(binaryDataReader.Eof);
        }
    }
}