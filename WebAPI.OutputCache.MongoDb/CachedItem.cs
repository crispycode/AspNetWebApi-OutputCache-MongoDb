using System;
using ServiceStack.Text;
using MongoDB.Bson.Serialization.Attributes;
using WebAPI.OutputCache.MongoDb.Utilities;

namespace WebAPI.OutputCache.MongoDb
{
    public class CachedItem
    {
        public CachedItem(string key, object value, DateTime expireAt)
        {
            Key = key;
            Value = JsonSerializer.SerializeToString(value);
            ValueType = value.GetType().AssemblyQualifiedName;

            ExpireAt = expireAt;
        }

        [BsonId]
        public string Key { get; set; }

        public string Value { get; set; }
        public string ValueType { get; private set; }

        [BsonElement("expireAt")]
        [BsonSerializer(typeof(UtcDateTimeSerializer))]       
        public DateTime ExpireAt { get; set; }
    }
}