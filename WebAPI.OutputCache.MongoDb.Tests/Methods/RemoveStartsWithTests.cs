using System;
using System.Linq;
using NUnit.Framework;
using MongoDB.Driver;

namespace WebAPI.OutputCache.MongoDb.Tests.Methods
{
    [TestFixture]
    public class RemoveStartsWithTests : MongoDbApiOutputCacheTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            var item1 = new CachedItem("apples-1", "Golden Delicious", DateTime.Now.AddHours(1));
            var item2 = new CachedItem("apples-2", "Pink Lady", DateTime.Now.AddHours(1));
            var item3 = new CachedItem("dogs-1", "Jack Russell", DateTime.Now.AddHours(1));

            MongoCollection.InsertOne(item1);
            MongoCollection.InsertOne(item2);
            MongoCollection.InsertOne(item3);
        }

        [TearDown]
        public void TearDown()
        {
            MongoDatabase.DropCollection("cache");
        }

        [Test]
        public void removes_keys_starting_with_given_string()
        {            
            Assert.That(MongoCollection.AsQueryable().Count(), Is.EqualTo(3));

            MongoDbApiOutputCache.RemoveStartsWith("apples");

            var allItems = MongoCollection.AsQueryable();
            
            Assert.That(MongoCollection.AsQueryable().Count(), Is.EqualTo(1));            

            Assert.That(allItems.Any(x => x.Key.Equals("apples-1")), Is.False);
            Assert.That(allItems.Any(x => x.Key.Equals("apples-2")), Is.False);
        }
    }
}