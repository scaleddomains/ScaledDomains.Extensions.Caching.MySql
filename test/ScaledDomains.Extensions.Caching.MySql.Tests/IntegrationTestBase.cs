using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    public class IntegrationTestBase
    {
        protected readonly Mock<IOptions<MySqlServerCacheOptions>> _optionsMock = new Mock<IOptions<MySqlServerCacheOptions>>();
        protected readonly Mock<ILogger<MySqlServerCacheMaintenanceService>> _loggerMock = new Mock<ILogger<MySqlServerCacheMaintenanceService>>();
        protected readonly Mock<IDatabaseOperations> _databaseOperationsMock = new Mock<IDatabaseOperations>();

        protected CacheItem GetCacheItem(string connectionString, string tableName, string key)
        {
            CacheItem result = null;

            var cmdTextBuilder = new StringBuilder();
            cmdTextBuilder.Append("SELECT Id, AbsoluteExpiration, ExpiresAt, SlidingExpiration, Value ");
            cmdTextBuilder.AppendFormat("FROM {0} ", tableName);
            cmdTextBuilder.Append("WHERE Id = @Id;");

            using var connection = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(cmdTextBuilder.ToString(), connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString) {Value = key});

            connection.Open();

            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                result = new CacheItem
                {
                    Id = reader.GetString(0),
                    AbsoluteExpiration = reader.IsDBNull(1) ? (DateTime?)null : new DateTime(reader.GetDateTime(1).Ticks, DateTimeKind.Utc),
                    ExpiresAt = new DateTime(reader.GetDateTime(2).Ticks, DateTimeKind.Utc),
                    SlidingExpiration = reader.IsDBNull(3) ? (TimeSpan?)null : reader.GetTimeSpan(3),
                    Value = reader.GetFieldValue<byte[]>(4)
                };
            }

            return result;
        }

        protected int CreateCacheItem(string connectionString, string tableName, CacheItem cacheItem)
        {
            var cmdTextBuilder = new StringBuilder();
            cmdTextBuilder.AppendFormat("INSERT INTO {0} ", tableName);
            cmdTextBuilder.Append("(Id, AbsoluteExpiration, ExpiresAt, SlidingExpiration, Value) ");
            cmdTextBuilder.Append("VALUES");
            cmdTextBuilder.Append("(@Id, @AbsoluteExpiration, @ExpiresAt, @SlidingExpiration, @Value);");

            using var connection = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(cmdTextBuilder.ToString(), connection);

            command.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.VarString, 767) { Value = cacheItem.Id });
            command.Parameters.Add(new MySqlParameter("@AbsoluteExpiration", MySqlDbType.Timestamp) { Value = (object)cacheItem.AbsoluteExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@ExpiresAt", MySqlDbType.Timestamp) { Value = cacheItem.ExpiresAt });
            command.Parameters.Add(new MySqlParameter("@SlidingExpiration", MySqlDbType.Time) { Value = (object)cacheItem.SlidingExpiration ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@Value", MySqlDbType.Blob) { Value = cacheItem.Value });

            connection.Open();

            return command.ExecuteNonQuery();
        }

        protected void ClearCache(string connectionString, string tableName)
        {
            var cmdText = $"TRUNCATE TABLE {tableName}";

            using var connection = new MySqlConnection(connectionString);

            using var command = new MySqlCommand(cmdText, connection);

            connection.Open();

            command.ExecuteNonQuery();
        }
    }
}
