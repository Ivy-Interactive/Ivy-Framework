# BladeContext Rename

## Summary

`IBladeService` has been renamed to `IBladeContext` and `BladeService` has been renamed to `BladeContext` to clarify that this is a context service accessed via `UseContext<T>()`, not a DI service.

## What Changed

### Before
```csharp
var blades = UseContext<IBladeService>();
```

### After
```csharp
var blades = UseContext<IBladeContext>();
```

## How to Find Affected Code

Run `dotnet build`.

Or search for:
- `IBladeService`
- `BladeService`
