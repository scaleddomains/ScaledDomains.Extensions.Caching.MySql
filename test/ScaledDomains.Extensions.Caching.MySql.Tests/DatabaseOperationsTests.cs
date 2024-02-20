using System;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ScaledDomains.Extensions.Caching.MySql.Tests;

[TestClass]
[TestCategory(TestCategoryNames.Unit)]
public class DatabaseOperationsTests
{
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAnInstance_WithNullOptions_ShouldThrowArgumentNullException()
        {
            _ = new DatabaseOperations(null!, Mock.Of<ISystemClock>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAnInstance_WithNullSystemClock_ShouldThrowArgumentNullException()
        {
            var optionsMock = new Mock<IOptions<MySqlServerCacheOptions>>();
            optionsMock.SetupGet(m => m.Value).Returns(new MySqlServerCacheOptions());
            _ = new DatabaseOperations(optionsMock.Object, null!);
        }
}
