using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyTcpSockets.Extensions;

namespace MyServiceBus.TcpContracts.Tests
{
    public static class AsyncEnumerableUtils
    {

        public static async Task<IReadOnlyList<T>> AsReadOnlyListAsync<T>(this IAsyncEnumerable<T> src)
        {
            var result = new List<T>();
            await foreach (var itm in src)
            {
                result.Add(itm);
            }

            return result;
        }

        public static IReadOnlyList<T> AsReadOnlyList<T>(this IAsyncEnumerable<T> src)
        {
            return src.AsReadOnlyListAsync().Result;
        }

        
    }
}