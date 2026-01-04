# BladeService Rename - v1.2.6

## Summary

`IBladeController` has been renamed to `IBladeService` and `BladeController` has been renamed to `BladeService` for consistency with other service naming conventions in the framework.

## What Changed

### Before (v1.2.5 and earlier)
```csharp
var blades = UseContext<IBladeController>();
```

### After (v1.2.6+)
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

## How to Refactor

### Basic Pattern

**Before:**
```csharp
public override object? Build()
{
    var blades = UseContext<IBladeController>();

    return new Button("Open", onClick: _ =>
        blades.Push(this, new DetailView(), "Details"));
}
```

**After:**
```csharp
public override object? Build()
{
    var blades = UseContext<IBladeService>();

    return new Button("Open", onClick: _ =>
        blades.Push(this, new DetailView(), "Details"));
}
```

### Type Parameters and Constraints

**Before:**
```csharp
public class MyComponent<T> where T : IBladeController
{
    private readonly IBladeController _controller;
}
```

**After:**
```csharp
public class MyComponent<T> where T : IBladeService
{
    private readonly IBladeService _controller;
}
```

### Method Parameters

**Before:**
```csharp
public void Navigate(IBladeController blades, IView target)
{
    blades.Push(this, target, "Title");
}
```

**After:**
```csharp
public void Navigate(IBladeService blades, IView target)
{
    blades.Push(this, target, "Title");
}
```

## Key Refactoring Rules

1. **Simple rename**: Replace all occurrences of `IBladeController` with `IBladeService`

2. **Simple rename**: Replace all occurrences of `BladeController` with `BladeService`

3. **No API changes**: The interface methods remain the same - only the name changed

4. **Find and replace**: This is a straightforward find-and-replace refactoring

## Quick Refactor Commands

### Using IDE
- **Visual Studio**: Right-click on `IBladeController` → Rename (F2) → `IBladeService`
- **Rider**: Refactor → Rename (Shift+F6)
- **VS Code**: F2 on the type name

### Using Command Line
```bash
# Find all occurrences
grep -r "IBladeController" --include="*.cs"
grep -r "BladeController" --include="*.cs"

# Replace (use with caution)
find . -name "*.cs" -exec sed -i 's/IBladeController/IBladeService/g' {} \;
find . -name "*.cs" -exec sed -i 's/BladeController/BladeService/g' {} \;
```

## Verification

After refactoring, run:
```bash
dotnet build
```

All usages should compile without errors.

## Benefits of New Naming

1. **Consistency**: Matches the `IQueryService`, `IAuthService`, and other service naming patterns
2. **Clarity**: The "Service" suffix better reflects that this is a dependency-injected service
3. **Discoverability**: Easier to find among other services when using IDE autocomplete
