using System;

namespace MyServiceBus.TcpClient;

public enum PublishFailReason
{
    NoActiveConnection, Disconnected
}

public class PublishFailException : Exception
{
    public PublishFailException(PublishFailReason reason, string msg) : base(msg)
    {
        Reason = reason;
    }
    
    public PublishFailReason Reason { get; }
}


