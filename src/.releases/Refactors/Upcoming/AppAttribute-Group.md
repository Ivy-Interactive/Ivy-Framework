# Rename AppAttribute path to group

## Summary

Renamed the `path` parameter and `Path` property in `AppAttribute` to `group` and `Group` to improve clarity and prevent agent hallucinations.

## What Changed

- `AppAttribute` constructor parameter `path` (type `string[]`) renamed to `group`.
- `AppAttribute.Path` property renamed to `AppAttribute.Group`.

## Before

```csharp
[App(path: ["Widgets", "Inputs"])]
public class MyView : ViewBase { ... }

// or
var path = appAttribute.Path;
```

## After

```csharp
[App(group: ["Widgets", "Inputs"])]
public class MyView : ViewBase { ... }

// or
var group = appAttribute.Group;
```

## Migration Path

Replace all occurrences of `path:` named arguments in `[App]` attributes with `group:`.
Update any manual access to `AppAttribute.Path` property to `AppAttribute.Group`.

## Verification

After refactoring, run:

```bash
dotnet clean
dotnet build
```
