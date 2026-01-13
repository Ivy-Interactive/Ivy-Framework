# UseStatic Migrated to UseRef - v1.2.11

## Summary

The `UseStatic` hook has been renamed to `UseRef` to better align with industry-standard naming conventions for hooks that provide persistent, non-reactive references.

## What Changed

### Before
```csharp
// UseStatic returned the object directly
var myRef = this.UseStatic(() => new MyHeavyObject());
myRef.Property = "new value";
```

### After
```csharp
// UseRef returns IState<T>, requiring .Value access
var myRef = this.UseRef(() => new MyHeavyObject());
myRef.Value.Property = "new value";
```

## How to Find Affected Code

Search for usages of `.UseStatic` in your `View` classes.

### Pattern: UseStatic usage
```regex
\.UseStatic\(
```

## How to Refactor

1. Rename `UseStatic` to `UseRef`.
2. Update usages of the returned variable to access `.Value`.

### Example

**Before:**
```csharp
var service = Context.UseStatic(() => new MyService());
service.DoSomething();
```

**After:**
```csharp
var service = Context.UseRef(() => new MyService());
service.Value.DoSomething();
```

## Rationale

- **Consistency**: `UseRef` is the standard name in React and other hook-based frameworks.
- **Clarity**: `Static` often implies something global or type-level. `Ref` describes a reference attached to a view instance.
