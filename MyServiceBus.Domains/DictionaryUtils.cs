using System;
using System.Collections.Generic;

namespace MyServiceBus.Domains
{
    public static class DictionaryUtils
    {

        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> src, TKey key) where TValue : new()
        {

            if (src.ContainsKey(key))
                return src[key];
            
            var result = new TValue();
            
            src.Add(key, result);

            return result;
        }
        
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> src, TKey key, Func<TValue> create)
        {

            if (src.ContainsKey(key))
                return src[key];
            
            var result = create();
            
            src.Add(key, result);

            return result;
        }

        
    }
}