using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScaledDomains.Extensions.Caching.MySql.Tests;

[TestClass]
[TestCategory(TestCategoryNames.Unit)]
public class SystemClockTests
{
        [TestMethod]
        public void SystemClock_Works()
        {
            Assert.IsTrue(DateTimeOffset.UtcNow - new SystemClock().UtcNow < TimeSpan.FromSeconds(1));
        }
}
