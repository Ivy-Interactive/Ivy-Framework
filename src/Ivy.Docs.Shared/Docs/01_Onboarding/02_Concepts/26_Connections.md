---
searchHints:
  - connection
  - integration
  - external-service
  - api
  - database
  - iconnection
---

# Connections

<Ingress>
Ivy Connections provide a unified abstraction for integrating external data sources and services into your application — from databases to third-party APIs.
</Ingress>

## Overview

A **Connection** in Ivy represents any external resource your application communicates with:

- **Databases** — SQL Server, PostgreSQL, MySQL, SQLite, etc.
- **Third-party APIs** — Payment gateways, messaging services, analytics
- **Cloud services** — AWS, Azure, Google Cloud
- **Custom internal services** — Microservices, legacy systems

Connections implement the `IConnection` interface, which provides a standardized way to:
- Register required services in the DI container
- Expose metadata about the connection (name, type, entities)
- Integrate with Ivy's secrets management

## The IConnection Interface

```csharp
public interface IConnection
{
    string GetContext(string connectionPath);
    string GetNamespace();
    string GetName();
    string GetConnectionType();
    ConnectionEntity[] GetEntities();
    void RegisterServices(IServiceCollection services);
}

public record ConnectionEntity(string Singular, string Plural);
```

### Method Reference

| Method | Description |
|--------|-------------|
| `GetContext` | Returns the connection's context for code generation |
| `GetNamespace` | Returns the namespace where the connection is defined |
| `GetName` | Returns the connection's display name |
| `GetConnectionType` | Returns the type of connection (e.g., "Database", "API") |
| `GetEntities` | Returns available entities (tables, resources) |
| `RegisterServices` | Registers required services in the DI container |

## Connection Types

### Database Connections

Database connections are the most common type. They are automatically generated when you use the [Ivy CLI database commands](../03_CLI/05_DatabaseIntegration/01_DatabaseOverview.md):

```terminal
>ivy db add --provider Postgres --name MyDatabase
```

This generates a connection class that implements `IConnection` along with Entity Framework context and entities. See the [Database Integration Guide](../03_CLI/05_DatabaseIntegration/01_DatabaseOverview.md) for details.

### API Connections

You can create custom connections for any external API. Here's an example of a payment gateway connection:

```csharp
public class StripeConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;
    
    public string GetNamespace() => typeof(StripeConnection).Namespace!;
    
    public string GetName() => "Stripe";
    
    public string GetConnectionType() => "PaymentAPI";
    
    public ConnectionEntity[] GetEntities() =>
    [
        new("Customer", "Customers"),
        new("Payment", "Payments"),
        new("Subscription", "Subscriptions")
    ];
    
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IStripeClient, StripeClient>();
        services.AddScoped<IPaymentService, StripePaymentService>();
    }
    
    public Secret[] GetSecrets() =>
    [
        new("Stripe:SecretKey"),
        new("Stripe:PublishableKey")
    ];
}
```

## Connection Secrets

Connections often require sensitive configuration like API keys or connection strings. Implement `IHaveSecrets` alongside `IConnection` to declare required secrets:

```csharp
public class MyApiConnection : IConnection, IHaveSecrets
{
    // ... IConnection implementation ...
    
    public Secret[] GetSecrets() =>
    [
        new("MyApi:ApiKey"),
        new("MyApi:ApiSecret")
    ];
}
```

This integrates with Ivy's [secrets management](./20_Secrets.md) for compile-time validation.

## Registering Connections

### Automatic Registration

Register all connections from your assembly automatically:

```csharp
var server = new Server();
server.AddConnectionsFromAssembly();
await server.RunAsync();
```

This scans the entry assembly for all classes implementing `IConnection` and calls their `RegisterServices` method.

### Manual Registration

For more control, register connections manually:

```csharp
var server = new Server();

var stripeConnection = new StripeConnection();
stripeConnection.RegisterServices(server.Services);

await server.RunAsync();
```

## Using Connection Services

After registration, use connection services in your views:

```csharp
public class PaymentView : ViewBase
{
    public override object? Build()
    {
        var paymentService = UseService<IPaymentService>();
        var payments = paymentService.GetRecentPayments();
        
        return new DataTable<Payment>(payments);
    }
}
```


