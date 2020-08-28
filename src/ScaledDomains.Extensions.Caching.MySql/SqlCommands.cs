namespace ScaledDomains.Extensions.Caching.MySql
{
    internal class SqlCommands
    {
        internal SqlCommands(string schemaName, string tableName)
        {
            var fullName = $"`{schemaName}`.`{tableName}`";

            GetCache = string.Format(GetCacheFormat, fullName); // TODO update expiration
        }

        private const string GetCacheFormat = "SELECT Value FROM {0} WHERE Id = @Id AND ExpiresAt >= @ExpiresAt;";

        internal readonly string GetCache;
    }
}
