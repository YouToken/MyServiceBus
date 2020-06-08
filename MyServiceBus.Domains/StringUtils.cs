using System;
using System.Text;

namespace MyServiceBus.Domains
{
    public static class StringUtils
    {

        public static string ToBase64(this byte[] src)
        {
            return Convert.ToBase64String(src);
        }

        
        public static string ToUtf8String(this byte[] src)
        {
            return Encoding.UTF8.GetString(src);
        }

    }
}