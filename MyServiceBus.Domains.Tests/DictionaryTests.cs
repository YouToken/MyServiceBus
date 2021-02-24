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
        
        
    }
}