---
searchHints:
  - dependency-injection
  - di
  - useservice
  - services
  - ioc
  - container
---

# UseService

<Ingress>
Services in Ivy provide dependency injection and service management for clean application architecture.
</Ingress>

## Overview

The service system in Ivy supports:

- Dependency injection
- Service registration and [configuration](../../../01_Onboarding/02_Concepts/01_Program.md)
- Service [lifecycle management](../../../01_Onboarding/02_Concepts/02_Views.md)
- Scoped and singleton services
- Service interfaces and implementations
- Service [middleware](../../../01_Onboarding/02_Concepts/01_Program.md)

## Basic Usage

Use the `UseService<T>()` hook to access registered services in your views:

```csharp demo-below
public class ServiceExampleView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var message = UseState("Hello from service!");

        return Layout.Vertical()
            | Text.P(message.Value)
            | new Button("Show Toast",
                onClick: _ => client.Toast(message.Value, "Service Demo"));
    }
}
```

### Service Registration

Register services in your [application startup](../../../01_Onboarding/02_Concepts/01_Program.md):

```csharp
public class Program
{
    public static void Main()
    {
        var server = new Server()
            .UseService<IMyService, MyService>()
            .UseService<IDataService, DataService>(ServiceLifetime.Singleton)
            .UseService<IAuthService, AuthService>();
    }
}
```

### Service Descriptions

Services can provide custom descriptions by implementing the `IDescribableService` interface. Use `ServerDescriptionReader` to read environment-specific service descriptions from your [application](../../../01_Onboarding/02_Concepts/10_Apps.md).

```csharp
// Implement IDescribableService for custom service descriptions
public class MyService : IMyService, IDescribableService
{
    public string ToYaml()
    {
        return "Custom service description in YAML format";
    }
}

// Read service descriptions with environment context
var description = await ServerDescriptionReader.ReadAsync(
    projectDirectory,
    environment: "PRODUCTION"
);
```

The `ServiceDescription` class includes an optional `Description` property for better documentation of your services.

### Service Interfaces

Define service interfaces for better abstraction:

```csharp
public interface IDataService
{
    Task<IEnumerable<Data>> GetDataAsync();
    Task<Data> GetDataByIdAsync(string id);
    Task SaveDataAsync(Data data);
}

public class DataService : IDataService
{
    private readonly ILogger<DataService> _logger;

    public DataService(ILogger<DataService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<Data>> GetDataAsync()
    {
        _logger.LogInformation("Fetching data");
        // Implementation
    }

    public async Task<Data> GetDataByIdAsync(string id)
    {
        _logger.LogInformation("Fetching data for id: {Id}", id);
        // Implementation
    }

    public async Task SaveDataAsync(Data data)
    {
        _logger.LogInformation("Saving data");
        // Implementation
    }
}
```

### Service Lifetime

Ivy supports different service lifetimes:

```csharp
// Singleton - Created once for the entire application
.UseService<ICacheService, CacheService>(ServiceLifetime.Singleton)

// Scoped - Created once per request
.UseService<IDbContext, DbContext>(ServiceLifetime.Scoped)

// Transient - Created each time requested
.UseService<ILogger, Logger>(ServiceLifetime.Transient)
```

### Service Middleware

Add middleware to services for cross-cutting concerns:

```csharp
public class LoggingServiceMiddleware : IServiceMiddleware
{
    private readonly ILogger _logger;

    public LoggingServiceMiddleware(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> next)
    {
        _logger.LogInformation("Service method called");
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service method failed");
            throw;
        }
    }
}

// Register middleware
.UseServiceMiddleware<LoggingServiceMiddleware>()
```

## Best Practices

1. **Interface-based Design**: Always define interfaces for your services
2. **Single Responsibility**: Each service should have a single, well-defined purpose
3. **Dependency Injection**: Use constructor injection for dependencies
4. **Service Lifetime**: Choose appropriate lifetimes for your services
5. **Error Handling**: Implement proper error handling in services
6. **Logging**: Use logging for important operations and errors
7. **Testing**: Make services easily testable through interfaces

## Examples

<Details>
<Summary>
Simple Service Usage
</Summary>
<Body>

Use a service to display notifications or interact with client features:

```csharp demo-below
public class SimpleServiceView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var count = UseState(0);

        return Layout.Vertical()
            | Text.P($"Button clicked {count.Value} times")
            | new Button("Show Toast", onClick: _ =>
            {
                count.Set(count.Value + 1);
                client.Toast($"Notification #{count.Value}", "Service Demo");
            });
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Using Multiple Services
</Summary>
<Body>

Access multiple services in a single view to combine their functionality:

```csharp demo-below
public class MultiServiceView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var message = UseState("Ready");
        var count = UseState(0);

        return Layout.Vertical()
            | Text.P($"Last action: {message.Value}")
            | Text.P($"Total actions: {count.Value}")
            | (Layout.Horizontal()
                | new Button("Action 1", onClick: _ =>
                {
                    count.Set(count.Value + 1);
                    client.Toast("Action 1 executed", "Service Demo");
                    message.Set("Action 1 completed");
                })
                | new Button("Action 2", onClick: _ =>
                {
                    count.Set(count.Value + 1);
                    client.Toast("Action 2 executed", "Service Demo");
                    message.Set("Action 2 completed");
                }));
    }
}
```

</Body>
</Details>
