---
description: Update IConnection implementations for the new RegisterServices and TestConnection signature.
---
# IConnection API Refactor

## Summary
The `IConnection` interface has been updated to improve dependency injection capabilities and runtime validation. `RegisterServices` now accepts a `Server` instance instead of `IServiceCollection`, providing access to the full server context during registration. A new `TestConnection` method has been introduced to enable configuration validation.

## Changes
This refactoring rule handles the following transformations:

### Update `RegisterServices` Signature
- **Before:**
  ```csharp
  public void RegisterServices(IServiceCollection services)
  {
      services.AddSingleton<MyService>();
  }
  ```
- **After:**
  ```csharp
  public void RegisterServices(Server server)
  {
      server.Services.AddSingleton<MyService>();
  }
  ```

### Implement `TestConnection` Method
All classes implementing `IConnection` must now define `TestConnection(IConfiguration config)`. 
- **After:**
  ```csharp
  public Task<(bool ok, string? message)> TestConnection(IConfiguration config)
  {
      return Task.FromResult((true, (string?)null));
  }
  ```
  *(Note: The actual implementation should validate the connection using `config` if applicable).*

## How to Find Affected Code
Search the codebase for classes implementing `IConnection` or containing the old `RegisterServices` signature:
```regex
public\s+void\s+RegisterServices\s*\(\s*IServiceCollection\s+services\s*\)
```
or search for exactly `IConnection`.

## Refactoring Instructions
1. Find every class extending `IConnection`.
2. Update the `RegisterServices` parameter from `IServiceCollection services` to `Server server`.
3. Inside `RegisterServices`, replace references to `services` with `server.Services`.
4. Add the `TestConnection` method implementation as required by the updated interface.
5. Run `dotnet clean && dotnet build` to verify the refactoring was successful.
