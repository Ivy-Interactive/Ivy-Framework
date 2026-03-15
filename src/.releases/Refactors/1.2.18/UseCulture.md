# Server.UseCulture() Replaces Manual CultureInfo Setup - v1.2.18

## Summary

The manual `CultureInfo` setup in `Program.cs` is replaced by a fluent `Server.UseCulture()` method. The `using System.Globalization;` import is no longer needed.

## What Changed

### Before

```csharp
using System.Globalization;
using Ivy;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
```

### After

```csharp
using Ivy;

var server = new Server();
server.UseCulture("en-US");
```

## How to Find Affected Code

Run a search for:

```regex
CultureInfo\.DefaultThreadCurrentCulture
```

Or:

```regex
using System\.Globalization;
```

## How to Refactor

1. Remove the `using System.Globalization;` line
2. Remove the `CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("...");` line
3. Add `server.UseCulture("en-US");` after the `var server = new Server();` line

## Verification

After refactoring, run:

```bash
dotnet build
```
