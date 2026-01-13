# BladeService Rename - v1.2.11

## Summary

`IBladeController` has been renamed to `IBladeService` and `BladeController` has been renamed to `BladeService` for consistency with other service naming conventions in the framework.

## What Changed

### Before (v1.2.10 and earlier)
```csharp
var blades = UseContext<IBladeController>();
```

### After (v1.2.11+)
```csharp
var blades = UseContext<IBladeService>();
```

## How to Find Affected Code

Run a `dotnet build`.

Or search for these patterns in the codebase:

### Pattern 1: IBladeController usage
```regex
IBladeController
```

### Pattern 2: BladeController usage
```regex
BladeController
```

### Pattern 3: UseContext with blade controller
```regex
UseContext<IBladeController>
```