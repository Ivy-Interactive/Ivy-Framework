# Code Widget Renamed to CodeBlock - v1.2.16

## Summary

The `Code` widget has been renamed to `CodeBlock` to improve clarity and reduce ambiguity. Additionally, the corresponding extension class `CodeExtensions` has been renamed to `CodeBlockExtensions`.

## What Changed

### Before (v1.2.15 and earlier)

```csharp
new Code("var x = 1;", Languages.Csharp)
    .ShowCopyButton()
    .ShowLineNumbers()
```

### After (v1.2.16+)

```csharp
new CodeBlock("var x = 1;", Languages.Csharp)
    .ShowCopyButton()
    .ShowLineNumbers()
```

## How to Find Affected Code

Run a `dotnet build`.

Or search for this pattern in the codebase:

### Pattern 1: Code constructor

```regex
new Code\(
```

### Pattern 2: Code Extensions

```regex
CodeExtensions
```

## How to Refactor

Replace all instances of `Code` with `CodeBlock`, and `CodeExtensions` with `CodeBlockExtensions`.

**Before:**

```csharp
public override object? Build()
{
    return new Code("Console.WriteLine(\"Hello World\");", Languages.Csharp);
}
```

**After:**

```csharp
public override object? Build()
{
    return new CodeBlock("Console.WriteLine(\"Hello World\");", Languages.Csharp);
}
```

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
