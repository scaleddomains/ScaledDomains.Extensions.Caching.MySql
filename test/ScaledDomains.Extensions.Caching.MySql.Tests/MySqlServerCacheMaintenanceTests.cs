using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    [TestClass]
    [TestCategory(TestCategoryNames.Integration)]
    public class MySqlServerCacheMaintenanceServiceTests : IntegrationTestBase
    {

        [TestMethod]
        public void CreateAnInstance_WithValidConfiguration_ShouldCreate()
        {
            _ = new MySqlServerCacheMaintenanceService(new MySqlServerCacheOptions
            {
                ConnectionString = "Server=example.com;Database=db;User=root;SslMode=None;",
                TableName = "table"
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAnInstance_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            _ = new MySqlServerCacheMaintenanceService(null);
        }

        [Ignore]
        [TestMethod]
        public async Task ExecuteAsync_DeleteExpiredCacheItemsAsyncInvokeOnce()
        {
            // Arrange

            var databaseOperationMock = new Mock<IDatabaseOperations>();

            var config = TestConfiguration.MySqlServerCacheOptions.Clone();
            config.DatabaseOperations = databaseOperationMock.Object;

            using var instance = new MySqlServerCacheMaintenanceService(config);
            
            // Act

            await instance.StartAsync(CancellationToken.None);

            // Assert

            databaseOperationMock.Verify(m => m.DeleteExpiredCacheItemsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Ignore]
        [TestMethod]
        [TestCategory(TestCategoryNames.Integration)]
        public async Task ExecuteAsync_ShouldDeleteExpiredCacheItems()
        {
            // Arrange

            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(m => m.UtcNow).Returns(_utcNow);

            var config = TestConfiguration.MySqlServerCacheOptions.Clone();
            config.SystemClock = clockMock.Object;

            var notExpiredItem = new CacheItem
            {
                Id = "c1",
                Value = Guid.NewGuid().ToByteArray(),
                ExpiresAt = _utcNow.Add(TimeSpan.FromMinutes(5)).UtcDateTime,
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            var expiredItem = new CacheItem
            {
                Id = "c2",
                Value = Guid.NewGuid().ToByteArray(),
                ExpiresAt = _utcNow.Add(TimeSpan.FromMinutes(-5)).UtcDateTime,
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            base.CreateCacheItem(notExpiredItem);
            base.CreateCacheItem(expiredItem);

            using var instance = new MySqlServerCacheMaintenanceService(config);
            
            // Act

            await instance.StartAsync(CancellationToken.None);
            await Task.Delay(30);

            // Assert

            Assert.IsNotNull(base.GetCacheItem(notExpiredItem.Id));
            Assert.IsNull(base.GetCacheItem(expiredItem.Id));
        }
    }
}
