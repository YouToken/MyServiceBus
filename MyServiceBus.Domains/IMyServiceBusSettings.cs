using System;

namespace MyServiceBus.Domains
{
    public interface IMyServiceBusSettings
    {
        TimeSpan EventuallyPersistenceDelay { get; }
        
        int MaxDeliveryPackageSize { get; }

    }
}