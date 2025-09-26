# Supabase Database Provider

<Ingress>
Connect your Ivy application to Supabase with automatic Entity Framework configuration for PostgreSQL.
</Ingress>

## Overview

Supabase is an open-source Firebase alternative that provides a PostgreSQL database with real-time capabilities, authentication, and storage. Ivy integrates with Supabase using its PostgreSQL backend through the Npgsql provider. Learn more at the [Supabase website](https://supabase.com/).

## Adding a Database Connection

To set up a Supabase database with Ivy, run the following command and choose `Supabase` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for a connection string. To get your connection string:

1. Go to your Supabase project dashboard
2. Click the "Connect" button at the top of the page to access all available connection strings
3. Choose the connection string type that's best for you:
   - **Direct connection**: Best for persistent servers (VMs, containers)
   - **Transaction pooler**: For applications with many short-lived connections
   - **Session pooler**: Should only be used for applications that must use IPv4

For more detailed information, see the [official Supabase documentation on connecting to Postgres](https://supabase.com/docs/guides/database/connecting-to-postgres).

![Supabase connect screen, showing all three types of connection string](assets/supabase_connect_screen.webp "Supabase connect screen")

Your connection string will look something like this:

```text
postgresql://postgres:[YOUR-PASSWORD]@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres
```

In addition to the URI-style format, Ivy also supports the standard Entity Framework key-value format:

```text
Host=db.[YOUR-PROJECT-REF].supabase.co;Database=postgres;Username=postgres;Password=[YOUR-PASSWORD]
```

For either format, remember to replace `[YOUR-PASSWORD]` with your database password, which is the password you used when creating the project (not your Supabase account password).

Ivy CLI will automatically detect and convert URI-style connection strings to the key-value format. After conversion, your connection string will be stored in .NET user secrets.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy treats Supabase as a specialized PostgreSQL provider, using the **Npgsql.EntityFrameworkCore.PostgreSQL** package with specific configuration for Supabase compatibility. This includes handling connection string conversions and ensuring proper connection pooling settings.

## Supabase-Specific Features

Supabase offers additional features like real-time subscriptions and Row Level Security (RLS). See the [Supabase Features documentation](https://supabase.com/docs/guides/database/overview).

## Security Best Practices

- **Use SSL connections** (enabled by default with Ivy and Supabase)
- **Implement Row Level Security** policies for user data isolation
- **Use service role key** only for administrative operations
- **Monitor database activity** through Supabase dashboard

See [Supabase Row Level Security documentation](https://supabase.com/docs/guides/database/postgres/row-level-security) for implementation details.

## Troubleshooting

### Common Issues

**Connection Problems**
- Verify your Supabase project is active
- Use correct host URL and credentials

**Other Issues**
See [Supabase Connection Troubleshooting](https://supabase.com/docs/guides/database/connecting-to-postgres#troubleshooting-and-postgres-connection-string-faqs) for more troubleshooting steps.

## Related Documentation

- [Database Overview](Overview.md)
- [Supabase Authentication](../04_Authentication/Supabase.md)
- [PostgreSQL Provider](PostgreSql.md)
- [Official Supabase Documentation](https://supabase.com/docs)
- [Npgsql Entity Framework Core Provider](https://www.npgsql.org/efcore/)
