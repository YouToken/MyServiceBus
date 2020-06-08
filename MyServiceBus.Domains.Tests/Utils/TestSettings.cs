using System;
using MyServiceBus.Domains;

namespace MyServiceBus.Domains.Tests.Utils
{
    public class TestSettings : IMyServiceBusSettings
    {
        public TimeSpan SessionTimeOut = TimeSpan.FromMinutes(1);
        public TimeSpan EventuallyPersistenceDelay { get; set; } = TimeSpan.FromSeconds(5);
    }
}