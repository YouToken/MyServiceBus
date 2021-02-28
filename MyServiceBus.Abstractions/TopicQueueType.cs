namespace MyServiceBus.Abstractions
{
    public enum TopicQueueType
    {
        Permanent,
        DeleteOnDisconnect,
        PermanentWithSingleConnection
    }
}