using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            _optionsMock.SetupGet(o => o.Value).Returns(() => new MySqlServerCacheOptions());
            _ = CreateMySqlServerCacheMaintenanceService(_optionsMock.Object, _loggerMock.Object, _databaseOperationsMock.Object);
        }

        private static MySqlServerCacheMaintenanceService CreateMySqlServerCacheMaintenanceService(IOptions<MySqlServerCacheOptions> options, ILogger<MySqlServerCacheMaintenanceService> logger, IDatabaseOperations databaseOperations)
        {
            return new MySqlServerCacheMaintenanceService(options, logger, databaseOperations);
        }

        [TestMethod]
        public void CreateAnInstance_WithNullOptions_ShouldThrowArgumentNullException()
        {            
            var excpetion = Assert.ThrowsException<ArgumentNullException>(() => CreateMySqlServerCacheMaintenanceService(null!, _loggerMock.Object, _databaseOperationsMock.Object));
            Assert.IsTrue(excpetion.Message.Contains("options"));
            _optionsMock.SetupGet(o => o.Value).Returns(() => null);
            excpetion = Assert.ThrowsException<ArgumentNullException>(() => CreateMySqlServerCacheMaintenanceService(_optionsMock.Object, _loggerMock.Object, _databaseOperationsMock.Object));
            Assert.IsTrue(excpetion.Message.Contains("options"));
        }

        [TestMethod]
        public void CreateAnInstance_WithNullLogger_ShouldThrowArgumentNullException()
        {
            _optionsMock.SetupGet(o => o.Value).Returns(() => new MySqlServerCacheOptions() { ExpirationScanFrequency = TimeSpan.FromSeconds(30) });
            var excpetion = Assert.ThrowsException<ArgumentNullException>(() => CreateMySqlServerCacheMaintenanceService(_optionsMock.Object, null!, _databaseOperationsMock.Object));
            Assert.IsTrue(excpetion.Message.Contains("logger"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAnInstance_WithNullDatabaseOperations_ShouldThrowArgumentNullException()
        {
            _optionsMock.SetupGet(o => o.Value).Returns(() => new MySqlServerCacheOptions());
            _ = CreateMySqlServerCacheMaintenanceService(_optionsMock.Object, _loggerMock.Object, null!);
        }

        [TestMethod]
        public async Task ExecuteAsync_DeleteExpiredCacheItemsAsyncInvokeOnce()
        {
            // Arrange
            _optionsMock.SetupGet(o => o.Value).Returns(() => new MySqlServerCacheOptions());
            using var instance = CreateMySqlServerCacheMaintenanceService(_optionsMock.Object, _loggerMock.Object, _databaseOperationsMock.Object);
            var cancellationToken = CancellationToken.None;

            // Act

            await instance.StartAsync(cancellationToken);

            // Assert

            _databaseOperationsMock.Verify(m => m.DeleteExpiredCacheItemsAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [TestMethod]
        [TestCategory(TestCategoryNames.Integration)]
        public async Task ExecuteAsync_ShouldDeleteExpiredCacheItems()
        {
            // Arrange

            var utcNow = new DateTimeOffset(2024, 2, 20, 9, 44, 0, DateTimeOffset.UtcNow.Offset);
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(m => m.UtcNow).Returns(utcNow);

            var notExpiredItem = new CacheItem
            {
                Id = "c1",
                Value = Guid.NewGuid().ToByteArray(),
                ExpiresAt = utcNow.Add(TimeSpan.FromMinutes(5)).UtcDateTime,
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            var expiredItem = new CacheItem
            {
                Id = "c2",
                Value = Guid.NewGuid().ToByteArray(),
                ExpiresAt = utcNow.Add(TimeSpan.FromMinutes(-5)).UtcDateTime,
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            _optionsMock.SetupGet(o => o.Value).Returns(TestConfiguration.MySqlServerCacheOptions);

            CreateCacheItem(TestConfiguration.MySqlServerCacheOptions.ConnectionString, TestConfiguration.MySqlServerCacheOptions.TableName, notExpiredItem);
            CreateCacheItem(TestConfiguration.MySqlServerCacheOptions.ConnectionString, TestConfiguration.MySqlServerCacheOptions.TableName, expiredItem);

            using var instance = new MySqlServerCacheMaintenanceService(_optionsMock.Object, _loggerMock.Object, new DatabaseOperations(_optionsMock.Object, clockMock.Object));

            // Act

            await instance.StartAsync(CancellationToken.None);
            await Task.Delay(30);

            // Assert

            Assert.IsNotNull(GetCacheItem(TestConfiguration.MySqlServerCacheOptions.ConnectionString, TestConfiguration.MySqlServerCacheOptions.TableName, notExpiredItem.Id));
            Assert.IsNull(GetCacheItem(TestConfiguration.MySqlServerCacheOptions.ConnectionString, TestConfiguration.MySqlServerCacheOptions.TableName, expiredItem.Id));
        }
    }
}
