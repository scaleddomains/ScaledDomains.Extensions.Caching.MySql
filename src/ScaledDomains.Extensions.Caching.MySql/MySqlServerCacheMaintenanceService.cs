using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ScaledDomains.Extensions.Caching.MySql
{
    public class MySqlServerCacheMaintenanceService : BackgroundService
    {
        private static readonly Random Random = new Random();
        private readonly ILogger<MySqlServerCacheMaintenanceService> _logger;
        private readonly IDatabaseOperations _databaseOperations;
        private readonly TimeSpan _expirationScanFrequency;

        public MySqlServerCacheMaintenanceService(IOptions<MySqlServerCacheOptions> options, ILogger<MySqlServerCacheMaintenanceService> logger, IDatabaseOperations databaseOperations)
        {
            _expirationScanFrequency = options?.Value?.ExpirationScanFrequency ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseOperations = databaseOperations ?? throw new ArgumentNullException(nameof(databaseOperations));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _databaseOperations.DeleteExpiredCacheItemsAsync(stoppingToken);
                }
                catch(TaskCanceledException)
                {
                }
                catch(Exception exception)
                {
                    _logger.LogError(exception, "Cache maintenance operation falied.");
                }

                var rnd = Random.Next(5000);
                var delay = _expirationScanFrequency.Add(TimeSpan.FromMilliseconds(rnd));

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
