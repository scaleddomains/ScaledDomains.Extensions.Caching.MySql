using System;

namespace ScaledDomains.Extensions.Caching.MySql;

internal sealed class SystemClock : ISystemClock
{
    /// <summary>
    /// Retrieves the current system time in UTC.
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
