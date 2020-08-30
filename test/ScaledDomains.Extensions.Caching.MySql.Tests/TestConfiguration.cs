using Microsoft.Extensions.Configuration;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    internal static class TestConfiguration
    {
        public static MySqlServerCacheOptions MySqlServerCacheOptions { get; }

        static TestConfiguration()
        {
            MySqlServerCacheOptions = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", false)
                .Build()
                .GetSection("ScaledDomains.Extensions.Caching.MySql")
                .Get<MySqlServerCacheOptions>();
        }
    }
}
