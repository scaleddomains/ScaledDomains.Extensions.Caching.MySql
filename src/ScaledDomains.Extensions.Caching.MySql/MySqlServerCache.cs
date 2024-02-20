using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace ScaledDomains.Extensions.Caching.MySql
{
    /// <summary>
    /// <see cref="IDistributedCache"/> cache implementation using MySQL database.
    /// </summary>
    public class MySqlServerCache : IDistributedCache
    {
        private readonly IDatabaseOperations _databaseOperations;

        public MySqlServerCache(IDatabaseOperations databaseOperations)
        {
            _databaseOperations = databaseOperations ?? throw new ArgumentNullException(nameof(databaseOperations));
        }

        /// <inheritdoc />
        public byte[]? Get(string key)
        {
            ValidateKey(key);

            var result = _databaseOperations.GetCacheItem(key);

            return result;
        }

        /// <inheritdoc />
        public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);

            var result = await _databaseOperations.GetCacheItemAsync(key, token);

            return result;
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            ValidateKey(key);

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), $"{nameof(value)} cannot be null.");
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null.");
            }

            _databaseOperations.SetCacheItem(key, value, options);
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);
            
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null.");
            }
            
            await _databaseOperations.SetCacheItemAsync(key, value, options, token);
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            ValidateKey(key);

            _databaseOperations.RefreshCacheItem(key);
        }

        /// <inheritdoc />
        public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);

            await _databaseOperations.RefreshCacheItemAsync(key, token);
        }

        public void Remove(string key)
        {
            ValidateKey(key);

            _databaseOperations.DeleteCacheItem(key);
        }

        public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);

            await _databaseOperations.DeleteCacheItemAsync(key, token);
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key), $"{nameof(key)} cannot be null or empty.");
            }

            if (key.Length > DatabaseOperations.IdColumnSize)
            {
                throw new ArgumentOutOfRangeException(nameof(key), key.Length, $"{nameof(key)} length cannot be more than {DatabaseOperations.IdColumnSize}.");
            }
        }
    }
}
