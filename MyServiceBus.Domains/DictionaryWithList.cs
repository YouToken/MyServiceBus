using System;
using System.Collections.Generic;
using System.Linq;

namespace MyServiceBus.Domains
{
    public class DictionaryWithList<TKey, TValue>
    {

        private readonly Dictionary<TKey, TValue> _dictionary = new ();
        private IReadOnlyList<TValue> _itemsAsList = Array.Empty<TValue>();
        
        public int SnapshotId { get; private set; }
        
        public void Add(TKey key, TValue value)
        {
            lock (_dictionary)
            {
                _dictionary.Add(key, value);
                _itemsAsList = _dictionary.Values.ToList();
                SnapshotId++;
            }
        }

        public bool Remove(TKey key)
        {
            lock (_dictionary)
            {
                var result = _dictionary.Remove(key);
                if (result)
                {
                    _itemsAsList = _dictionary.Values.ToList();
                    SnapshotId++;
                }
                return result;
            }
        }


        public IReadOnlyList<TValue> GetAllValues()
        {
            return _itemsAsList;
        }

        public TValue this[TKey key] => _dictionary[key];
    }

}