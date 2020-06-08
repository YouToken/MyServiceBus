using System;
using System.IO;
using DotNetCoreDecorators;
using MyServiceBus.Domains;

namespace MyServiceBus.Server
{
    public class SettingsModel : IMyServiceBusSettings
    {

        public SettingsModel()
        {
            _eventuallyPersistentDelay = new Lazy<TimeSpan>(() => TimeSpan.Parse(EventuallyPersistenceDelay));
        }


        public string QueuesConnectionString { get; set; }
        public string MessagesConnectionString { get; set; }


        TimeSpan IMyServiceBusSettings.EventuallyPersistenceDelay => _eventuallyPersistentDelay.Value;

        private readonly Lazy<TimeSpan> _eventuallyPersistentDelay;
            
        
        public string EventuallyPersistenceDelay { get; set; }
    }
    
    public static class SettingsReader
    {

        public static SettingsModel ReadSettings()
        {

            var file = Environment.GetEnvironmentVariable("HOME").AddLastSymbolIfNotExists(Path.DirectorySeparatorChar)+".myservicebus";

            var fileData = File.ReadAllBytes(file);


            return MyYamlSettingsParser.MyYamlSettingsParser.ParseSettings<SettingsModel>(fileData);
        }
    
    }
}