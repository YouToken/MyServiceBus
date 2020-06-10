using System;
using System.IO;
using DotNetCoreDecorators;
using Microsoft.Extensions.Configuration;
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
            var homeFolder = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(homeFolder) && File.Exists(Path.Combine(homeFolder, ".myservicebus")))
            {
                var file = Path.Combine(homeFolder, ".myservicebus");
                var fileData = File.ReadAllBytes(file);
                return MyYamlSettingsParser.MyYamlSettingsParser.ParseSettings<SettingsModel>(fileData);
            }
            homeFolder = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH");
            if (!string.IsNullOrEmpty(homeFolder) && File.Exists(Path.Combine(homeFolder, ".myservicebus")))
            {
                var file = Path.Combine(homeFolder, ".myservicebus");
                var fileData = File.ReadAllBytes(file);
                return MyYamlSettingsParser.MyYamlSettingsParser.ParseSettings<SettingsModel>(fileData);
            }

            var configBuilder = new ConfigurationBuilder();

            homeFolder = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(homeFolder) && File.Exists(Path.Combine(homeFolder, ".myservicebus.json")))
            {
                FileStream fileStream = new FileStream(Path.Combine(homeFolder, ".myservicebus.json"), FileMode.Open);
                configBuilder.AddJsonStream(fileStream);
            }

            homeFolder = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH");
            if (!string.IsNullOrEmpty(homeFolder) && File.Exists(Path.Combine(homeFolder, ".myservicebus.json")))
            {
                FileStream fileStream = new FileStream(Path.Combine(homeFolder, ".myservicebus.json"), FileMode.Open);
                configBuilder.AddJsonStream(fileStream);
            }

            configBuilder.AddEnvironmentVariables();

            var config = configBuilder.Build();

            var data = config.Get<SettingsModel>();

            if (string.IsNullOrEmpty(data.MessagesConnectionString))
            {
                Console.WriteLine("MessagesConnectionString should be set in config (~/.myservicebus.json) or env variable");
                throw new Exception("MessagesConnectionString should be set in config (~/.myservicebus.json) or env variable");
            }

            if (string.IsNullOrEmpty(data.QueuesConnectionString))
            {
                Console.WriteLine("QueuesConnectionString should be set i config or env variable");
                throw new Exception("QueuesConnectionString should be set i config or env variable");
            }

            return data;
        }
    
    }
}