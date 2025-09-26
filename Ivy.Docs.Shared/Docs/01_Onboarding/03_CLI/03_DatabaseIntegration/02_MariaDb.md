---
title: MariaDB
---

# MariaDB Database Provider

<Ingress>
Connect your Ivy application to MariaDB with automatic Entity Framework configuration.
</Ingress>

## Overview

MariaDB is a popular open-source relational database that started as a fork of MySQL. It offers enhanced features, improved performance, and better storage engines while maintaining MySQL compatibility. For more information, visit the [MariaDB documentation](https://mariadb.com/docs/general-resources/about/about-mariadb).

## Adding a Database Connection

To set up MariaDB with Ivy, run the following command and choose `MariaDb` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for a connection string. The connection string you provide should follow this format:

```text
Server=localhost; Database=my_db; User=user; Password=password;
```

Specifically, your connection string should contain the following information, in the form of semicolon-separated key-value pairs:

- **Server**: The hostname of your MariaDB server instance.
- **Database**: The name of the database you wish to connect to.
- **User** and **Password**: The credentials used to authenticate to the server.

For all connection options, see the [MySqlConnector documentation](https://mysqlconnector.net/connection-options/).

> **Note**: `MySqlConnector` is an ADO.NET driver, used by Ivy to connect with MariaDB and MySQL.

Your connection string will be stored in .NET user secrets.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **Pomelo.EntityFrameworkCore.MySql** package for MariaDB connections.

> **Note**: When you provide a connection string, Ivy will verify that you're connecting to a MariaDB server (not MySQL). If it detects MySQL instead, you'll be prompted to use the MySQL provider.

## MariaDB-Specific Features

Key advantages over MySQL:
- **Advanced JSON support** with better performance
- **Temporal tables** for data versioning
- **Multiple storage engines** including Aria and ColumnStore

See the [MariaDB Documentation](https://mariadb.com/kb/en/library/documentation/) for details.

## Security Best Practices

- **Use SSL connections** in production environments
- **Create dedicated database users** with minimal required permissions
- **Enable binary logging** for point-in-time recovery
- **Implement proper backup strategies** with MariaDB backup tools

## Troubleshooting

### Common Issues

**Connection Issues**
- Verify server is running and listening on the expected port
- Check firewall settings

**Authentication Problems**
- Verify credentials and user privileges

See [MariaDB Troubleshooting](https://mariadb.com/kb/en/troubleshooting-connection-issues/) for more help.

## Related Documentation

- [Database Overview](Overview.md)
- [MySQL Provider](MySql.md)
- [PostgreSQL Provider](PostgreSql.md)
- [SQL Server Provider](SqlServer.md)
- [Official MariaDB Documentation](https://mariadb.com/kb/en/documentation/)
- [Pomelo.EntityFrameworkCore.MySql for MariaDB](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
