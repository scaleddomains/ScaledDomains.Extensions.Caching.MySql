
# MySQL Server Distributed Cache Command Line Tool

## About

MySQL Server Distributed Cache Command Line Tool is a [.NET Core Global Tool](https://aka.ms/global-tools) to create datatable for [ScaledDomains.Extensions.Caching.MySql](https://github.com/scaleddomains/ScaledDomains.Extensions.Caching.MySql) cache implementation. Founded and maintained by [Endre Toth](https://github.com/endret) and [Attila Ersek](https://github.com/attilaersek).

## Getting started

The Distributed MySQL Server Cache implementation allows the distributed cache to use a MySQL Server database as its caching store. To create a table in a database instance, you can use the `mysql-distributed-cache` tool. The tool creates a table the name that you specify.

### Create command

Create table and indexes in a MySQL Server database for distributed caching.

The following example creates table with `myDistributedCache` name on MySQL server (`example.com`) under the `pizzaordersystem-db` schema.

Arguments:
1. [connectionString] - The mysql connection string to connect to the database.
2. [tableName] - Name of the table to be created.

Options:
*  -f | --force - Force to create the table (if the table is already exist will remove it)

``` shell
mysql-distributed-cache Server=example.com;Database=pizzaordersystem-db;User=dbAdmin; myDistributedCache 
```
> The connection string may contain credentials that should be kept out of source control systems.

The table has the following schema:

``` sql
describe `pizzaordersystem-db`.`myDistributedCache`;
```
| Field               | Type          |Null | Key | Default | Extra
| -- | -- | -- | -- | -- | -- |
| Id | varchar(767) |NO|PRI|NULL| |
| AbsoluteExpiration | datetime(6)  | YES || NULL | |
| ExpiresAt          | datetime(6)  | NO | MUL | NULL | |
| SlidingExpiration  | time(6)       | YES || NULL | |
| Value              | longblob      | NO || NULL | |

## Bug reports and feature requests

Please use the [Issue Tracker](https://github.com/scaleddomains/ScaledDomains.Extensions.Caching.MySql/issues).
