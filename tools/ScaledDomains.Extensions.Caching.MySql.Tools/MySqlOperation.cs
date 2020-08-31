using System;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql.Tools
{
    internal static class MySqlOperation
    {
        public static async Task CreateTable(string connectionString, string tableName, bool dropTableIfAlreadyExists, CancellationToken cancellationToken = default)
        {
            var commandText = dropTableIfAlreadyExists
                ? SqlCommands.DropTable(connectionString, tableName) +
              SqlCommands.CreateTable(connectionString, tableName)
            : SqlCommands.CreateTable(connectionString, tableName);

            await using var connection = new MySqlConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command = new MySqlCommand(commandText, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
