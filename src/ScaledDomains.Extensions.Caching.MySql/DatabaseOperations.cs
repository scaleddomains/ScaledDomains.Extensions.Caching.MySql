using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql
{
    internal sealed class DatabaseOperations : IDatabaseOperations
    {
        internal const int IdColumnSize = 767;

        private readonly SqlCommands _sqlCommands;
        private readonly ISystemClock _systemClock;
        private readonly string _connectionString;

        public DatabaseOperations(IOptions<MySqlServerCacheOptions> options, ISystemClock systemClock)
        {
            if (options is null || options.Value is null) 
            {
                throw new ArgumentNullException(nameof(options));
            }

            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

            _connectionString = options.Value.ConnectionString;
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);

            _sqlCommands = new SqlCommands(connectionStringBuilder.Database, options.Value.TableName);
        }

        public byte[]? GetCacheItem(string key)
        {
            var utcNow = _systemClock.UtcNow;

            var cmdText = _sqlCommands.GetCacheItem;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow.UtcDateTime });

            connection.Open();

            byte[]? result = null;

            using var reader = command.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);

            if (reader.Read())
            {
                result = reader.GetFieldValue<byte[]>(0);
            }

            return result;
        }

        public async Task<byte[]?> GetCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var utcNow = _systemClock.UtcNow;

            var cmdText = _sqlCommands.GetCacheItem;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow.UtcDateTime });

            await connection.OpenAsync(token).ConfigureAwait(false);

            byte[]? result = null;

            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow | CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, token).ConfigureAwait(false);

            if (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                result = await reader.GetFieldValueAsync<byte[]>(0, token).ConfigureAwait(false);
            }

            return result;
        }

        public void SetCacheItem(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var utcNow = _systemClock.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(utcNow, options);
            ValidateOptions(options.SlidingExpiration, absoluteExpiration);

            var cmdText = _sqlCommands.SetCacheItem;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);
            
            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });
            command.Parameters.Add(new MySqlParameter("@Value", MySqlDbType.Blob) { Value = value });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow.UtcDateTime });
            command.Parameters.Add(new MySqlParameter("@SlidingExpiration", MySqlDbType.Time) { Value = (object?)options.SlidingExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@AbsoluteExpiration", MySqlDbType.Timestamp) { Value = (object?)absoluteExpiration?.UtcDateTime ?? DBNull.Value });

            connection.Open();

            command.ExecuteNonQuery();
        }

        public async Task SetCacheItemAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var utcNow = _systemClock.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(utcNow, options);
            ValidateOptions(options.SlidingExpiration, absoluteExpiration);

            var cmdText = _sqlCommands.SetCacheItem;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });
            command.Parameters.Add(new MySqlParameter("@Value", MySqlDbType.Blob) { Value = value });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow.UtcDateTime });
            command.Parameters.Add(new MySqlParameter("@SlidingExpiration", MySqlDbType.Time) { Value = (object?)options.SlidingExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@AbsoluteExpiration", MySqlDbType.Timestamp) { Value = (object?)absoluteExpiration?.UtcDateTime ?? DBNull.Value });

            await connection.OpenAsync(token).ConfigureAwait(false);

            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

        public void RefreshCacheItem(string key)
        {
            var cmdText = _sqlCommands.RefreshCacheItem;
            var utcNow = _systemClock.UtcNow;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow.UtcDateTime });

            connection.Open();

            command.ExecuteNonQuery();
        }

        public async Task RefreshCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var cmdText = _sqlCommands.RefreshCacheItem;
            var utcNow = _systemClock.UtcNow;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });
            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow.UtcDateTime });

            await connection.OpenAsync(token).ConfigureAwait(false);

            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

        public void DeleteCacheItem(string key)
        {
            var cmdText = _sqlCommands.DeleteCacheItem;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });

            connection.Open();

            command.ExecuteNonQuery();
        }

        public async Task DeleteCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var cmdText = _sqlCommands.DeleteCacheItem;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, IdColumnSize) { Value = key });

            await connection.OpenAsync(token).ConfigureAwait(false);

            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

        public async Task DeleteExpiredCacheItemsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var utcNow = _systemClock.UtcNow;

            var cmdText = _sqlCommands.DeleteExpiredCacheItems;

            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(cmdText, connection);

            command.Parameters.Add(new MySqlParameter("@UtcNow", MySqlDbType.Timestamp) { Value = utcNow.UtcDateTime });

            await connection.OpenAsync(token).ConfigureAwait(false);

            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

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

        private static void ValidateOptions(TimeSpan? slidingExpiration, DateTimeOffset? absoluteExpiration)
        {
            if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue)
            {
                throw new InvalidOperationException("Either absolute or sliding expiration must be provided.");
            }
        }
    }
}
