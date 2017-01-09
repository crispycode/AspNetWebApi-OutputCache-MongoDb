using MongoDB.Driver;

namespace WebAPI.OutputCache.MongoDb.Tests
{
    public class MongoDbApiOutputCacheTestsBase
    {
        protected IMongoDatabase MongoDatabase;
        protected IMongoCollection<CachedItem> MongoCollection;

        protected MongoDbApiOutputCache MongoDbApiOutputCache;

        public MongoDbApiOutputCacheTestsBase()
        {
            var mongoUrl = new MongoUrl("mongodb://localhost/MongoDbApiOutputCache_Test");
            var client = new MongoClient(mongoUrl);            

            MongoDatabase = client.GetDatabase(mongoUrl.DatabaseName);
            MongoCollection = MongoDatabase.GetCollection<CachedItem>("cache");

            MongoDbApiOutputCache = new MongoDbApiOutputCache(MongoDatabase);
        }
    }
}