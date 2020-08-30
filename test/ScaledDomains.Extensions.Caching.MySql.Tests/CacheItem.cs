using System;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    public class CacheItem
    {
        public string Id { get; set; }

        public DateTime? AbsoluteExpiration { get; set; }

        public DateTime ExpiresAt { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public byte[] Value { get; set; }
    }
}
