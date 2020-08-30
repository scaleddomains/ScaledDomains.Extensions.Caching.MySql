using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    public class IntegrationTestBase
    {
        private static readonly string TableName = TestConfiguration.MySqlServerCacheOptions.TableName;
        private static readonly string ConnectionString = TestConfiguration.MySqlServerCacheOptions.ConnectionString;

        protected static IDistributedCache CreateMySqlServerCache(ISystemClock clock = null)
        {
            if (clock != null)
            {
                var options = TestConfiguration.MySqlServerCacheOptions.Clone();
                options.SystemClock = clock;

                return new MySqlServerCache(options);
            }

            return new MySqlServerCache(TestConfiguration.MySqlServerCacheOptions);
        }

        protected CacheItem GetCacheItem(string key)
        {
            CacheItem result = null;

            var cmdTextBuilder = new StringBuilder();
            cmdTextBuilder.Append("SELECT Id, AbsoluteExpiration, ExpiresAt, SlidingExpiration, Value ");
            cmdTextBuilder.AppendFormat("FROM {0} ", TableName);
            cmdTextBuilder.Append("WHERE Id = @Id;");

            using var connection = new MySqlConnection(ConnectionString);
            using var command = new MySqlCommand(cmdTextBuilder.ToString(), connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString) {Value = key});

            connection.Open();

            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                result = new CacheItem
                {
                    Id = reader.GetString(0),
                    AbsoluteExpiration = reader.IsDBNull(1) ? null : (DateTime?)new DateTime(reader.GetFieldValue<DateTime?>(1).Value.Ticks, DateTimeKind.Utc),
                    ExpiresAt = new DateTime(reader.GetDateTime(2).Ticks, DateTimeKind.Utc),
                    SlidingExpiration = reader.IsDBNull(3) ? null : (TimeSpan?) reader.GetTimeSpan(3),
                    Value = reader.GetFieldValue<byte[]>(4)
                };
            }

            return result;
        }

        protected int CreateCacheItem(CacheItem cacheItem)
        {
            var cmdTextBuilder = new StringBuilder();
            cmdTextBuilder.AppendFormat("INSERT INTO {0} ", TableName);
            cmdTextBuilder.Append("(Id, AbsoluteExpiration, ExpiresAt, SlidingExpiration, Value) ");
            cmdTextBuilder.Append("VALUES");
            cmdTextBuilder.Append("(@Id, @AbsoluteExpiration, @ExpiresAt, @SlidingExpiration, @Value);");

            using var connection = new MySqlConnection(ConnectionString);
            using var command = new MySqlCommand(cmdTextBuilder.ToString(), connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, 767) { Value = cacheItem.Id });
            command.Parameters.Add(new MySqlParameter("@AbsoluteExpiration", MySqlDbType.Timestamp) { Value = (object)cacheItem.AbsoluteExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@ExpiresAt", MySqlDbType.Timestamp) { Value = cacheItem.ExpiresAt });
            command.Parameters.Add(new MySqlParameter("@SlidingExpiration", MySqlDbType.Time) { Value = (object)cacheItem.SlidingExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@Value", MySqlDbType.Blob) { Value = cacheItem.Value });

            connection.Open();

            return command.ExecuteNonQuery();
        }

        protected void ClearCache()
        {
            var cmdText = $"TRUNCATE TABLE {TableName}";

            using var connection = new MySqlConnection(ConnectionString);

            using var command = new MySqlCommand(cmdText, connection);

            connection.Open();

            command.ExecuteNonQuery();
        }
    }
}
