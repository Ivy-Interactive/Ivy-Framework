---
searchHints:
  - connection
  - integration
  - external-service
  - api
  - database
  - iconnection
  - test-connection
  - register-services
  - ihavesecrets
  - connection-entity
---

# Connections

<Ingress>
Ivy Connections provide a unified abstraction for integrating external data sources and services into your application — from databases to third-party APIs.
</Ingress>

## Overview

A **Connection** in Ivy represents any external resource your application communicates with:

- **[Databases](../03_CLI/05_DatabaseIntegration/01_DatabaseOverview.md)** — [SQL Server](../03_CLI/05_DatabaseIntegration/02_SqlServer.md), [PostgreSQL](../03_CLI/05_DatabaseIntegration/02_PostgreSql.md), [MySQL](../03_CLI/05_DatabaseIntegration/02_MySql.md), [SQLite](../03_CLI/05_DatabaseIntegration/02_SQLite.md), etc.
- **Third-party APIs** — Payment gateways, messaging services, analytics
- **[Cloud services](../03_CLI/06_Deployment/01_DeploymentOverview.md)** — [AWS](../03_CLI/06_Deployment/02_AWS.md), [Azure](../03_CLI/06_Deployment/03_Azure.md), [Google Cloud](../03_CLI/06_Deployment/04_GCP.md)
- **Custom internal services** — Microservices, legacy systems

Connections implement the `IConnection` interface, which provides a standardized way to:
- Register required services in the [DI container](./01_Program.md)
- Expose metadata about the connection (name, type, entities)
- Integrate with Ivy's [secrets management](./14_Secrets.md)
- Test connectivity and configuration

## The IConnection Interface

```csharp
public interface IConnection
{
    string GetContext(string connectionPath);
    string GetNamespace();
    string GetName();
    string GetConnectionType();
    ConnectionEntity[] GetEntities();
    void RegisterServices(Server server);
    Task<(bool ok, string? message)> TestConnection(IConfiguration config);
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
| `RegisterServices` | Registers required services on the `Server` instance |
| `TestConnection` | Validates connectivity and configuration using the provided `IConfiguration` |

## Connection Types

### Database Connections

Database connections are the most common type. They are automatically generated when you use the [Ivy CLI database commands](../03_CLI/05_DatabaseIntegration/01_DatabaseOverview.md):

```terminal
>ivy db add --provider Postgres --name MyDatabase
```

This generates a connection class that implements [IConnection](#the-iconnection-interface) along with Entity Framework context and entities. See the [Database Integration Guide](../03_CLI/05_DatabaseIntegration/01_DatabaseOverview.md) for details.

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
    
    public void RegisterServices(Server server)
    {
        server.Services.AddSingleton<IStripeClient, StripeClient>();
        server.Services.AddScoped<IPaymentService, StripePaymentService>();
    }

    public Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        return Task.FromResult((true, "Connection successful"));
    }
    
    public Secret[] GetSecrets() =>
    [
        new("Stripe:SecretKey"),
        new("Stripe:PublishableKey")
    ];
}
```

## Connection Secrets

Connections often require sensitive configuration like API keys or connection strings. Implement [IHaveSecrets](./14_Secrets.md) alongside `IConnection` to declare required secrets:

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

This integrates with Ivy's [secrets management](./14_Secrets.md) for compile-time validation.

## Registering Connections

### Automatic Registration

Register all connections from your assembly automatically on the [Server](./01_Program.md):

```csharp
var server = new Server();
server.AddConnectionsFromAssembly();
await server.RunAsync();
```

This scans the entry assembly for all classes implementing `IConnection` and calls their `RegisterServices` method.

### Manual Registration

For more control, register connections manually with your [Server](./01_Program.md):

```csharp
var server = new Server();

var stripeConnection = new StripeConnection();
stripeConnection.RegisterServices(server);

await server.RunAsync();
```

## Using Connection Services

After registration, use connection services in your [views](./02_Views.md):

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

The example uses [UseService](../../03_Hooks/02_Core/11_UseService.md) to resolve the connection's registered service and a [DataTable](../../02_Widgets/07_Advanced/01_DataTable.md) to display the data.


