namespace MyServiceBus.Domains
{
    public interface IMyServiceBusSettings
    {
        
        int MaxDeliveryPackageSize { get; }
        
        int MaxPersistencePackage { get; }

    }
}