using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyTcpSockets.Extensions;

namespace MyServiceBus.TcpContracts.Tests;

public class IncomingTcpTrafficMock : IIncomingTcpTrafficReader
{

    public readonly Queue<byte> _incomingTraffic = new Queue<byte>();


    public void NewPackageAsync(ReadOnlyMemory<byte> incoming)
    {
        var span = incoming.Span;
        foreach (var b in span)
        {
            _incomingTraffic.Enqueue(b);
        }

    }

    public async ValueTask<byte> ReadByteAsync(CancellationToken token)
    {
        while (true)
        {
            if (_incomingTraffic.Count > 0)
                return _incomingTraffic.Dequeue();

            await Task.Delay(100, token);
        }



    }
    
    public ValueTask<int> ReadBytesAsync(Memory<byte> buffer, CancellationToken token)
    {

        var pos = 0;

        var span = buffer.Span;
        
        while (pos<buffer.Length)
        {

            if (_incomingTraffic.Count == 0)
                return new ValueTask<int>(pos);
            
            var b= _incomingTraffic.Dequeue();
            span[pos] = b;
            pos++;
        }

        return new ValueTask<int>(pos);

    }

}