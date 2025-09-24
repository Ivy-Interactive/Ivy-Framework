# Secrets Management

<Ingress>
The Ivy Framework provides a comprehensive secrets management foundation that enables compile-time tracking of required application secrets, ensuring all necessary configuration is in place before deployment.
</Ingress>

## Overview

The Ivy Framework now includes a robust foundation for secrets management through the introduction of the `IHaveSecrets` interface and `Secret` record. This infrastructure enables compile-time tracking of required application secrets, making it easier to validate that all necessary configuration is in place before deployment.

## Core Components

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

### Implementing IHaveSecrets

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

Database connections automatically declare their required secrets when generated through the Ivy CLI. This integration ensures that your database connection strings are automatically included in secrets validation.

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

## Benefits

### Compile-time Validation

The secrets management system provides several key benefits:

1. **Compile-time Tracking**: All required secrets are declared at compile time, making it impossible to forget required configuration.

2. **Deployment Safety**: The system prevents deployment failures due to missing database configuration or other secrets.

3. **Consistency**: Standardized naming conventions ensure consistent secret management across your application.

4. **Automation**: Database connections automatically declare their secrets, reducing manual configuration overhead.

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
    
    // Validate that all secrets are present in configuration
    // This would typically check environment variables, appsettings, etc.
}
```

## Best Practices

### Secret Naming Conventions

Follow these conventions for consistent secret management:

1. **Use Hierarchical Names**: Use colon-separated paths to group related secrets

   ```csharp
   new Secret("Database:Production:ConnectionString")
   new Secret("Database:Staging:ConnectionString")
   ```

2. **Be Descriptive**: Use clear, descriptive names that indicate the purpose

   ```csharp
   new Secret("PaymentGateway:Stripe:SecretKey")
   new Secret("EmailService:SendGrid:ApiKey")
   ```

3. **Follow .NET Conventions**: Align with standard .NET configuration patterns

   ```csharp
   new Secret("ConnectionStrings:DefaultConnection")
   new Secret("Logging:ApplicationInsights:ConnectionString")
   ```

### Service Implementation

When implementing services that require secrets:

1. **Declare All Dependencies**: Include all secrets your service needs, even if they're used indirectly
2. **Group Related Secrets**: Use hierarchical naming to group related configuration
3. **Document Requirements**: Consider adding XML documentation to explain what each secret is used for

```csharp
/// <summary>
/// Service for processing payments through external gateways
/// </summary>
public class PaymentService : IHaveSecrets
{
    /// <summary>
    /// Returns the secrets required by this payment service
    /// </summary>
    public Secret[] GetSecrets()
    {
        return
        [
            new Secret("PaymentGateway:Stripe:SecretKey"),      // For processing payments
            new Secret("PaymentGateway:Stripe:WebhookSecret"),  // For webhook validation
            new Secret("PaymentGateway:PayPal:ClientSecret"),   // Alternative payment method
            new Secret("Database:Payments:ConnectionString")    // For transaction logging
        ];
    }
}
```

## Integration with Deployment

The secrets management system integrates seamlessly with deployment pipelines:

1. **Pre-deployment Validation**: Check that all declared secrets are available
2. **Configuration Generation**: Use declared secrets to generate configuration templates
3. **Documentation**: Automatically generate documentation of required secrets
4. **Security Scanning**: Validate that secrets are properly secured in your deployment environment

This foundation makes Ivy applications more robust and deployment-ready by ensuring that all necessary configuration is properly declared and validated.
