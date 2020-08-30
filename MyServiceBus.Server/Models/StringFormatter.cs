using System;
using ProtoBuf.WellKnownTypes;

namespace MyServiceBus.Server.Models
{
    public static class StringFormatter
    {

        public static string FormatTimeStamp(this TimeSpan ts)
        {
            if (ts.TotalSeconds < 1)
                return ts.TotalMilliseconds.ToString("000000000") + "ms";
            
            if (ts.TotalMinutes < 1)
                return ts.Seconds.ToString("00")+"s";

            if (ts.TotalHours < 1)
                return ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            
            if (ts.TotalHours < 24)
                return ts.Hours.ToString("00")+":"+ ts.Minutes.ToString("00")+":"+ts.Seconds.ToString("00");
            
            return ts.Days.ToString("00")+"d "+ts.Hours.ToString("00")+":"+ ts.Minutes.ToString("00")+":"+ts.Seconds.ToString("00");
        }
        
    }
}