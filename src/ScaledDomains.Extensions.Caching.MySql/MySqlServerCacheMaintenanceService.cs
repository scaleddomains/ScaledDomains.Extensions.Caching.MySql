using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ScaledDomains.Extensions.Caching.MySql
{
    public class MySqlServerCacheMaintenanceService : BackgroundService
    {
        private readonly IDatabaseOperations _databaseOperations;
        private readonly TimeSpan _frequency;
        private static readonly Random Random = new Random();

        public MySqlServerCacheMaintenanceService(IOptions<MySqlServerCacheOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null.");
            }

            options.Value.Validate();

            _databaseOperations = options.Value.DatabaseOperations ?? new DatabaseOperations(options.Value);
            _frequency = options.Value.ExpirationScanFrequency;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _databaseOperations.DeleteExpiredCacheItemsAsync(stoppingToken);

                var rnd = Random.Next(5000);
                var delay = _frequency.Add(TimeSpan.FromMilliseconds(rnd));

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
