---
searchHints:
  - oracle
  - database
  - sql
  - enterprise
  - relational
  - db
---

# Oracle Database Provider

<Ingress>
Connect your Ivy application to Oracle Database with automatic Entity Framework configuration.
</Ingress>

## Overview

Oracle Database is a multi-model database management system developed by Oracle Corporation. It's widely used in enterprise environments and offers advanced features for mission-critical applications.

## Adding a Database Connection

To set up Oracle Database with Ivy, run the following command and choose `Oracle` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for a connection string. The connection string you provide should follow this format:

```text
Data Source=localhost:1521/FREEPDB1; User Id=user; Password=password;
```

Specifically, your connection string should contain the following information, in the form of semicolon-separated key-value pairs:

- **Data Source**: Database to connect to, specified by either an easy connect name (as shown above), a connect descriptor, or an Oracle net services name.
- **User Id** and **Password**: The credentials used to authenticate to the server.

For all connection options, see the [Oracle documentation](https://docs.oracle.com/en/database/oracle/oracle-database/19/odpnt/ConnectionConnectionString.html#GUID-DF4ED9A3-1AAF-445D-AEEF-016E6CD5A0C0__BABBAGJJ).

Your connection string will be stored in .NET user secrets.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **Oracle.EntityFrameworkCore** package for Oracle connections.

## Oracle-Specific Features

Key enterprise features Ivy can leverage:
- **Advanced Security** - encryption and access controls
- **Performance** - partitioning and indexing
- **PL/SQL** - stored procedures and functions

For complete features, see the [Oracle Database documentation](https://docs.oracle.com/en/database/oracle/oracle-database/index.html).

## Security Best Practices

- **Use encrypted connections** with SSL/TLS
- **Create dedicated database users** with minimal required privileges
- **Enable auditing** for sensitive operations
- **Use Oracle Advanced Security** features in production
- **Implement connection pooling** to optimize resource usage

## Troubleshooting

### Common Issues

**Authentication Failed**
- Check username/password and account status

For detailed troubleshooting, refer to [Oracle Database Error Messages](https://docs.oracle.com/en/database/oracle/oracle-database/19/errmg/index.html).

## Related Documentation

- [Database Overview](Overview.md)
- [SQL Server Provider](SqlServer.md)
- [PostgreSQL Provider](PostgreSql.md)
- [Enterprise Features](../../02_Concepts/Services.md)
- [Official Oracle Database Documentation](https://docs.oracle.com/en/database/oracle/oracle-database/index.html)
- [Oracle.EntityFrameworkCore Package](https://docs.oracle.com/en/database/oracle/oracle-data-access-components/19.3/odpnt/ODPEFCore.html)
