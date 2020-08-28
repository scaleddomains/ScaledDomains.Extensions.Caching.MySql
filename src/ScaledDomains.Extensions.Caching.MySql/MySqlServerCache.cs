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
        public Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);

            return _databaseOperations.GetCacheItemAsync(key, token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
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
