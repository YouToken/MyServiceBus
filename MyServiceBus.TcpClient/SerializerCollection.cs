using System;
using System.Collections.Generic;

namespace MyServiceBus.TcpClient
{
    public class SerializerCollection
    {
        private readonly Dictionary<Type, Func<object, byte[]>> _serializers
            = new Dictionary<Type, Func<object, byte[]>>();
        
        private Func<object, byte[]> _defaultSerializer;

        public void RegisterSerializer(Type type, Func<object, byte[]> serializeFunc)
        {

            if (_serializers.ContainsKey(type))
                throw new Exception($"Serializer {type} is already registered");

            _serializers.Add(type, serializeFunc);
           
        }
        
        public void RegisterDefaultSerializer(Func<object, byte[]> defaultSerializer)
        {
            
            if (_defaultSerializer != null)
                throw new Exception("Default Serializer is already registered");            
            
            _defaultSerializer = defaultSerializer;
        }

        public byte[] Serialize<T>(T valueToPublish)
        {
            var type = typeof(T);
            var serializer = _serializers.ContainsKey(type) ? _serializers[type] : _defaultSerializer;
            
            if (serializer == null)
                throw new Exception($"Serializer not found for type {type}");

            return serializer(valueToPublish);
        }
    }
}