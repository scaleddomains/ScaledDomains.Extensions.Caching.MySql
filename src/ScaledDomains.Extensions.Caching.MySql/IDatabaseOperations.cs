using System.Threading;
using System.Threading.Tasks;

namespace ScaledDomains.Extensions.Caching.MySql
{
    internal interface IDatabaseOperations
    {
        byte[] GetCacheItem(string key);

        Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default);
    }
}
