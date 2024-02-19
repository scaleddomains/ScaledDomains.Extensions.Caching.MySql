using System;
using System.IO;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace ScaledDomains.Extensions.Caching.MySql.Tools
{
    internal static class SqlCommands
    {
        internal static string CreateTable(string connectionString, string tableName)
        {
            var csBuilder = new MySqlConnectionStringBuilder(connectionString);

            var fullName = $"`{csBuilder.Database}`.`{tableName}`";

           return string.Format(CreateTableFormat, fullName);
        }

        internal static string DropTable(string connectionString, string tableName)
        {
            var csBuilder = new MySqlConnectionStringBuilder(connectionString);

            var fullName = $"`{csBuilder.Database}`.`{tableName}`";

            return string.Format(DropTableFormat, fullName);
        }

        private const string DropTableFormat = "DROP TABLE IF EXISTS {0}; ";

        private const string CreateTableFormat = "CREATE TABLE {0} (" +
                                                 "`Id` varchar(767) CHARACTER SET ascii COLLATE ascii_bin NOT NULL," +
                                                 "`AbsoluteExpiration` datetime(6) DEFAULT NULL," +
                                                 "`ExpiresAt` datetime(6) NOT NULL," +
                                                 "`SlidingExpiration` time(6) DEFAULT NULL," +
                                                 "`Value` longblob NOT NULL," +
                                                 "PRIMARY KEY(`Id`)," +
                                                 "KEY `Index_ExpiresAt` (`ExpiresAt`) ); ";
    }
}
