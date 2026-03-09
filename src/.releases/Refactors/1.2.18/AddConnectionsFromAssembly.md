# IConnection and AddConnectionsFromAssembly Breaking Changes - v1.2.18

## Summary

There are two related breaking changes to the connection registration system:

1. `Server.AddConnectionsFromAssembly()` now accepts an optional `Assembly?` parameter. This is a binary breaking change that requires recompilation of consuming code.
2. The `IConnection.RegisterServices` method signature has changed to accept a `Server` instance instead of an `IServiceCollection`. This is a compile-time breaking change for all custom connection implementations.

## What Changed

### 1. IConnection Signature (Compile-Time Break)

#### Before

```csharp
using Microsoft.Extensions.DependencyInjection;

public class MyConnection : IConnection
{
    // ...other methods...
    
    public void RegisterServices(IServiceCollection services)
    {
        // Custom registration
    }
}
```

#### After

```csharp
using Ivy;

public class MyConnection : IConnection
{
    // ...other methods...
    
    public void RegisterServices(Server server)
    {
        // Change from using 'services' to 'server.Services' or 'server.UseAuth()'
    }
}
```

### 2. AddConnectionsFromAssembly (Binary Break)

If you see a `MissingMethodException` at runtime for `AddConnectionsFromAssembly()`, it means the consuming project was compiled against an older version of the framework where this method took no parameters.

## How to Find Affected Code

Run a search for custom `IConnection` implementations:

```regex
: IConnection
```

Or specifically looking for the old `RegisterServices` signature:

```regex
public void RegisterServices\(IServiceCollection
```

## How to Refactor

1. **For the Compile Break:** Update any classes that implement `IConnection`. Change the `RegisterServices(IServiceCollection services)` signature to `RegisterServices(Server server)`. If the method previously used `services.Add...`, change it to use `server.Services.Add...`, or use `server.UseAuth()` if it's an Auth connection.
2. **For the Binary Break:** Simply `dotnet clean` and `dotnet build` the consuming project to recompile against the new optional assembly parameter signature.

## Verification

After refactoring, run:

```bash
dotnet clean
dotnet build
```
