using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    [TestClass]
    public class MySqlServerCacheBaseTests
    {
        [TestMethod]
        public void CreateAnInstance_WithValidConfiguration_ShouldCreate()
        {
            _ = new MySqlServerCache(new MySqlServerCacheOptions
            {
                ConnectionString = "Server=example.com;Database=db;User=root;",
                TableName = "table"
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAnInstance_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            _ = new MySqlServerCache(null);
        }
    }
}
