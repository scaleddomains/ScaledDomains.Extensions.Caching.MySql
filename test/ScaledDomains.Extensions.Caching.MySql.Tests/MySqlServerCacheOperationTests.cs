using System;
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
    public class MySqlServerCacheOperationTests : IntegrationTestBase
    {
        private readonly IDistributedCache _mySqlServerCache;

        private readonly Mock<ISystemClock> _clock;

        private readonly DateTimeOffset _expired;
        private readonly DateTimeOffset _notExpired;

        public MySqlServerCacheOperationTests()
        {
            _clock = new Mock<ISystemClock>();
            _clock.Setup(p => p.UtcNow).Returns(_utcNow);

            _expired = _utcNow.AddMinutes(-5);
            _notExpired = _utcNow.AddMinutes(5);

            _mySqlServerCache = CreateMySqlServerCache(_clock.Object);
        }

        [TestInitialize]
        public void Setup()
        {
            base.ClearCache();
        }

        [TestMethod]
        public void Get_NotExpiredItem_ShouldReturnsItem()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _notExpired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray()
            };

            base.CreateCacheItem(testItem);

            // Act

            var actualItem = _mySqlServerCache.Get(testItem.Id);

            // Assert

            Assert.IsNotNull(actualItem);
            CollectionAssert.AreEquivalent(testItem.Value, actualItem);
        }

        [TestMethod]
        public async Task GetAsync_NotExpiredItem_ShouldReturnsItem()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _notExpired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray()
            };

            base.CreateCacheItem(testItem);

            // Act

            var actualItem = await _mySqlServerCache.GetAsync(testItem.Id);

            // Assert

            Assert.IsNotNull(actualItem);
            CollectionAssert.AreEquivalent(testItem.Value, actualItem);
        }

        [TestMethod]
        public void Get_NotExpiredItemWithSlidingExpiration_ShouldExtendExpiresAt()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _notExpired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray(),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            base.CreateCacheItem(testItem);

            // Act

            _mySqlServerCache.Get(testItem.Id);

            // Assert

            var actual = base.GetCacheItem(testItem.Id);
            Assert.AreEqual(_utcNow.Add(testItem.SlidingExpiration.Value), actual.ExpiresAt);
        }

        [TestMethod]
        public void Get_ExpiredItem_ShouldReturnsNull()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _expired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray()
            };

            base.CreateCacheItem(testItem);

            // Act

            var actualItem = _mySqlServerCache.Get(testItem.Id);

            // Assert

            Assert.IsNull(actualItem);
        }

        [TestMethod]
        public void Get_ItemDoesNotExist_ShouldReturnsNull()
        {
            // Act

            var actualItem = _mySqlServerCache.Get(Guid.NewGuid().ToString());

            // Assert

            Assert.IsNull(actualItem);
        }

        [TestMethod]
        public void Refresh_NotExpiredItem_ShouldUpdateExpiresAt()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _notExpired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray(),
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            base.CreateCacheItem(testItem);

            // Act

            _mySqlServerCache.Refresh(testItem.Id);

            // Assert

            var actualItem = GetCacheItem(testItem.Id);

            Assert.AreEqual(_utcNow.Add(testItem.SlidingExpiration.Value).UtcDateTime, actualItem.ExpiresAt);
        }

        [TestMethod]
        public void Refresh_ExpiredItem_ShouldNotUpdateExpiresAt()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _expired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray(),
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            base.CreateCacheItem(testItem);

            // Act

            _mySqlServerCache.Refresh(testItem.Id);

            // Assert

            var actualItem = GetCacheItem(testItem.Id);

            Assert.AreEqual(testItem.ExpiresAt, actualItem.ExpiresAt);
        }

        [TestMethod]
        public async Task RefreshAsync_NotExpiredItem_ShouldUpdateExpiresAt()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _notExpired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray(),
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            CreateCacheItem(testItem);

            // Act

            await _mySqlServerCache.RefreshAsync(testItem.Id);

            // Assert

            var actualItem = GetCacheItem(testItem.Id);

            Assert.AreEqual(_utcNow.Add(testItem.SlidingExpiration.Value).UtcDateTime, actualItem.ExpiresAt);
        }

        [TestMethod]
        public void Remove_ExistingNonExpiredItem_ShouldRemoveItem()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _notExpired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray(),
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            CreateCacheItem(testItem);

            // Act

            _mySqlServerCache.Remove(testItem.Id);

            // Assert

            var actualItem = GetCacheItem(testItem.Id);

            Assert.IsNull(actualItem);
        }

        [TestMethod]
        public async Task RemoveAsync_ExistingNonExpiredItem_ShouldRemoveItem()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _notExpired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray(),
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            CreateCacheItem(testItem);

            // Act

            await _mySqlServerCache.RemoveAsync(testItem.Id);

            // Assert

            var actualItem = GetCacheItem(testItem.Id);

            Assert.IsNull(actualItem);
        }

        [TestMethod]
        public void Remove_NonExistingItem_ShouldNotFail()
        {
            // Arrange

            var id = "nonExistingItemKey";

            // Act & Assert

            _mySqlServerCache.Remove(id);
        }

        [TestMethod]
        public async Task RemoveAsync_NonExistingItem_ShouldNotFail()
        {
            // Arrange

            var id = "nonExistingItemKey";

            // Act & Assert

            await _mySqlServerCache.RemoveAsync(id);
        }

        [Ignore]
        [TestMethod]
        public void Set_ShouldReturnsStoreItemIntoDatabase()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = _expired.UtcDateTime,
                Value = Guid.NewGuid().ToByteArray()
            };

            // Act

            _mySqlServerCache.Set(
                testItem.Id,
                testItem.Value,
                new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromHours(1)});

            // Assert

            var actualItem = base.GetCacheItem(testItem.Id);

            Assert.IsNotNull(actualItem);
            CollectionAssert.AreEquivalent(testItem.Value, actualItem.Value);
            Assert.AreEqual(testItem.ExpiresAt, actualItem.ExpiresAt);
        }
    }
}
