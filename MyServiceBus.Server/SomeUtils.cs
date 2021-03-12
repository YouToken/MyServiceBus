namespace MyServiceBus.Server
{
    public static class SomeUtils
    {

        public static string ByteSizeToString(this long size)
        {
            return ((double) size).SizeToString();
        }

        private static string SizeToString(this double size)
        {
            if (size < 1024)
                return size.ToString("0.##") + "b";

            size /=  1024;
            
            if (size<1024)
                return size.ToString("0.##") + "Kb";
            
            size /= 1024;
            
            if (size<1024)
                return size.ToString("0.##") + "Mb";
            
            size /= 1024;

            return size.ToString("0.##") + "Gb";
        } 
        
    }
}