using System.Collections.Generic;
using NUnit.Framework;

namespace MyServiceBus.Domains.Tests
{
    public class DictionaryTests
    {

        [Test]
        public void TestInsertCase()
        {

            var dict = new Dictionary<string, string>();

            var result = dict.AddIfNotExistsByCreatingNewDictionary("test1", ()=>"test1");

            if (result.added)
                dict = result.newDictionary;
            
            Assert.IsTrue(result.added);
            
            Assert.IsTrue(dict.ContainsKey("test1"));

        }
        
        [Test]
        public void TestNotInsertCase()
        {

            var dict = new Dictionary<string, string>
            {
                ["test1"] = "test1"
            };

            var result = dict.AddIfNotExistsByCreatingNewDictionary("test1", ()=>"test1");
            
            Assert.IsFalse(result.added);
            
        }
        
        
        [Test]
        public void TestDeleteCase()
        {

            var dict = new Dictionary<string, string>
            {
                ["test1"] = "test1"
            };

            var result = dict.RemoveIfExistsByCreatingNewDictionary("test1", (k1, k2) => k1 == k2);

            if (result.removed)
                dict = result.result;
            
            Assert.IsTrue(result.removed);
            Assert.IsFalse(dict.ContainsKey("test1"));
        }
        
                
        [Test]
        public void TestDeleteNotExistsCase()
        {

            var dict = new Dictionary<string, string>
            {
                ["test1"] = "test1"
            };

            var result = dict.RemoveIfExistsByCreatingNewDictionary("test2", (k1, k2) => k1 == k2);

            if (result.removed)
                dict = result.result;
            
            Assert.IsFalse(result.removed);
            Assert.IsTrue(dict.ContainsKey("test1"));
        }
        
    }
}