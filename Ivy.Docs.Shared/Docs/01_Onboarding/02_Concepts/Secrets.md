---
searchHints:
  - configuration
  - environment
  - security
  - credentials
  - api-keys
  - secrets
---

# Secrets

<Ingress>
The Ivy Framework provides a comprehensive secrets management foundation that enables compile-time tracking of required application secrets, ensuring all necessary configuration is in place before deployment.
</Ingress>

## Overview

The Ivy Framework now includes a robust foundation for secrets management through the introduction of the `IHaveSecrets` interface and `Secret` record. This infrastructure enables compile-time tracking of required application secrets, making it easier to validate that all necessary configuration is in place before deployment.

### IHaveSecrets Interface

The `IHaveSecrets` interface is the foundation of Ivy's secrets management system. Any class that requires secrets should implement this interface:

```csharp
public interface IHaveSecrets
{
    Secret[] GetSecrets();
}
```

### Secret Record

The `Secret` record represents a required secret configuration:

```csharp
public record Secret(string Name);
```

## Basic Usage

To declare that your service requires secrets, implement the `IHaveSecrets` interface:

```csharp
public class MyService : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new Secret("ApiKey"),
            new Secret("ConnectionString"),
            new Secret("OAuth:ClientSecret")
        ];
    }
}
```

### Hierarchical Secret Names

Ivy supports hierarchical secret naming using colon-separated paths, which aligns with .NET configuration standards:

```csharp
public class ConfigurationService : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new Secret("Database:ConnectionString"),
            new Secret("Auth:Jwt:SecretKey"),
            new Secret("External:PaymentGateway:ApiKey"),
            new Secret("Monitoring:ApplicationInsights:InstrumentationKey")
        ];
    }
}
```

## Database Connections with Built-in Secrets Declaration

Database connection classes automatically declare their required secrets when generated through the Ivy CLI. This integration ensures that your database connection strings are automatically included in secrets validation.

### Generated Connection Classes

When you generate a database connection using the Ivy CLI, the generated connection class implements both `IConnection` and `IHaveSecrets`:

```csharp
public class MyDatabaseConnection : IConnection, IHaveSecrets
{
    // ... existing connection methods ...

    public Secret[] GetSecrets()
    {
        return
        [
            new("ConnectionStrings:MyDatabase")
        ];
    }
}
```

### Connection String Format

The connection string secret name follows the colon-separated format (`ConnectionStrings:ConnectionName`) for consistency with .NET configuration standards:

```csharp
public class UserDatabaseConnection : IConnection, IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new("ConnectionStrings:UserDatabase"),
            new("ConnectionStrings:AnalyticsDatabase")
        ];
    }
}
```

### Configuration Validation

Before deployment, you can validate that all required secrets are properly configured:

```csharp
// Example validation logic (implementation depends on your deployment pipeline)
public void ValidateSecrets(IEnumerable<IHaveSecrets> services)
{
    var allSecrets = services
        .SelectMany(s => s.GetSecrets())
        .Select(s => s.Name)
        .ToHashSet();
}
```
