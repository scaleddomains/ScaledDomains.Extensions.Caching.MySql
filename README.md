# ScaledDomains.Extensions.Caching.MySql

## About

ScaledDomains.Extensions.Caching.MySql is a free, open source distributed cache implementation using MySql as datastore, inspired by [Microsoft.Extensions.Cachning.SqlServer](https://www.nuget.org/packages/Microsoft.Extensions.Caching.SqlServer). Founded and maintained by [Endre Toth](https://github.com/endret) and [Attila Ersek](https://github.com/attilaersek).

![Build](https://github.com/scaleddomains/ScaledDomains.Extensions.Caching.MySql/workflows/build-master-and-publish/badge.svg?branch=master)
![License](https://img.shields.io/github/license/scaleddomains/ScaledDomains.Extensions.Caching.MySql)
[![codecov](https://codecov.io/gh/scaleddomains/ScaledDomains.Extensions.Caching.MySql/branch/master/graph/badge.svg)](https://codecov.io/gh/scaleddomains/ScaledDomains.Extensions.Caching.MySql)

## Getting started

The Distributed MySQL Server Cache implementation allows the distributed cache to use a MySQL Server database as its caching store. To create a table in a database instance, you can use the `mysql-distributed-cache` tool. The tool creates a table the name that you specify.

### 1. Create table

#### 1.1 Requirements

* [`mysql-distributed-cache`](https://github.com/scaleddomains/ScaledDomains.Extensions.Caching.MySql/blob/master/tools/README.md) .NET CLI Tool

The following example creates table with `myDistributedCache` name on MySQL server (`example.com`) under the `pizzaordersystem-db` schema.

The CLI tool parameters:
1. [connectionString] - The mysql connection string to connect to the database.
2. [tableName] - Name of the table to be created.
``` shell
mysql-distributed-cache Server=example.com;Database=pizzaordersystem-db;User=dbAdmin; myDistributedCache 
```
> The connection string may contain credentials that should be kept out of source control systems.

The table has the following schema:

``` sql
describe `pizzaordersystem-db`.`myDistributedCache`;
```
| Field               | Type          |Null | Key | Default | Extra
|--|--|--|--|--|--|
| Id | varchar(767) |NO|PRI|NULL| |
| AbsoluteExpiration | datetime(6)  |YES||NULL| |
| ExpiresAt          | datetime(6)  |NO|MUL|NULL| |
| SlidingExpiration  | time(6)       |YES  ||NULL| |
| Value              | longblob      |NO||NULL| |

### 2. Setup your application

The package manipulates cache values using an instance of [IDistributedCache](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache).

``` csharp
services.AddDistributedMySqlServerCache(options => {
                options.ConnectionString = _config["DistributedCache_ConnectionString"];
                options.TableName = "myDistributedCache";
});
```

#### 2.1 Options

| Property | Type | Description | Default | Required/Optional
|--|--|--|--|--|
| `ConnectionString` | string | The connection string to the database. || REQUIRED
| `TableName ` | string | Name of the table where the cache items are stored. || REQUIRED
| `ExpirationScanFrequency` | TimeSpan | The minimum length of time between successive scans for expired items. |00:20:00| OPTIONAL

### Usage

The  [IDistributedCache](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache)  interface provides basic methods to manage items, in the distributed cache implementation:

-   [Get](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.get),  [GetAsync](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.getasync): Accepts a string key and retrieves a cached item as a  `byte[]`  array if found in the cache.
-   [Set](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.set),  [SetAsync](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.setasync): Adds an item (as  `byte[]`  array) to the cache using a string key.
-   [Refresh](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.refresh),  [RefreshAsync](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.refreshasync): Refreshes an item in the cache based on its key, resetting its sliding expiration timeout (if any).
-   [Remove](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.remove),  [RemoveAsync](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.removeasync): Removes a cache item based on its string key.

### Limits

* Cache key length must be less than **767** and encoding should be **ASCII**.
* The maximum size of the cache item is **4Gb**.

## Bug reports and feature requests

Please use the [Issue Tracker](https://github.com/scaleddomains/ScaledDomains.Extensions.Caching.MySql/issues).
