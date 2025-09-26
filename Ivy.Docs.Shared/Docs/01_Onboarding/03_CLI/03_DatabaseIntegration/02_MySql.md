---
title: MySQL
---

# MySQL Database Provider

<Ingress>
Connect your Ivy application to MySQL with automatic Entity Framework configuration.
</Ingress>

## Overview

MySQL is one of the world's most popular open-source relational databases, known for its speed, reliability, and ease of use. Ivy provides seamless integration with MySQL through Entity Framework Core.

## Adding a Database Connection

To set up MySQL with Ivy, run the following command and choose `MySql` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for a connection string. The connection string you provide should follow this format:

```text
Server=localhost; Database=my_db; User=user; Password=password;
```

Specifically, your connection string should contain the following information, in the form of semicolon-separated key-value pairs:

- **Server**: The hostname of your MySQL server instance.
- **Database**: The name of the database you wish to connect to.
- **User** and **Password**: The credentials used to authenticate to the server.

For all connection options, see the [MySqlConnector documentation](https://mysqlconnector.net/connection-options/).

> **Note**: `MySqlConnector` is an ADO.NET driver, used by Ivy to connect with MySQL and MariaDB.

Your connection string will be stored in .NET user secrets.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **Pomelo.EntityFrameworkCore.MySql** package for MySQL connections.

> **Note**: When you provide a connection string, Ivy will verify that you're connecting to an actual MySQL server (not MariaDB). If it detects MariaDB instead, you'll be prompted to use the MariaDB provider.

## MySQL-Specific Features

Key features Ivy can leverage:
- **JSON columns** for document storage (MySQL 5.7+)
- **Full-text indexes** for search functionality
- **Multiple storage engines** (InnoDB, MyISAM)

See [MySQL Feature Reference](https://dev.mysql.com/doc/refman/8.4/en/features.html) for details.

## Security Best Practices

- **Use SSL connections** in production environments
- **Create dedicated database users** with minimal required permissions
- **Enable binary logging** for point-in-time recovery
- **Use connection pooling** to optimize performance

For more security recommendations, see [MySQL Security Guidelines](https://dev.mysql.com/doc/refman/8.4/en/security-guidelines.html).

## Troubleshooting

### Common Issues

**Connection Issues**
- Verify server is running and listening on the expected port
- Check firewall settings

**Authentication Problems**
- Verify credentials and user privileges

See [MySQL Problems and Common Errors](https://dev.mysql.com/doc/refman/8.4/en/problems.html) for more help.

## Related Documentation

- [Database Overview](Overview.md)
- [MariaDB Provider](MariaDb.md)
- [PostgreSQL Provider](PostgreSql.md)
- [SQL Server Provider](SqlServer.md)
- [Official MySQL Documentation](https://dev.mysql.com/doc/)
- [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
