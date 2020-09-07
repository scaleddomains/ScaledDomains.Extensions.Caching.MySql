using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;

namespace ScaledDomains.Extensions.Caching.MySql
{
    /// <summary>
    /// <see cref="IDistributedCache"/> cache implementation using MySQL database.
    /// </summary>
    public class MySqlServerCache : IDistributedCache
    {
        private readonly IDatabaseOperations _databaseOperations;

        private readonly ISystemClock _clock;
        
        public MySqlServerCache(IOptions<MySqlServerCacheOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null.");
            }

            options.Value.Validate();

            _databaseOperations = options.Value.DatabaseOperations ?? new DatabaseOperations(options.Value);

            _clock = options.Value.SystemClock;
        }

        /// <inheritdoc />
        public byte[] Get(string key)
        {
            ValidateKey(key);

            var result = _databaseOperations.GetCacheItem(key);

            return result;
        }

        /// <inheritdoc />
        public async Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            ValidateKey(key);

            var result = await _databaseOperations.GetCacheItemAsync(key, token);

            return result;
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

            var s = Encoding.ASCII.GetString(Encoding.Default.GetBytes(key));

            if (key.Length > DatabaseOperations.IdColumnSize)
            {
                throw new ArgumentOutOfRangeException(nameof(key), key.Length, $"{nameof(key)} length cannot be more than {DatabaseOperations.IdColumnSize}.");
            }
        }
    }
}
