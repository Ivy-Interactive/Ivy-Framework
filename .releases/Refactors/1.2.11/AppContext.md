# AppArgs Rename - v1.2.11

## Summary

`AppArgs` has been renamed to `AppContext` for consistency with other context naming conventions in the framework.

## What Changed

### Before (v1.2.10 and earlier)
```csharp
var args = UseService<AppArgs>();
var url = $"{args.Scheme}://{args.Host}/path";
```

### After (v1.2.11+)
```csharp
var args = UseService<Ivy.Apps.AppContext>();
var url = $"{args.Scheme}://{args.Host}/path";
```

> **Note:** Use the fully qualified name `Ivy.Apps.AppContext` to avoid ambiguity with `System.AppContext`.

## How to Find Affected Code

Run a `dotnet build`.

Or search for these patterns in the codebase:

### Pattern 1: AppArgs usage
```regex
AppArgs
```

### Pattern 2: UseService with AppArgs
```regex
UseService<AppArgs>
```

## How to Refactor

### Basic Pattern

**Before:**
```csharp
public override object? Build()
{
    var args = UseService<AppArgs>();
    var link = $"{args.Scheme}://{args.Host}/links/{secret}";
    // ...
}
```

**After:**
```csharp
public override object? Build()
{
    var args = UseService<Ivy.Apps.AppContext>();
    var link = $"{args.Scheme}://{args.Host}/links/{secret}";
    // ...
}
```

### With Using Alias (Alternative)

If you have many usages in a file, you can add a using alias:

```csharp
using AppContext = Ivy.Apps.AppContext;

public class MyView : ViewBase
{
    public override object? Build()
    {
        var args = UseService<AppContext>();
        // ...
    }
}
```

## Key Refactoring Rules

1. **Simple rename**: Replace all occurrences of `AppArgs` with `Ivy.Apps.AppContext`

2. **Fully qualify**: Use `Ivy.Apps.AppContext` to avoid conflict with `System.AppContext`

3. **No API changes**: The class properties and methods remain the same - only the name changed

## Quick Refactor Commands

### Using IDE
- **Visual Studio**: Find and Replace (Ctrl+Shift+H) → `AppArgs` → `Ivy.Apps.AppContext`
- **Rider**: Find and Replace in Path (Ctrl+Shift+R)
- **VS Code**: Search and Replace in Files (Ctrl+Shift+H)

### Using Command Line
```bash
# Find all occurrences
grep -r "AppArgs" --include="*.cs"

# Replace (use with caution)
find . -name "*.cs" -exec sed -i 's/AppArgs/Ivy.Apps.AppContext/g' {} \;
```

## Verification

After refactoring, run:
```bash
dotnet build
```

All usages should compile without errors.

## Benefits of New Naming

1. **Consistency**: Matches the `AppContext` pattern used elsewhere in the framework
2. **Clarity**: The "Context" suffix better reflects that this provides contextual information about the running application
3. **Discoverability**: Easier to find alongside other context types like `IViewContext`
