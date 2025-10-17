---
searchHints:
  - spanner
  - google
  - database
  - cloud
  - distributed
  - db
---

# Google Spanner Database Provider

<Ingress>
Connect your Ivy application to Google Cloud Spanner with automatic Entity Framework configuration.
</Ingress>

## Overview

Google Cloud Spanner is a fully managed, mission-critical, relational database service that offers transactional consistency at global scale, automatic, synchronous replication for high availability, and support for schema changes without downtime. Ivy provides seamless integration with Google Cloud Spanner through Entity Framework Core.

## Authenticating to Google Spanner

Before using Google Spanner with Ivy, you usually must configure Google's Application Default Credentials (or ADC for short). For local development using a Google Account, [install](https://cloud.google.com/sdk/docs/install) the Google Cloud CLI and run:

```terminal
>gcloud auth application-default login
```

Ivy will check for Google application credentials during setup. If credentials aren't found, you can still proceed (especially when using the Spanner emulator), but you'll need proper credentials for production usage.

For comprehensive instructions, see the [ADC documentation](https://cloud.google.com/docs/authentication/provide-credentials-adc).

## Adding a Database Connection

To set up Google Spanner with Ivy, first make sure you have configured ADC in the previous section (unless using the Spanner emulator). Then, run the following command and choose `Spanner` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted to choose a Spanner database. There are two ways to make this choice:

- **By selection**: All accessible databases are fetched and displayed. You are asked to choose one of them.
- **By component**: If an API necessary for enumerating databases is not enabled for your project, then you must manually enter the components that identify the database.

A Spanner database is uniquely identified by three components:
- **Project ID**
- **Instance ID**
- **Database ID**

Once these components have been selected, by either method above, a connection string of the following format is generated and stored in .NET user secrets:

```text
Data Source=projects/{projectId}/instances/{instanceId}/databases/{databaseId}
```

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **Google.Cloud.EntityFrameworkCore.Spanner** package and imports the `Google.Cloud.Spanner.V1` namespace for Spanner connections.

## Advanced Configuration

### Working with the Spanner Emulator

Ivy works with the Spanner emulator for development. Set up the emulator with:

**Windows:**
```terminal
>gcloud emulators spanner start
>gcloud emulators spanner env-init > set_vars.cmd && set_vars.cmd
```

**macOS/Linux:**
```terminal
>gcloud emulators spanner start
>$(gcloud emulators spanner env-init)
```

When using the emulator, you don't need to set up Google application credentials. However, you do need to manually build a connection string and pass it to Ivy CLI. Replace `{projectId}`, `{instanceId}` and `{databaseId}` below:

```terminal
>ivy db add --provider Spanner --connection-string "Data Source=projects/{projectId}/instances/{instanceId}/databases/{databaseId}; EmulatorDetection=EmulatorOnly;"
```

For more details on emulator setup, see the [Spanner Emulator documentation](https://cloud.google.com/spanner/docs/emulator).

## Spanner-Specific Features

Key features that Ivy can leverage:
- **Interleaved tables** for parent-child relationships
- **Strong consistency** across regions
- **Secondary indexes** for query optimization

For more information, visit the [Spanner documentation](https://cloud.google.com/spanner/docs).

## Security Best Practices

- **Use service accounts** with minimum required permissions
- **Enable audit logging** for security monitoring
- **Implement row-level security** where appropriate
- **Use VPC Service Controls** to restrict access in production

## Troubleshooting

### Common Issues

**Authentication Failed**
- Ensure Google application credentials are properly configured
- Verify service account has appropriate permissions

**Connection Timeouts**
- Check network connectivity to Google Cloud
- Ensure your Spanner instance is running and healthy

## Related Documentation

- [Database Overview](Overview.md)
- [SQL Server Provider](SqlServer.md)
- [PostgreSQL Provider](PostgreSql.md)
- [Official Google Cloud Spanner Documentation](https://cloud.google.com/spanner/docs)
- [Google.Cloud.EntityFrameworkCore.Spanner Package](https://github.com/googleapis/dotnet-spanner-entity-framework)
