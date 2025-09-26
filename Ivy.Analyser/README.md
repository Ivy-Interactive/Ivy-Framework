# Ivy.Analyser

A Roslyn-based .NET analyzer that enforces proper usage of Ivy framework hooks, similar to React's "Rules of Hooks".

## Overview

This analyzer ensures that Ivy hook functions (such as `UseState`, `UseEffect`, etc.) are used **only directly inside the `Build()` method** of classes that inherit from `ViewBase`. Violating these rules may cause runtime errors, so they are enforced at compile time.

## Installation

### Using Package Manager Console
```powershell
Install-Package Ivy.Analyser
```

### Using .NET CLI
```bash
dotnet add package Ivy.Analyser
```

### Using PackageReference
Add the following to your `.csproj` file:
```xml
<PackageReference Include="Ivy.Analyser" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

## Rules Enforced

### ✅ Valid Usage
Hook functions must be invoked **directly inside the top level of the `Build()` method**:

```csharp
public class MyView : ViewBase
{
    public override object? Build()
    {
        var state = UseState(false);        // ✅ Valid
        UseEffect(() => { /* ... */ });     // ✅ Valid
        var memo = UseMemo(() => 42);        // ✅ Valid
        
        return new Button();
    }
}
```

### ❌ Invalid Usage

The analyzer will report **IVYHOOK001** errors for these violations:

#### 1. Hook calls inside lambdas
```csharp
public override object? Build()
{
    var handler = (Event<Button> e) =>
    {
        var s = UseState(false); // ❌ Error IVYHOOK001
    };
    return new Button().HandleClick(handler);
}
```

#### 2. Hook calls inside local functions
```csharp
public override object? Build()
{
    void LocalFunction()
    {
        var s = UseState(false); // ❌ Error IVYHOOK001
    }
    
    LocalFunction();
    return new Button();
}
```

#### 3. Hook calls in other methods
```csharp
public override object? Build()
{
    Initialize();
    return new Button();
}

private void Initialize()
{
    var s = UseState(false); // ❌ Error IVYHOOK001
}
```

#### 4. Hook calls in anonymous methods
```csharp
public override object? Build()
{
    Action action = delegate()
    {
        var s = UseState(false); // ❌ Error IVYHOOK001
    };
    
    return new Button();
}
```

## Supported Hooks

The analyzer detects improper usage of these hook functions:

- `UseState`
- `UseEffect` 
- `UseMemo`
- `UseRef`
- `UseContext`
- `UseCallback`

## Error Details

**Diagnostic ID:** `IVYHOOK001`  
**Severity:** Error  
**Message:** `Ivy hook '{hookName}' can only be used directly inside the Build() method`

## Configuration

The analyzer runs automatically when you build your project. No additional configuration is needed.

### Suppressing Warnings (Not Recommended)

If you need to suppress the analyzer for specific cases, you can use:

```csharp
#pragma warning disable IVYHOOK001
var state = UseState(false); // This will not trigger the analyzer
#pragma warning restore IVYHOOK001
```

However, this is **not recommended** as it may lead to runtime errors.

## IDE Integration

The analyzer works with:
- Visual Studio 2019/2022
- Visual Studio Code with C# extension
- JetBrains Rider
- Any IDE that supports Roslyn analyzers

Violations will be highlighted in your IDE with red squiggly lines and appear in the Error List.

## Building from Source

```bash
git clone <repository-url>
cd Ivy.Analyser
dotnet build
dotnet test
dotnet pack
```

## License

This project is licensed under the same license as the Ivy Framework.

## Related

- [Ivy Framework](https://github.com/Ivy-Interactive/Ivy-Framework)
- [React Rules of Hooks](https://reactjs.org/docs/hooks-rules.html)
- [Roslyn Analyzers Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)