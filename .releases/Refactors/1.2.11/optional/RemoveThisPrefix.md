# Remove `this.` Prefix from Hooks - v1.2.11

## Summary

Hooks can now be called directly without the `this.` prefix. This improves code readability and reduces verbosity.

## What Changed

### Before
```csharp
var count = this.UseState(0);
this.UseEffect(() => {
    Console.WriteLine("Mounted");
});
```

### After
```csharp
var count = UseState(0);
UseEffect(() => {
    Console.WriteLine("Mounted");
});
```

## How to Find Affected Code

Search for usage of `this.Use` in your `View` classes.

### Pattern: Explicit `this.` usage
```regex
this\.Use\w+\(
```

## How to Refactor

1. Remove `this.` from hook calls.
2. Verify that the file compiles (ensure the base class or static imports are correct, though this is typically handled by the framework's base View class).

## Rationale

- **Conciseness**: Removes unnecessary boilerplate.
- **Modern Style**: Aligns with the cleaner syntax preferred in modern C# and similar framework patterns.
