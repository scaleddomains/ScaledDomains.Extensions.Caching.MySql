using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace ScaledDomains.Extensions.Caching.MySql
{
    public class MySqlServerCacheOptions : IOptions<MySqlServerCacheOptions>
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
        /// By default, it's 20 minutes.
        /// </summary>
        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);

        /// <summary>
        /// The minimum length of time between successive scans for expired items.
        /// By default, it's 20 minutes.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(20);

        public MySqlServerCacheOptions Value { get => this; }

        /// <summary>
        /// For testing purposes
        /// </summary>
        internal ISystemClock SystemClock { get; set; } = new SystemClock();
        
        /// <summary>
        /// For testing purposes
        /// </summary>
        internal IDatabaseOperations DatabaseOperations { get; set; }

        internal MySqlServerCacheOptions Clone()
        {
            return (MySqlServerCacheOptions) MemberwiseClone();
        }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                throw new ArgumentNullException(nameof(this.ConnectionString), $"{nameof(this.ConnectionString)} cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(this.TableName))
            {
                throw new ArgumentNullException(nameof(this.TableName), $"{nameof(this.TableName)} cannot be null or empty.");
            }
        }
    }
}
