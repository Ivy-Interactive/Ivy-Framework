---
title: SQL Server
searchHints:
  - sqlserver
  - mssql
  - database
  - microsoft
  - sql
  - db
---

# SQL Server Database Provider

<Ingress>
Connect your Ivy application to Microsoft SQL Server with automatic Entity Framework configuration.
</Ingress>

## Overview

SQL Server is Microsoft's enterprise-grade relational database management system. Ivy provides seamless integration with SQL Server through Entity Framework Core.

## Adding a Database Connection

To set up SQL Server with Ivy, run the following command and choose `SqlServer` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for a connection string. The connection string you provide should follow one of the following formats, depending on your authentication mode:

> For details on authentication modes, see Microsoft's [Choose an authentication mode](https://learn.microsoft.com/en-us/sql/relational-databases/security/choose-an-authentication-mode).

**Windows Authentication (Recommended)**
```text
Server=localhost; Database=my_db; Trusted_Connection=True;
```

**SQL Server Authentication**
```text
Server=localhost; Database=my_db; User Id=user; Password=password;
```

Specifically, your connection string should contain the following information, in the form of semicolon-separated key-value pairs:

- **Server**: The hostname of your SQL Server instance.
- **Database**: The name of the database you wish to connect to.
- One of the following sets of options:
  - **Trusted_Connection**: Set to `True` to use Windows authentication.
  - **User ID** and **Password**: The credentials used to authenticate with SQL Server authentication.

For all connection options, see the [SqlConnection.ConnectionString documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlclient.sqlconnection.connectionstring).

Your connection string will be stored in .NET user secrets.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **Microsoft.EntityFrameworkCore.SqlServer** package for SQL Server connections.

## Advanced Configuration

### Custom Schema

```terminal
>ivy db add --provider SqlServer --name MySqlServer --schema MyCustomSchema
```

### Schema Support

SQL Server supports multiple schemas. When connecting with Ivy, you'll be prompted to select a schema from your database, or you can specify one directly using the `--schema` parameter:

```terminal
>ivy db add --provider SqlServer --name MySqlServer --schema MyCustomSchema
```

See Microsoft's [Create a database schema](https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/create-a-database-schema) for more details.

## Security Best Practices

- **Use Windows Authentication** when possible for local development
- **Use Azure AD authentication** for Azure SQL Database
- **Enable encryption** in connection strings for production

## Troubleshooting

### Common Issues

**Connection Problems**
- Verify server is running and network connectivity
- Check credentials and permissions
- Ensure firewall allows port 1433

For detailed troubleshooting, see [SQL Server Troubleshooting](https://learn.microsoft.com/en-us/troubleshoot/sql/welcome-sql-server).

## Related Documentation

- [Database Overview](Overview.md)
- [PostgreSQL Provider](PostgreSql.md)
- [MySQL Provider](MySql.md)
- [SQLite Provider](SQLite.md)
- [SQL Server Technical Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)
- [SQL Server EF Core Database Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
