# Airtable Database Provider

<Ingress>
Connect your Ivy application to Airtable with automatic Entity Framework configuration for seamless integration with your Airtable bases.
</Ingress>

## Overview

Airtable is a cloud-based spreadsheet-database hybrid that combines the simplicity of a spreadsheet with the power of a database. Ivy provides integration with Airtable through Entity Framework Core, allowing you to leverage Airtable's flexible data organization in your applications.

## Adding a Database Connection

To set up Airtable with Ivy, run the following command and choose `Airtable` when asked to select a DB provider:

```terminal
>ivy db add
```

You will be asked to name your connection, then prompted for two pieces of information:

1. **Access Token**: Your Airtable personal access token (PAT)
2. **Base ID**: The ID of your Airtable base (starts with 'app')

These values will be combined into a connection string and stored in .NET user secrets:

```text
BaseId=appXXXXXXXXXXXXXX;ApiKey=patXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

See [Database Integration Overview](Overview.md) for more information on adding database connections to Ivy.

## Creating an Access Token

Airtable uses personal access tokens (PATs) for authentication. You can generate a token from the [Personal access tokens](https://airtable.com/create/tokens) page in your Airtable Builder Hub. Be sure to enable the scopes `data.records:read` and `schema.bases:read`, and if writing to Airtable is needed for your project, `data.records:write` as well. No other scopes are currently used by Ivy. Then, give it access to the Airtable base(s) you wish to use or select "Add all resources."

![Airtable create personal access token page](assets/airtable_create_pat.webp "Airtable create personal access token page")

For detailed instructions, see the [Airtable personal access tokens documentation](https://airtable.com/developers/web/guides/personal-access-tokens).

## Finding Your Base ID

To find your base ID, login to Airtable in your browser and visit the [Airtable API Reference](https://airtable.com/api). At the bottom of that page, you should see a list of bases your account has access to:

![Airtable API Reference](assets/airtable_api_reference.webp "Airtable API Reference")

Select the one you want your Ivy project to connect to. This will lead you to base-specific API documentation. You can find the base ID on the Introduction page. Look for "The ID of this base is..."

![Airtable Base-Specific API Reference, showing the chosen base's ID](assets/airtable_base_id.webp "Airtable Base-Specific API Reference")

## Configuration

Ivy automatically configures the **Ivy.Airtable.EFCore** package (an Ivy-specific fork of `Airtable.EFCore`) and imports the `Airtable.EFCore` and `AirtableApiClient` namespaces for Airtable connections. This allows you to interact with your Airtable bases using Entity Framework Core.

## Working With Airtable Tables

Ivy maps Airtable tables to entity classes, handles Airtable's data types, and provides standard Entity Framework CRUD operations.

## Airtable-Specific Features

Key features Ivy can leverage:
- **Rich field types** (attachments, links, formulas)
- **Record linking** for relationships
  - _Disclaimer: links are currently exposed as raw record IDs instead of entity references_
- **Views** for filtered data presentation

See [Airtable's API Reference](https://airtable.com/developers/web/api/introduction) for more details on Airtable features.

## Troubleshooting

### Common Issues

**Authentication Failed**
- Verify token validity and permissions

**Base Access Issues**
- Confirm Base ID and account access

**Rate Limiting**
- For details on API limits, see [Airtable API Rate Limits](https://airtable.com/developers/web/api/rate-limits).

## Related Documentation

- [Database Overview](Overview.md)
- [SQLite Provider](SQLite.md)
- [PostgreSQL Provider](PostgreSql.md)
- [Official Airtable API Documentation](https://airtable.com/developers/web/api/introduction)
- [Airtable .NET API Client](https://github.com/ngocnicholas/airtable.net)
