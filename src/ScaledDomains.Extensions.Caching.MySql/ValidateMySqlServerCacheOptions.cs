using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace ScaledDomains.Extensions.Caching.MySql
{
    public class ValidateMySqlServerCacheOptions : IValidateOptions<MySqlServerCacheOptions>
    {
        public ValidateOptionsResult Validate(string? name, MySqlServerCacheOptions options)
        {
            var failures = new List<string>();
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                failures.Add("{nameof(options.ConnectionString)} cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(options.TableName))
            {
               failures.Add("{nameof(options.TableName)} cannot be null or empty.");
            }

            return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
        }
    }
}
