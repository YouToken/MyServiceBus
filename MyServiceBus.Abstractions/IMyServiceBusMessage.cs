using System;

namespace MyServiceBus.Abstractions
{
    public interface IMyServiceBusMessage
    {
        long Id { get; }
        int AttemptNo { get; }
        ReadOnlyMemory<byte> Data { get; } 
    }
}