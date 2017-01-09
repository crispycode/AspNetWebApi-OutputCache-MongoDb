using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using WebApi.OutputCache.Core.Cache;
using JsonSerializer = ServiceStack.Text.JsonSerializer;

namespace WebAPI.OutputCache.MongoDb
{
    public class MongoDbApiOutputCache : IApiOutputCache
    {
        internal readonly IMongoCollection<CachedItem> MongoCollection;

        public IEnumerable<string> AllKeys
        {
            get { return MongoCollection.AsQueryable().Select(x => x.Key); }
        }

        public MongoDbApiOutputCache(IMongoDatabase mongoDatabase)
            : this(mongoDatabase, "cache")
        {            
        }        

        public MongoDbApiOutputCache(IMongoDatabase mongoDatabase, string cacheCollectionName)
        {                        
            MongoCollection = mongoDatabase.GetCollection<CachedItem>(cacheCollectionName);

            MongoCollection.Indexes.CreateOne(Builders<CachedItem>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = TimeSpan.FromMilliseconds(0) });            
        }

        public void RemoveStartsWith(string key)
        {
            var filter = Builders<CachedItem>.Filter.Regex("_id", new BsonRegularExpression("^" + key));
            MongoCollection.DeleteMany(filter);
        }

        public T Get<T>(string key) where T : class
        {
            var filter = Builders<CachedItem>.Filter.Eq("_id", new BsonString(key));

            var item = MongoCollection.Find(filter).FirstOrDefault();

            if (item == null)
                return null;

            return CheckItemExpired(item)
                ? null
                : JsonSerializer.DeserializeFromString<T>(item.Value);
        }

        public object Get(string key)
        {
            var filter = Builders<CachedItem>.Filter.Eq("_id", new BsonString(key));
            var item = MongoCollection.Find(filter).FirstOrDefault();

            if (item == null)
            {
                return null;
            }
            
            var type = Type.GetType(item.ValueType);
            return JsonSerializer.DeserializeFromString(item.Value, type);
        }

        public void Remove(string key)
        {
            var filter = Builders<CachedItem>.Filter.Eq("_id", new BsonString(key));
            MongoCollection.DeleteOne(filter);
        }

        public bool Contains(string key)
        {
            var filter = Builders<CachedItem>.Filter.Eq("_id", new BsonString(key));

            return MongoCollection.Find(filter).Count() == 1;
        }

        public void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null)
        {
            if (key.Length > 256) //saves calling getByteCount if we know it could be less than 1024 bytes
                if (Encoding.UTF8.GetByteCount(key) >= 1024)
                    throw new KeyTooLongException();

            var cachedItem = new CachedItem(key, o, expiration.DateTime);

            var filter = Builders<CachedItem>.Filter.Eq("_id", new BsonString(key));
            MongoCollection.ReplaceOne(filter, cachedItem, new UpdateOptions { IsUpsert = true });
        }

        private bool CheckItemExpired(CachedItem item)
        {
            if (item.ExpireAt >= DateTime.UtcNow)
                return false;

            //does the work of TTL collections early - TTL is only "fired" once a minute or so
            Remove(item.Key);

            return true;
        }
    }
}