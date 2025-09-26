# Snowflake Database Provider

<Ingress>
Connect your Ivy application to Snowflake with automatic Entity Framework configuration.
</Ingress>

## Overview

Snowflake is a cloud-based data warehousing platform that provides a single, integrated solution for data storage, processing, and analytics. Ivy offers seamless integration with Snowflake through Entity Framework Core, allowing you to leverage Snowflake's powerful data processing capabilities in your applications.

## Adding a Database Connection

To set up Snowflake with Ivy, run the following command and choose `Snowflake` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for a connection string. The connection string you provide should follow this format:

```text
account=myaccount; db=mydatabase; schema=myschema; warehouse=mywarehouse; user=myuser; password=mypassword;
```

Specifically, your connection string should contain the following information, in the form of semicolon-separated key-value pairs:

- **account**: Snowflake account name. See Snowflake's [Account Identifiers Documentation](https://docs.snowflake.com/en/user-guide/admin-account-identifier) for instructions on how to find this.
- **db**: The name of the database you wish to connect to.
- **schema**: The name of the schema you wish to use.
- **warehouse**: The name of the warehouse you wish to use for compute.
- **user** and **password**: The credentials used to authenticate to the server, if using password-based authentication. Other connection options may be required if using a different authentication method.

For additional authentication methods (OAuth, SSO, etc.) and all connection options, see the `Snowflake.Data` [Connecting Documentation](https://github.com/snowflakedb/snowflake-connector-net/blob/master/doc/Connecting.md).

> **Note**: `Snowflake.Data` is Snowflake's official ADO.NET driver, used by Ivy.

Your connection string will be stored in .NET user secrets.

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Configuration

Ivy automatically configures the **EFCore.Snowflake** package for Snowflake connections.

## Snowflake-Specific Features

Key features Ivy can leverage:
- **Semi-structured data types** (JSON, Avro, Parquet)
- **Time Travel** for historical data access
- **Zero-copy cloning** and **Automatic clustering**

Learn more in the [Snowflake Documentation](https://docs.snowflake.com/en/user-guide).

## Security Best Practices

- **Use key pair authentication** instead of password authentication
- **Enable network policies** to restrict access
- **Use private connectivity** (AWS PrivateLink, Azure Private Link) when possible
- **Implement column-level security** for sensitive data

## Troubleshooting

### Common Issues

**Authentication Failed**
- Verify username and password are correct (if using password-based authentication)
- Check that your account identifier is correctly formatted

**Connection Timeouts**
- Verify warehouse is running
- Check network connectivity to Snowflake

**Schema Access Issues**
- Ensure the user has appropriate permissions
- Verify the schema exists in the specified database

## Related Documentation

- [Database Overview](Overview.md)
- [PostgreSQL Provider](PostgreSql.md)
- [SQL Server Provider](SqlServer.md)
- [Official Snowflake Documentation](https://docs.snowflake.com)
- [Snowflake.Data](https://github.com/snowflakedb/snowflake-connector-net/tree/master)
- [EFCore.Snowflake Package](https://github.com/Sielnix/EFCore.Snowflake)
