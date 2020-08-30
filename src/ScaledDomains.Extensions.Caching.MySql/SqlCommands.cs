namespace ScaledDomains.Extensions.Caching.MySql
{
    internal class SqlCommands
    {
        internal SqlCommands(string schemaName, string tableName)
        {
            var fullName = $"`{schemaName}`.`{tableName}`";

            GetCache = string.Format(UpdateCacheFormat + GetCacheFormat, fullName);
            SetCache = string.Format(SetCacheFormat, fullName);
            RefreshCache = string.Format(UpdateCacheFormat, fullName);
        }

        private const string GetCacheFormat = "SELECT Value FROM {0} WHERE Id = @Id AND ExpiresAt >= @UtcNow; ";

        private const string UpdateCacheFormat =
            "UPDATE {0} SET ExpiresAt = (CASE WHEN (SlidingExpiration IS NUll) THEN AbsoluteExpiration ELSE ADDTIME(@UtcNow, SlidingExpiration) END) " +
            "WHERE Id = @Id AND ExpiresAt >= @UtcNow AND SlidingExpiration IS NOT NULL AND (AbsoluteExpiration IS NULL OR AbsoluteExpiration >= ExpiresAt); ";
            
        private const string SetCacheFormat = 
            "INSERT INTO {0} (Id, Value, ExpiresAt, SlidingExpiration, AbsoluteExpiration) VALUES (@Id, @Value, @ExpiresAt, @SlidingExpiration, @AbsoluteExpiration) " +
            "ON DUPLICATE KEY UPDATE " +
            "Value = @Value, " +
            "ExpiresAt = (CASE WHEN (@SlidingExpiration IS NUll) THEN @AbsoluteExpiration ELSE ADDTIME(@UtcNow, @SlidingExpiration)), "+
            "SlidingExpiration = @SlidingExpiration, "+
            "AbsoluteExpiration = @AbsoluteExpiration;";

        internal readonly string GetCache;

        internal readonly string SetCache;

        internal readonly string RefreshCache;
    }
}
