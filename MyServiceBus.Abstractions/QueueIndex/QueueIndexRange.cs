using System;
using System.Collections.Generic;

namespace MyServiceBus.Abstractions.QueueIndex
{
    
    public interface IQueueIndexRange 
    {
        long FromId { get; }
        long ToId { get; }
    }
    
    public class QueueIndexRange : IQueueIndexRange
    {

        public QueueIndexRange(long fromId, long toId)
        {
            FromId = fromId;
            ToId = toId;
        }
        public QueueIndexRange(IQueueIndexRange src)
        {
            FromId = src.FromId;
            ToId = src.ToId;
        }
        
        public QueueIndexRange(long startMessageId)
        {
            FromId = startMessageId;
            ToId = FromId - 1;
        }
        
        
        public long FromId { get; set; }
        public long ToId { get; set; }

        public long GetNextMessage()
        {
            var result = FromId;
            FromId++;
            return result;
        }

        public void AddNextMessage(long id)
        {

            if (ToId == -1 || ToId < FromId)
            {
                FromId = id;
                ToId = id;
                return;
            }

            if (ToId + 1 == id)
                ToId = id;
            else if (FromId - 1 == id)
                FromId = id;
            else
                throw new Exception("Something went wrong. Invalid interval is choosen");
        }

        public bool IsMyInterval(long id)
        {
            return id >= FromId -1  && id <= ToId + 1;
        }
        
        public bool IsEmpty()
        {
            return ToId < FromId;
        }

  

        public static QueueIndexRange Create(long fromId, long toId)
        {
            return new QueueIndexRange(fromId, toId);
        }

        public bool IsBefore(long messageId)
        {
            return messageId < FromId - 1;
        }

        public override string ToString()
        {
            if (IsEmpty())
                return "EMPTY";

            return FromId + " - " + ToId;
        }

        public long Count => ToId - FromId + 1;

        public IEnumerable<long> GetElements()
        {
            for (var i = FromId; i <= ToId; i++)
                yield return i;
        }
    }
}