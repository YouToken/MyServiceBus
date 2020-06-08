namespace MyServiceBus.Domains.Execution
{
    public enum ExecutionResult
    {
        Ok, TopicNotFound, RequestExpired, QueueIsNotFound
    }
}