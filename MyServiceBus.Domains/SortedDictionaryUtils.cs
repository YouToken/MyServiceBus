using System;
using System.Collections.Generic;
using System.Linq;

namespace MyServiceBus.Domains
{
    public static class SortedDictionaryUtils
    {

        public static IReadOnlyList<long> RemoveKeyLessThenValue<TValue>(this SortedDictionary<long, TValue> dict,
            long valueLessToDelete)
        {

            if (dict.Count == 0)
                return Array.Empty<long>();
            
            List<long> result = null;

            var firstKey = dict.Keys.First();
            while (firstKey < valueLessToDelete)
            {
                dict.Remove(firstKey);
                if (result == null)
                    result = new List<long>();
                
                result.Add(firstKey);

                if (dict.Count == 0)
                    break;

                firstKey = dict.Keys.First();
            }
            
            if (result == null)
                return Array.Empty<long>();

            return result;
        }


        public static IReadOnlyList<T> AddToReadOnlyList<T>(this IReadOnlyList<T> src, T newItem)
        {
            var result = new List<T>();
            result.AddRange(src);
            result.Add(newItem);

            return result;
        }
        
    }
}