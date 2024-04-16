using System;

namespace ScaledDomains.Extensions.Caching.MySql
{
    public class MySqlServerCacheOptions
    {
        /// <summary>
        /// The connection string to the database.
        /// </summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>
        /// Name of the table where the cache items are stored.
        /// </summary>
        public string TableName { get; set; } = default!;

        /// <summary>
        /// The minimum length of time between successive scans for expired items.
        /// By default, it's 20 minutes.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(20);
    }
}
