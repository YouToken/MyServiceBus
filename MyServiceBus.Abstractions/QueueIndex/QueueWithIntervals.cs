using System;
using System.Collections.Generic;
using System.Linq;

namespace MyServiceBus.Abstractions.QueueIndex
{
    public class QueueWithIntervals
    {
        public QueueWithIntervals(long from, long to)
        {
            _messages.Add(new QueueIndexRange(from, to));
        }
        
        public QueueWithIntervals(long messageId)
        {
            _messages.Add(new QueueIndexRange(messageId));
        }

        private readonly List<QueueIndexRange> _messages = new List<QueueIndexRange>();

        private int GetIndexToInsert(long messageId)
        {
            var i = 0;

            foreach (var range in _messages)
            {
                if (range.IsBefore(messageId))
                    return i;

                i++;
            }

            return i;
        }

        private int GetIndexToDelete(long messageId)
        {

            var i = 0;

            foreach (var range in _messages)
            {
                if (range.FromId <= messageId && messageId <= range.ToId)
                    return i;

                i++;
            }

            return -1;
        }

        private QueueIndexRange GetInterval(long messageId)
        {


            if (_messages.Count == 1)
            {
                var firstOne = _messages[0];
                if (firstOne.IsEmpty() || firstOne.IsMyInterval(messageId))
                    return firstOne;
            }

            foreach (var range in _messages)
            {
                if (range.IsMyInterval(messageId))
                    return range;
            }

            var index = GetIndexToInsert(messageId);

            var newItem = new QueueIndexRange(0);

            if (index >= _messages.Count)
                _messages.Add(newItem);
            else
                _messages.Insert(index, newItem);

            return newItem;

        }


        public IEnumerable<long> GetElements()
        {
            return _messages.SelectMany(range => range.GetElements());
        }


        public void Enqueue(long messageId)
        {
            var interval = GetInterval(messageId);
            interval.AddNextMessage(messageId);

        }

        public long Dequeue()
        {
            var interval = _messages[0];
            if (interval.IsEmpty())
                return -1;

            var result = interval.GetNextMessage();

            if (interval.IsEmpty() && _messages.Count > 1)
                _messages.RemoveAt(0);

            return result;

        }

        public long GetMessagesCount()
        {
            long result = 0;
            foreach (var indexRange in _messages)
            {
                result += indexRange.Count();
            }

            return result;
        }

        public override string ToString()
        {
            return $"Intervals: {_messages.Count}. Count: {GetMessagesCount()}";
        }

        public IReadOnlyList<QueueIndexRangeReadOnly> GetSnapshot()
        {
            return _messages.Select(QueueIndexRangeReadOnly.Create).ToList();
        }


        public long GetMinId()
        {
            return _messages[0].FromId;
        }

        public void Remove(in long messageId)
        {
            var index = GetIndexToDelete(messageId);

            if (index < 0)
            {
                Console.WriteLine("No element " + messageId);
                return;
            }

            var range = _messages[index];

            if (range.FromId == messageId)
                range.FromId++;
            else if (range.ToId == messageId)
                range.ToId--;
            else
            {
                var newRange = QueueIndexRange.Create(range.FromId, messageId - 1);
                range.FromId = messageId + 1;
                _messages.Insert(index, newRange);
            }

            if (range.IsEmpty() && _messages.Count > 1)
                _messages.RemoveAt(index);
        }
    }
}