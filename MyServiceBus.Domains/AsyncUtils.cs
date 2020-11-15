using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyServiceBus.Domains
{
    public static class AsyncUtils
    {

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var result = new List<T>();

            await foreach (var itm in source)
            {
                result.Add(itm);
            }

            return result;
        }
        
    }
}