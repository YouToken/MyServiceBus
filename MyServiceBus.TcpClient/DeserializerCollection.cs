using System;
using System.Collections.Generic;

namespace MyServiceBus.TcpClient
{
    public class DeserializerCollection
    {
        private readonly Dictionary<Type, Func<byte[], object>> _deserializers
            = new Dictionary<Type, Func<byte[], object>>();

        private Func<byte[], object> _defaultDeserializer;
        
        public void RegisterDeserializer(Type type, Func<byte[], object> deserializeFunc)
        {

            if (_deserializers.ContainsKey(type))
                throw new Exception($"Deserializer is already registered for type {type}");

            _deserializers.Add(type, deserializeFunc);
        }
        
        public void RegisterDefaultDeserializer(Func<byte[], object> defaultSerializer)
        {
            if (_defaultDeserializer != null)
                throw new Exception("Default Deserializer is already registered");
            
            _defaultDeserializer = defaultSerializer;
        }

        public Func<byte[], object> Get(Type type)
        {
            
            var deserializer =  _deserializers.ContainsKey(type) 
                ? _deserializers[type] 
                : _defaultDeserializer;
            
            if (deserializer == null)
                throw new Exception($"Deserializer not found for type {type}");

            return deserializer;
        }
    }
}