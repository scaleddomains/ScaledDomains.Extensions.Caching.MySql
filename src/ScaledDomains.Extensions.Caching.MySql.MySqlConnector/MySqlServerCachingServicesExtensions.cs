using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ScaledDomains.Extensions.Caching.MySql
{
    /// <summary>
    /// Extension methods for setting up MySQL Server distributed cache services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class MySqlServerCachingServicesExtensions
    {
        /// <summary>
        /// Adds MySQL Server distributed caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{SqlServerCacheOptions}"/> to configure the provided <see cref="MySqlServerCacheOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDistributedMySqlServerCache(this IServiceCollection services, Action<MySqlServerCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }
            
            services.AddOptions();
            AddDistributedMySqlServerCache(services);
            services.Configure(setupAction);

            return services;
        }

        /// <summary>
        /// Adds MySQL Server distributed caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDistributedMySqlServerCache(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<MySqlServerCacheOptions>, ValidateMySqlServerCacheOptions>());
            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.TryAddSingleton<IDatabaseOperations, DatabaseOperations>();
            services.AddSingleton<IDistributedCache, MySqlServerCache>();

            services.AddHostedService<MySqlServerCacheMaintenanceService>();
            
            return services;
        }
    }
}
