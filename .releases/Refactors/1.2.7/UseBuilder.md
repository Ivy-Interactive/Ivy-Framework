# Refactor UseBuilder to UseWebApplicationBuilder

The `Server.UseBuilder()` method has been renamed to `UseWebApplicationBuilder()`.

## Goal

Update the server configuration to use the new method name.

## Locate Code

Look for the server initialization code, typically in `Program.cs`.

**Search Query:** `.UseBuilder()`

## Required Changes

### Rename Method

From:
```csharp
server.UseBuilder(builder => 
{
    // ...
});
```

To:
```csharp
server.UseWebApplicationBuilder(builder => 
{
    // ...
});
```
