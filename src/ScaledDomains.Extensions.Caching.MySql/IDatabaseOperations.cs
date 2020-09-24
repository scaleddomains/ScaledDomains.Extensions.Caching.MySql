using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace ScaledDomains.Extensions.Caching.MySql
{
    public interface IDatabaseOperations
    {
        byte[] GetCacheItem(string key);

        Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default);

        void SetCacheItem(string key, byte[] value, DistributedCacheEntryOptions options);

        Task SetCacheItemAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default);

        void RefreshCacheItem(string key);

        Task RefreshCacheItemAsync(string key, CancellationToken token = default);

        void DeleteCacheItem(string key);

        Task DeleteCacheItemAsync(string key, CancellationToken token = default);

        Task DeleteExpiredCacheItemsAsync(CancellationToken token = default);
    }
}
