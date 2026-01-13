# EffectTrigger.OnMount Rename - v1.2.11

## Summary

`EffectTrigger.AfterInit` has been renamed to `EffectTrigger.OnMount`.
## What Changed

### Before (v1.2.10 and earlier)
```csharp
  UseEffect(async () =>
        {
            ...
        }, [EffectTrigger.AfterInit()]);
```

### After (v1.2.11+)
```csharp
  UseEffect(async () =>
        {
            ...
        }, [EffectTrigger.OnMount()]);
```

## How to Find Affected Code

Run a `dotnet build`.