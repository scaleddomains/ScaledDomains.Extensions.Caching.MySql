using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    [TestClass]
    [TestCategory(TestCategoryNames.Unit)]
    public class MySqlServerCacheTests
    {
        [TestMethod]
        public void CreateAnInstance_WithValidConfiguration_ShouldCreate()
        {
            var databaseOperationsMock = new Mock<IDatabaseOperations>();
            _ = new MySqlServerCache(databaseOperationsMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAnInstance_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            _ = new MySqlServerCache(null!);
        }
    }
}
