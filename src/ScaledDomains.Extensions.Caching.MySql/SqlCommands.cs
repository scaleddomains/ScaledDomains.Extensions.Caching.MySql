namespace ScaledDomains.Extensions.Caching.MySql
{
    internal class SqlCommands
    {
        internal SqlCommands(string schemaName, string tableName)
        {
            var fullName = $"`{schemaName}`.`{tableName}`";

            GetCacheItem = string.Format(UpdateCacheItemFormat + GetCacheItemFormat, fullName);
            SetCacheItem = string.Format(SetCacheItemFormat, fullName);
            RefreshCacheItem = string.Format(UpdateCacheItemFormat, fullName);
            DeleteCacheItem = string.Format(DeleteCacheItemFormat, fullName);
            DeleteExpiredCacheItems = string.Format(DeleteExpiredCacheItemsFormat, fullName);
        }

        private const string GetCacheItemFormat = 
            "SELECT Value FROM {0} WHERE Id = @Id AND ExpiresAt >= @UtcNow; ";

        private const string UpdateCacheItemFormat =
            "UPDATE {0} SET ExpiresAt = (CASE WHEN (SlidingExpiration IS NUll) THEN AbsoluteExpiration ELSE ADDTIME(@UtcNow, SlidingExpiration) END) " +
            "WHERE Id = @Id AND ExpiresAt >= @UtcNow AND SlidingExpiration IS NOT NULL AND (AbsoluteExpiration IS NULL OR AbsoluteExpiration >= ExpiresAt); ";
            
        private const string SetCacheItemFormat =
            "INSERT INTO {0} (Id, Value, ExpiresAt, SlidingExpiration, AbsoluteExpiration) VALUES (@Id, @Value, CASE WHEN (@SlidingExpiration IS NUll) THEN @AbsoluteExpiration ELSE ADDTIME(@UtcNow, @SlidingExpiration) END, @SlidingExpiration, @AbsoluteExpiration) " +
            "ON DUPLICATE KEY UPDATE " +
            "Value = @Value, " +
            "ExpiresAt = (CASE WHEN (@SlidingExpiration IS NUll) THEN @AbsoluteExpiration ELSE ADDTIME(@UtcNow, @SlidingExpiration) END), "+
            "SlidingExpiration = @SlidingExpiration, "+
            "AbsoluteExpiration = @AbsoluteExpiration;";

        private const string DeleteCacheItemFormat = 
            "DELETE FROM {0} WHERE Id = @Id";

        public const string DeleteExpiredCacheItemsFormat = "DELETE FROM {0} WHERE ExpiresAt < @UtcNow";

        internal readonly string GetCacheItem;

        internal readonly string SetCacheItem;

        internal readonly string RefreshCacheItem;

        internal readonly string DeleteCacheItem;

        internal readonly string DeleteExpiredCacheItems;
    }
}
