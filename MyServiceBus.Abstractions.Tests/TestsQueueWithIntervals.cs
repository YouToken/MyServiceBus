using System;
using System.Linq;
using MyServiceBus.Abstractions.QueueIndex;
using NUnit.Framework;

namespace MyServiceBus.Abstractions.Tests
{
    public class TestsQueueWithIntervals
    {

        [Test]
        public void TestInsertingAndRemoving()
        {
            var queueWithIndex = new QueueWithIntervals(0);

            queueWithIndex.Enqueue(0);
            queueWithIndex.Enqueue(1);
            queueWithIndex.Enqueue(2);

            queueWithIndex.Enqueue(7);
            queueWithIndex.Enqueue(8);

            var result = queueWithIndex.GetElements().ToList();

            Assert.AreEqual(5, result.Count);

            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(1, result[1]);
            Assert.AreEqual(2, result[2]);
            Assert.AreEqual(7, result[3]);
            Assert.AreEqual(8, result[4]);
        }


        [Test]
        public void TestIntervals()
        {
            var queueWithIndex = new QueueWithIntervals(0);

            queueWithIndex.Enqueue(0);
            queueWithIndex.Enqueue(1);
            queueWithIndex.Enqueue(2);

            queueWithIndex.Enqueue(7);
            queueWithIndex.Enqueue(8);

            var result = queueWithIndex.GetSnapshot();

            Assert.AreEqual(2, result.Count);


            Assert.AreEqual(0, result[0].FromId);
            Assert.AreEqual(2, result[0].ToId);

            Assert.AreEqual(7, result[1].FromId);
            Assert.AreEqual(8, result[1].ToId);

        }


        [Test]
        public void TestDequing()
        {
            var queueWithIndex = new QueueWithIntervals(0);

            queueWithIndex.Enqueue(0);
            queueWithIndex.Enqueue(1);
            queueWithIndex.Enqueue(2);

            queueWithIndex.Enqueue(7);
            queueWithIndex.Enqueue(8);


            Assert.AreEqual(0, queueWithIndex.Dequeue());


            Assert.AreEqual(1, queueWithIndex.Dequeue());
            Assert.AreEqual(2, queueWithIndex.Dequeue());

            Assert.AreEqual(7, queueWithIndex.Dequeue());
            Assert.AreEqual(8, queueWithIndex.Dequeue());

            Assert.AreEqual(-1, queueWithIndex.Dequeue());

        }

        [Test]
        public void TestSimpleCase()
        {
            var messagesRange = new QueueIndexRange(-1);

            messagesRange.AddNextMessage(1);

            Assert.AreEqual(1, messagesRange.FromId);
            Assert.AreEqual(1, messagesRange.ToId);

            messagesRange.AddNextMessage(2);

            Assert.AreEqual(1, messagesRange.FromId);
            Assert.AreEqual(2, messagesRange.ToId);

            var message = messagesRange.GetNextMessage();

            Assert.AreEqual(1, message);
            Assert.IsFalse(messagesRange.IsEmpty());

            message = messagesRange.GetNextMessage();

            Assert.AreEqual(2, message);
            Assert.IsTrue(messagesRange.IsEmpty());
        }

        [Test]
        public void TestAddMessages()
        {
            var queue = new QueueWithIntervals(-1);

            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            var messageId = queue.Dequeue();
            Assert.AreEqual(1, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(2, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(3, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(-1, messageId);


        }

        [Test]
        public void TestBrokenSequence()
        {
            var queue = new QueueWithIntervals(-1);

            queue.Enqueue(100);
            queue.Enqueue(99);

            queue.Enqueue(6);
            queue.Enqueue(5);

            queue.Enqueue(2);
            queue.Enqueue(1);

            queue.Enqueue(10);
            queue.Enqueue(11);

            var messageId = queue.Dequeue();
            Assert.AreEqual(1, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(2, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(5, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(6, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(10, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(11, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(99, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(100, messageId);

            messageId = queue.Dequeue();
            Assert.AreEqual(-1, messageId);
        }

    }
}