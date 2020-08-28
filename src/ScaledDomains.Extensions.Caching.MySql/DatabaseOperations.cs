using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql
{
    internal sealed class DatabaseOperations : IDatabaseOperations
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
            var utcNow = _systemClock.UtcNow;

            var cmdText = _sqlCommands.GetCache;

            using var connection = new MySqlConnection();
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, 767) { Value = key });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow  });

            connection.Open();

            byte[] result = null;

            using var reader = command.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);

            if (reader.Read())
            {
                result = reader.GetFieldValue<byte[]>(0);
            }

            return result;
        }

        public async Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var utcNow = _systemClock.UtcNow;

            var cmdText = _sqlCommands.GetCache;

            using var connection = new MySqlConnection();
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, 767) { Value = key });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow  });

            await connection.OpenAsync(token);

            byte[] result = null;

            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow | CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, token);

            if (await reader.ReadAsync(token))
            {
                result = await reader.GetFieldValueAsync<byte[]>(0, token);
            }

            return result;
        }

        public void SetCacheItem(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var utcNow = _systemClock.UtcNow;
            
            var absoluteExpiration = GetAbsoluteExpiration(utcNow, options);
            ValidateOptions(options.SlidingExpiration, absoluteExpiration);
            
            var cmdText = _sqlCommands.SetCache;

            using var connection = new MySqlConnection();
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, 767) { Value = key });
            command.Parameters.Add(new MySqlParameter("@Value", MySqlDbType.Blob) { Value = value });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow  });
            command.Parameters.Add(new MySqlParameter("@SlidingExpiration", MySqlDbType.Time) { Value = (object)options.SlidingExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@AbsoluteExpiration", MySqlDbType.Timestamp) { Value = (object)absoluteExpiration?.UtcDateTime ?? DBNull.Value });

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException ex) when (!IsDuplicateKeyException(ex))
            {

            }
        }

        public async Task SetCacheItemAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var utcNow = _systemClock.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(utcNow, options);
            ValidateOptions(options.SlidingExpiration, absoluteExpiration);

            var cmdText = _sqlCommands.SetCache;

            using var connection = new MySqlConnection();
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, 767) { Value = key });
            command.Parameters.Add(new MySqlParameter("@Value", MySqlDbType.Blob) { Value = value });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow  });
            command.Parameters.Add(new MySqlParameter("@SlidingExpiration", MySqlDbType.Time) { Value = (object)options.SlidingExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@AbsoluteExpiration", MySqlDbType.Timestamp) { Value = (object)absoluteExpiration?.UtcDateTime ?? DBNull.Value });

            await connection.OpenAsync(token).ConfigureAwait(false);

            try
            {
                await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
            catch (MySqlException ex) when (!IsDuplicateKeyException(ex))
            {
                
            }
        }

        private bool IsDuplicateKeyException(MySqlException ex) => ex.ErrorCode == (int)MySqlErrorCode.DuplicateKey; //TODO: must be tested

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset utcNow, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                return utcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }

            if (options.AbsoluteExpiration.HasValue)
            {
                if (options.AbsoluteExpiration.Value <= utcNow)
                {
                    throw new InvalidOperationException("The absolute expiration value must be in the future.");
                }

                return options.AbsoluteExpiration.Value;
            }

            return null;
        }

        private void ValidateOptions(TimeSpan? slidingExpiration, DateTimeOffset? absoluteExpiration)
        {
            if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue)
            {
                throw new InvalidOperationException("Either absolute or sliding expiration must be provided.");
            }
        }
    }
}
