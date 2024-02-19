using System;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ScaledDomains.Extensions.Caching.MySql.Tests
{
    [TestClass]
    public class MySqlServerCachingServicesExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDistributedSqlServerCache_WithNullServicesCollection_ShouldThrowArgumentNullException()
        {
            MySqlServerCachingServicesExtensions.AddDistributedMySqlServerCache(null, options => {
                options.ConnectionString = "Server=example.com;Database=db;User=root";
                options.TableName = "MyTable";
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDistributedSqlServerCache_WithNullOptionAction_ShouldThrowArgumentNullException()
        {
            new ServiceCollection().AddDistributedMySqlServerCache(null);
        }

        [TestMethod]
        public void AddDistributedSqlServerCache_AddsAsSingleRegistrationService()
        {
            // Arrange

            var services = new ServiceCollection();

            // Act

            services.AddDistributedMySqlServerCache(options => {
                options.ConnectionString = "Server=example.com;Database=db;User=root";
                options.TableName = "MyTable";
            });

            // Assert

            var cacheServiceDescriptor = services.SingleOrDefault(i => i.ServiceType == typeof(IDistributedCache));

            Assert.IsNotNull(cacheServiceDescriptor);
            Assert.AreEqual(typeof(MySqlServerCache), cacheServiceDescriptor.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, cacheServiceDescriptor.Lifetime);

            var maintenanceServiceDescriptor = services.SingleOrDefault(i => i.ServiceType == typeof(IHostedService));

            Assert.IsNotNull(maintenanceServiceDescriptor);
            Assert.AreEqual(typeof(MySqlServerCacheMaintenanceService), maintenanceServiceDescriptor.ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, maintenanceServiceDescriptor.Lifetime);
        }

        [TestMethod]
        public void AddDistributedSqlServerCache_ReplacesPreviouslyUserRegisteredServices()
        {
            // Arrange

            var services = new ServiceCollection();
            services.AddScoped(typeof(IDistributedCache), sp => Mock.Of<IDistributedCache>());

            // Act

            services.AddDistributedMySqlServerCache(options => {
                options.ConnectionString = "Server=example.com;Database=db;User=root";
                options.TableName = "MyTable";
            });

            // Assert

            var serviceProvider = services.BuildServiceProvider();

            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));

            Assert.IsNotNull(distributedCache);
            Assert.AreEqual(ServiceLifetime.Scoped, distributedCache.Lifetime);
            Assert.IsInstanceOfType(serviceProvider.GetRequiredService<IDistributedCache>(), typeof(MySqlServerCache));
        }
    }
}
