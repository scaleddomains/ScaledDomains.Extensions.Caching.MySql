using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;

namespace ScaledDomains.Extensions.Caching.MySql
{
    /// <summary>
    /// <see cref="IDistributedCache"/> cache implementation using MySQL database.
    /// </summary>
    public class MySqlServerCache : IDistributedCache
    {
        private const int MaxKeyLength = 255;

        private readonly IDatabaseOperations _databaseOperations;

        public MySqlServerCache(IOptions<MySqlServerCacheOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(options.Value.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.Value.ConnectionString), $"{nameof(options.Value.ConnectionString)} cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(options.Value.TableName))
            {
                throw new ArgumentNullException(nameof(options.Value.TableName), $"{nameof(options.Value.TableName)} cannot be null or empty.");
            }

            _databaseOperations = new DatabaseOperations(options.Value);
        }

        /// <inheritdoc />
        public byte[] Get(string key)
        {
            ValidateKey(key);

            return _databaseOperations.GetCacheItem(key);
        }

        /// <inheritdoc />
        public async Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);

            return await _databaseOperations.GetCacheItemAsync(key, token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
           ValidateKey(key);

           _databaseOperations.SetCacheItem(key, value, options);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);

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

            if (key.Length >= MaxKeyLength)
            {
                throw new ArgumentOutOfRangeException(nameof(key), key.Length, $"{nameof(key)} length cannot be more than {MaxKeyLength}.");
            }
        }
    }
}
