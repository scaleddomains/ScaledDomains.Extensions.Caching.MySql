using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    [TestClass]
    [Ignore]
    [TestCategory(TestCategoryNames.Integration)]
    public class MySqlServerCacheOperationTests : IntegrationTestBase
    {
        private readonly IDistributedCache _mySqlServerCache;

        private readonly DateTimeOffset _expired;
        private readonly DateTimeOffset _notExpired;

        public MySqlServerCacheOperationTests()
        {
            var clock = new Mock<ISystemClock>();
            clock.Setup(p => p.UtcNow).Returns(_utcNow);

            _mySqlServerCache = CreateMySqlServerCache(clock.Object);
            
            _expired = _utcNow.AddMinutes(-5);
            _notExpired = _utcNow.AddMinutes(5);
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

        [TestMethod]
        public void Set_ShouldStoreCacheItemIntoDatabase()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = DateTime.MinValue,
                Value = Guid.NewGuid().ToByteArray()
            };

            // Act

            _mySqlServerCache.Set(
                testItem.Id,
                testItem.Value,
                new DistributedCacheEntryOptions { AbsoluteExpiration = _notExpired });

            // Assert

            var actualItem = base.GetCacheItem(testItem.Id);

            Assert.IsNotNull(actualItem);
            CollectionAssert.AreEquivalent(testItem.Value, actualItem.Value);
        }

        [TestMethod]
        public async Task SetAsync_ShouldStoreCacheItemIntoDatabase()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = DateTime.MinValue,
                Value = Guid.NewGuid().ToByteArray()
            };

            // Act

            await _mySqlServerCache.SetAsync(
                testItem.Id,
                testItem.Value,
                new DistributedCacheEntryOptions { AbsoluteExpiration = _notExpired });

            // Assert

            var actualItem = base.GetCacheItem(testItem.Id);

            Assert.IsNotNull(actualItem);
            CollectionAssert.AreEquivalent(testItem.Value, actualItem.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Set_WithNullData_ShouldThrowArgumentNullException()
        {
            _mySqlServerCache.Set(
                "myKey",
                null,
                new DistributedCacheEntryOptions { AbsoluteExpiration = _notExpired });
        }

        [TestMethod]
        public void Set_WithAbsoluteExpiration_ShouldSetExpiresAtAndSlidingExpirationShouldBeNull()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = DateTime.MinValue,
                Value = Guid.NewGuid().ToByteArray()
            };

            var absoluteExpiration = _utcNow.AddHours(1);

            // Act

            _mySqlServerCache.Set(
                testItem.Id,
                testItem.Value,
                new DistributedCacheEntryOptions { AbsoluteExpiration = absoluteExpiration });

            // Assert

            var actualItem = base.GetCacheItem(testItem.Id);

            Assert.AreEqual(absoluteExpiration.UtcDateTime, actualItem.ExpiresAt);
            Assert.AreEqual(absoluteExpiration.UtcDateTime, actualItem.AbsoluteExpiration);
            Assert.IsNull(actualItem.SlidingExpiration);
        }

        [TestMethod]
        public void Set_WithAbsoluteExpirationRelativeToNow_ShouldSetExpiresAtAndSlidingExpirationShouldBeNull()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = DateTime.MinValue,
                Value = Guid.NewGuid().ToByteArray()
            };

            var absoluteExpiration = TimeSpan.FromHours(1);

            // Act

            _mySqlServerCache.Set(
                testItem.Id,
                testItem.Value,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration });

            // Assert

            var actualItem = base.GetCacheItem(testItem.Id);

            Assert.AreEqual(_utcNow.Add(absoluteExpiration).UtcDateTime, actualItem.ExpiresAt);
            Assert.AreEqual(_utcNow.Add(absoluteExpiration).UtcDateTime, actualItem.AbsoluteExpiration);
            Assert.IsNull(actualItem.SlidingExpiration);
        }

        [TestMethod]
        public void Set_WithSlidingExpiration_ShouldSetExpiresAtAndAbsoluteExpirationShouldBeNull()
        {
            // Arrange

            var testItem = new CacheItem
            {
                Id = "myKey",
                ExpiresAt = DateTime.MinValue,
                Value = Guid.NewGuid().ToByteArray()
            };

            var slidingExpiration = TimeSpan.FromMilliseconds(1234.567);

            // Act

            _mySqlServerCache.Set(
                testItem.Id,
                testItem.Value,
                new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration });

            // Assert

            var actualItem = base.GetCacheItem(testItem.Id);

            Assert.AreEqual(_utcNow.Add(slidingExpiration).UtcDateTime, actualItem.ExpiresAt);
            Assert.AreEqual(slidingExpiration, actualItem.SlidingExpiration);
            Assert.IsNull(actualItem.AbsoluteExpiration);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Set_WithInvalidCacheEntryOption_ShouldThrowInvalidOperationException()
        {
            _mySqlServerCache.Set(
                "myKey",
                new byte[] { 1, 2, 3 },
                new DistributedCacheEntryOptions { SlidingExpiration = null, AbsoluteExpiration = null });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Set_WithInvalidNullCacheEntryOption_ShouldThrowArgumentNullException()
        {
            _mySqlServerCache.Set(
                "myKey",
                new byte[] { 1, 2, 3 },
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetAsync_WithInvalidNullCacheEntryOption_ShouldThrowArgumentNullException()
        {
            await _mySqlServerCache.SetAsync(
                "myKey",
                new byte[] { 1, 2, 3 },
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Set_WithInvalidAbsoluteExpirationValue_ShouldThrowInvalidOperationException()
        {
            _mySqlServerCache.Set(
                "myKey",
                new byte[] { 1, 2, 3 },
                new DistributedCacheEntryOptions { SlidingExpiration = null, AbsoluteExpiration = _utcNow.AddMilliseconds(-1) });
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("\t \n")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Set_WithNullOrEmptyWhiteSpaceKey_ShouldThrowArgumentNullException(string key)
        {
            _mySqlServerCache.Set(
                key,
                new byte[] { 1, 2, 3 },
                new DistributedCacheEntryOptions { AbsoluteExpiration = _utcNow.AddMilliseconds(1) });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Set_WithTooLongKey_ShouldThrowArgumentOutOfRangeException()
        {
            _mySqlServerCache.Set(
                string.Join("", Enumerable.Repeat("K", 767 + 1)),
                new byte[] { 1, 2, 3 },
                new DistributedCacheEntryOptions { AbsoluteExpiration = _utcNow.AddMilliseconds(1) });
        }

        [TestMethod]

        public void SetAsync_MultipleSetWithSameKey_ShouldStoreOnlyOneItem()
        {
            const byte count = 128;
            const string keyName = "myKey";
            var signal = new ManualResetEventSlim();

            var tasks = new Task[count];

            for (var i = 0; i < count; i++)
            {
                var _1 = i;
                tasks[i] = Task.Run(() =>
                {
                    signal.Wait();

                    return _mySqlServerCache.SetAsync(
                        keyName,
                        new byte[] { 1, 2, 3 },
                        new DistributedCacheEntryOptions { AbsoluteExpiration = _notExpired });
                });
            }

            signal.Set();

            Task.WaitAll(tasks);

            Assert.IsNotNull(base.GetCacheItem(keyName));
        }
    }
}
