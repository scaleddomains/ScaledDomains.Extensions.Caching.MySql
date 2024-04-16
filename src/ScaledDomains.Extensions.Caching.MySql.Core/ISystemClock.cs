using System;

namespace ScaledDomains.Extensions.Caching.MySql;

public interface ISystemClock
{
    
    /// <summary>
    /// Retrieves the current system time in UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
