using System;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace ScaledDomains.Extensions.Caching.MySql
{
    public class MySqlServerCacheOptions
    {
        /// <summary>
        /// The connection string to the database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Name of the table where the cache items are stored.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The default sliding expiration set for a cache entry if neither Absolute or SlidingExpiration has been set explicitly.
        /// By default, its 20 minutes.
        /// </summary>
        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);

        /// <summary>
        /// For testing purposes
        /// </summary>
        internal ISystemClock SystemClock { get; set; } = new SystemClock();
    }
}
