using System;
using System.Collections.Generic;

namespace MyServiceBus.TcpClient
{
    public class PublishAndForgetState
    {
        
        private  List<byte[]> _messagesToSend = new List<byte[]>();


        private long _fireAndForgetRequestId = -1;

        public bool HasFireAndForgetRequests()
        {
            return _fireAndForgetRequestId > -1;
        }

        public void Enqueue(IEnumerable<byte[]> data)
        {
            _messagesToSend.AddRange(data);
        }

        public IReadOnlyList<byte[]> GetMessagesToSend()
        {
            if (_messagesToSend.Count == 0)
                return Array.Empty<byte[]>();
            
            var result = _messagesToSend;
            _messagesToSend = new List<byte[]>();
            return result;
        }


        public void Confirmed(in long requestId)
        {
            if (_fireAndForgetRequestId != requestId)
                Console.WriteLine($"Somehow we are here. _fireAndForgetRequestId={_fireAndForgetRequestId}. RequestId = {requestId}");

            _fireAndForgetRequestId = -1;

        }

        public void SetOnRequest(in long requestId)
        {
            _fireAndForgetRequestId = requestId;
        }
    }
}