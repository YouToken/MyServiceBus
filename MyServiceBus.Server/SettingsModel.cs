using System;
using MyServiceBus.Domains;
using MyYamlParser;

namespace MyServiceBus.Server
{
    public class SettingsModel : IMyServiceBusSettings
    {
        public SettingsModel()
        {
            _eventuallyPersistentDelay = new Lazy<TimeSpan>(() => TimeSpan.Parse(EventuallyPersistenceDelay));
        }

        [YamlProperty]
        public string GrpcUrl { get; set; }
        
        TimeSpan IMyServiceBusSettings.EventuallyPersistenceDelay => _eventuallyPersistentDelay.Value;
        private readonly Lazy<TimeSpan> _eventuallyPersistentDelay;
            
        [YamlProperty]
        public string EventuallyPersistenceDelay { get; set; }
        
        [YamlProperty(defaultValue: 1024*1024)]
        public int MaxDeliveryPackageSize { get; set; }
    }

}