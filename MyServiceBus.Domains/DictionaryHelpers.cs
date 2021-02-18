using System;
using System.Collections.Generic;

namespace MyServiceBus.Domains
{
    public static class DictionaryHelpers
    {
        
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> src, TKey key) where TValue : new()
        {

            if (src.ContainsKey(key))
                return src[key];
            
            var result = new TValue();
            
            src.Add(key, result);

            return result;
        }
        
        public static (TValue value, bool created) GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> src, TKey key, Func<TValue> create)
        {

            if (src.TryGetValue(key, out var foundValue))
                return (foundValue, false);
            
            var result = create();
            src.Add(key, result);
            return (result, true);
        }
        
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getValue)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, getValue());

            return dictionary[key];
        }

        public static void AddIfNotExists<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
        }

        
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
            else
                dictionary[key] = value;
        }
        
                
        
        public static (bool added, Dictionary<TKey, TValue> newDictionary, TValue value) AddIfNotExistsByCreatingNewDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
            TKey key, Func<TValue> getValue)
        {
            if (dictionary.TryGetValue(key, out var foundValue))
                return (false, dictionary, foundValue);

            var newValue = getValue();
            var result = new Dictionary<TKey, TValue>(dictionary) {{key, newValue}};
            return (true, result, newValue);
        }

        public static (bool removed, Dictionary<TKey, TValue> result) RemoveIfExistsByCreatingNewDictionary<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary, TKey keyToRemove, Func<TKey, TKey, bool> keysAreEqual)
        {

            if (!dictionary.ContainsKey(keyToRemove))
                return (false, dictionary);

            var result = new Dictionary<TKey, TValue>();

            foreach (var (key, value) in dictionary)
            {
                if (keysAreEqual(key, keyToRemove))
                    continue;
                
                result.Add(key, value);
            }

            return (true, result);
        }
    }
}