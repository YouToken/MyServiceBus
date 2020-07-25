using System;
using MyServiceBus.Domains;
using MyYamlSettingsParser;

namespace MyServiceBus.Server
{
    public class SettingsModel : IMyServiceBusSettings
    {
        public SettingsModel()
        {
            _eventuallyPersistentDelay = new Lazy<TimeSpan>(() => TimeSpan.Parse(EventuallyPersistenceDelay));
        }

        [YamlProperty]
        public string QueuesConnectionString { get; set; }
        
        [YamlProperty]
        public string MessagesConnectionString { get; set; }

        TimeSpan IMyServiceBusSettings.EventuallyPersistenceDelay => _eventuallyPersistentDelay.Value;
        private readonly Lazy<TimeSpan> _eventuallyPersistentDelay;
            
        [YamlProperty]
        public string EventuallyPersistenceDelay { get; set; }
    }

}