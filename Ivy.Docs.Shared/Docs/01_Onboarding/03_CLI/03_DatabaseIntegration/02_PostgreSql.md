---
title: PostgreSQL
searchHints:
  - postgres
  - postgresql
  - database
  - sql
  - relational
  - db
---

# PostgreSQL Database Provider

<Ingress>
Connect your Ivy application to PostgreSQL with automatic Entity Framework configuration.
</Ingress>

## Overview

PostgreSQL is an advanced open-source relational database known for its reliability, feature robustness, and performance. Ivy provides seamless integration with PostgreSQL through Entity Framework Core.

## Adding a Database Connection

To set up PostgreSQL with Ivy, run the following command and choose `Postgres` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for a connection string. The connection string you provide should follow this format:

```text
Host=localhost;Database=mydb;Username=user;Password=pass
```

Specifically, your connection string should contain the following information, in the form of semicolon-separated key-value pairs:

- **Host**: The hostname of your Postgres server instance.
- **Database**: The name of the database you wish to connect to.
- **Username** and **Password**: The credentials used to authenticate to the server.

> **Note**: Ivy also supports URI-style connection strings (e.g., `postgresql://user:password@host:port/dbname`) and will automatically convert them to the key-value format.

For all connection options, see [Npgsql Connection String Parameters](https://www.npgsql.org/doc/connection-string-parameters.html).

Your connection string will be stored in .NET user secrets.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **Npgsql.EntityFrameworkCore.PostgreSQL** package for PostgreSQL connections.

## Advanced Configuration

### Custom Schema

PostgreSQL supports multiple schemas. When configuring your PostgreSQL database with Ivy, you'll be prompted to select a schema from your database, or you can specify one directly using the `--schema` parameter:

```terminal
>ivy db add --provider Postgres --name MyPostgres --schema MyCustomSchema
```

## PostgreSQL-Specific Features

Key features Ivy can leverage:
- **JSONB columns** for document storage
- **Array types** for collections
- **Custom data types** and enums

See [About PostgreSQL](https://www.postgresql.org/about/) for more information on PostgreSQL features.

## Security Best Practices

- **Create dedicated database users** with minimal required permissions
- **Enable row-level security** when appropriate
- **Use connection pooling** to optimize performance

## Troubleshooting

### Common Issues

**Connection Issues**
- Verify server is running and listening on expected port
- Check credentials and firewall settings

**Authentication Problems**
- Check `pg_hba.conf` configuration

For detailed help, see the [PostgreSQL Documentation](https://www.postgresql.org/docs/current/) and search for common issues in the [PostgreSQL Wiki](https://wiki.postgresql.org/wiki/Main_Page).

## Related Documentation

- [Database Overview](Overview.md)
- [SQL Server Provider](SqlServer.md)
- [MySQL Provider](MySql.md)
- [Supabase Provider](Supabase.md)
- [Official PostgreSQL Documentation](https://www.postgresql.org/docs/current/)
- [Npgsql Entity Framework Core Provider](https://www.npgsql.org/efcore/)
