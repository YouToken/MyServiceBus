using System;
using System.Collections.Generic;
using System.IO;

namespace MyServiceBus.Persistence.AzureStorage.TopicMessages
{
    public class BinaryDataReader : ISequenceReader
    {

        private readonly List<MemoryStream> _items = new List<MemoryStream>();

        private int _currentChunkId;
        
        public long Position { get; private set; }

        public void Write(MemoryStream src)
        {
            src.Position = 0;
            _items.Add(src);
            Length += src.Length;
        }

        public long Length { get; private set; }
        public long RemainsToRead => Length - Position;

        private void IncrementPosition()
        {
            Position++;
        }

        private void CheckTheEndOfArray()
        {
            
            var memStream = _items[_currentChunkId];
            if (memStream.Position >= memStream.Length)
            {
                _currentChunkId++;
            } 
        }

        public bool Eof => Position>=Length;


        private void GarbageCollect()
        {
            while (_currentChunkId>0 && _currentChunkId>0)
            {
                var firstItem = _items[0];
                _items.RemoveAt(0);

                Length -= firstItem.Length;
                Position -= firstItem.Length;
                _currentChunkId--;

            }
        }

        public byte ReadByte()
        {

            if (Eof)
                throw new IndexOutOfRangeException($"We are beyond the range of {Length} bytes");

            var memStream = _items[_currentChunkId];

            var result = (byte)memStream.ReadByte();
            IncrementPosition();

            GarbageCollect();

            return result;
        }

        public MemoryStream ReadArray(int length)
        {
            if (Eof)
                throw new IndexOutOfRangeException($"We are beyond the range of {Length} bytes");
            
            
            var result = new MemoryStream();

            var remainsLength = length;

            while (remainsLength>0)
            {
            
                var memStream = _items[_currentChunkId];
                    

                var remainsToRead = memStream.Length - memStream.Position;

                var copySize = remainsLength > remainsToRead ? (int)remainsToRead : remainsLength;
                
                result.WriteFromStream(memStream, copySize);
                
                Position += copySize;
                remainsLength -= copySize;

                CheckTheEndOfArray();
            }
            
            GarbageCollect();
            
            return result;
        }

    }
}