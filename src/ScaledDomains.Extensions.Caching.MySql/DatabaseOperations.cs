using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql
{
    internal class DatabaseOperations : IDatabaseOperations
    {
        private readonly SqlCommands _sqlCommands;
        private readonly ISystemClock _systemClock;

        public DatabaseOperations(MySqlServerCacheOptions options)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(options.ConnectionString);

            _sqlCommands = new SqlCommands(connectionStringBuilder.Database, options.TableName);
            _systemClock = options.SystemClock;
        }

        public byte[] GetCacheItem(string key)
        {
            return GetCacheItemAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var cmdText = _sqlCommands.GetCache;

            using (var connection = new MySqlConnection())
            using (var command = new MySqlCommand(cmdText, connection))
            {
                command.Parameters.AddWithValue("@Id", key);
                command.Parameters.AddWithValue("@ExpiresAt", _systemClock.UtcNow);

                await connection.OpenAsync(token);

                byte[] result = null;

                using (var reader = await command.ExecuteReaderAsync(
                    CommandBehavior.SingleRow | CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, token))
                {
                    if (await reader.ReadAsync(token))
                    {
                        result = await reader.GetFieldValueAsync<byte[]>(0, token);
                    }

                    return result;
                }
            }
        }
    }
}
