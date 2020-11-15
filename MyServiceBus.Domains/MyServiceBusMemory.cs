using System;

namespace MyServiceBus.Domains
{
    public static class MyServiceBusMemory
    {

        public static Func<int, byte[]> AllocateByteArray = size => new byte[size];

    }
}