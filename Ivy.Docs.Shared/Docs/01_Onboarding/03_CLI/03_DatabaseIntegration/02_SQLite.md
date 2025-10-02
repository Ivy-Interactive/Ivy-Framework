---
title: SQLite
---

# SQLite Database Provider

<Ingress>
Connect your Ivy application to SQLite with automatic Entity Framework configuration.
</Ingress>

## Overview

SQLite is a lightweight, file-based relational database that's perfect for development, testing, and applications that need a simple, self-contained database solution. No server setup required! Learn more about SQLite at the [official SQLite website](https://www.sqlite.org/).

## Adding a Database Connection

To set up SQLite with Ivy, run the following command and choose `Sqlite` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for the path to your database file:

```terminal
Path to database file: (existing_file.db):
```

Ivy will attempt to suggest existing SQLite files in your project, but you can specify any path. Ivy automatically adds the database file to your project with `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` so it's included when building your project.

A connection string will be automatically generated as:

```text
Data Source=data.db
```

Unlike other providers, SQLite doesn't store this connection string in .NET user secrets. Instead, it is included directly in the generated DbContextFactory source file.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **Microsoft.EntityFrameworkCore.Sqlite** package for SQLite connections.

## Database Initialization

The Database Generator for SQLite projects includes automatic database initialization and improved logging capabilities:

- **Automatic Database Creation**: The generated DbContextFactory automatically creates the SQLite database file on first use
- **Thread-Safe Initialization**: Uses semaphore locking for safe concurrent access
- **Flexible Storage**: Supports custom storage volumes with a default location in the user's local application data folder
- **Better Logging**: Entity Framework logs are properly routed through the application's logging infrastructure

The generated factory accepts optional volume and logger parameters for enhanced configuration:

```csharp
public MyDbContextFactory(
    ServerArgs args,
    IVolume? volume = null,
    ILogger? logger = null
)
```

<Callout Type="info">
The `IVolume` parameter allows custom storage locations for your SQLite database, defaulting to the user's local application data folder. Inject your custom `IVolume` implementation through dependency injection for production deployments, containerized applications, or multi-tenant scenarios.
</Callout>

For more details on volume configuration, see the [Volume documentation](../../02_Concepts/Volume.md).

## SQLite-Specific Features

Key advantages:
- **Zero-configuration** - no server setup
- **Cross-platform** database files
- **Full-text search** and **JSON support**

See [SQLite Features](https://www.sqlite.org/features.html) for details.

## Security Best Practices

- **Secure file permissions**
- **Use WAL mode**: `PRAGMA journal_mode=WAL`
- **Enable foreign keys**: `PRAGMA foreign_keys=ON`

See [SQLite Security Considerations](https://www.sqlite.org/security.html) for more.

## Troubleshooting

### Common Issues

**File Access Issues**
- Check read/write permissions and directory existence
- Ensure file isn't locked by another process

**Database Locked Errors**
- Close connections properly and use WAL mode

See the [SQLite FAQ](https://www.sqlite.org/faq.html) for more help.

## Related Documentation

- [Database Overview](Overview.md)
- [PostgreSQL Provider](PostgreSql.md)
- [SQL Server Provider](SqlServer.md)
- [MySQL Provider](MySql.md)
- [Official SQLite Documentation](https://www.sqlite.org/docs.html)
- [EF Core SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
