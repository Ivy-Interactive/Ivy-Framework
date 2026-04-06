# Ivy.Analyser

A Roslyn-based .NET analyzer that enforces proper usage of Ivy framework hooks, similar to React's "Rules of Hooks".

## Overview

This analyzer ensures that Ivy hook functions (such as `UseState`, `UseEffect`, etc.) are used **only directly inside the `Build()` method** of classes that inherit from `ViewBase`, following the same rules as React's Rules of Hooks. Violating these rules may cause runtime errors, so they are enforced at compile time.

The analyzer enforces that hooks must be called:

- ✅ At the top level of the `Build()` method
- ✅ Unconditionally (not inside if statements, loops, or switch statements)
- ✅ In the same order on every render

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

The analyzer will report errors and warnings for these violations:

#### 1. Hook calls inside lambdas (Error IVYHOOK001)

```csharp
public override object? Build()
{
    var handler = (Event<Button> e) =>
    {
        var s = UseState(false); // ❌ Error IVYHOOK001
    };
    return new Button().OnClick(handler);
}
```

#### 2. Hook calls inside local functions (Error IVYHOOK001)

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

#### 3. Hook calls in other methods (Error IVYHOOK001)

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

#### 4. Hook calls in anonymous methods (Error IVYHOOK001)

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

#### 5. Hook calls conditionally (Warning IVYHOOK002)

```csharp
public override object? Build()
{
    if (someCondition)
    {
        var state = UseState(false); // ⚠️ Warning IVYHOOK002
    }
    
    var result = condition ? UseState(0) : UseState(1); // ⚠️ Warning IVYHOOK002 (ternary)
    
    return new Button();
}
```

#### 6. Hook calls in loops (Warning IVYHOOK003)

```csharp
public override object? Build()
{
    for (int i = 0; i < 10; i++)
    {
        var state = UseState(i); // ⚠️ Warning IVYHOOK003
    }
    
    foreach (var item in items)
    {
        var state = UseState(item); // ⚠️ Warning IVYHOOK003
    }
    
    while (condition)
    {
        var state = UseState(false); // ⚠️ Warning IVYHOOK003
    }
    
    return new Button();
}
```

#### 7. Hook calls in switch statements (Warning IVYHOOK004)

```csharp
public override object? Build()
{
    switch (value)
    {
        case 1:
            var state = UseState(false); // ⚠️ Warning IVYHOOK004
            break;
    }
    
    return new Button();
}
```

#### 8. Hook calls not at the top of Build() method (Warning IVYHOOK005)

```csharp
public override object? Build()
{
    var x = SomeMethod(); // Non-hook statement
    var state = UseState(false); // ⚠️ Warning IVYHOOK005 - hook must be at the top
    
    int y = 10; // Another non-hook statement
    var state2 = UseState(0); // ⚠️ Warning IVYHOOK005
    
    return new Button();
}
```

## Hook Detection

The analyzer automatically detects hooks by their naming convention:

- Method name must start with `Use`
- The fourth character must be an uppercase letter

**Examples:**
- ✅ `UseState`, `UseEffect`, `UseCustomHook`, `UseMyFeature`
- ❌ `Use`, `Useless`, `useState`, `useEffect`

This means any custom hooks you create following the `UseX` pattern will be automatically validated by the analyzer.

## Diagnostic Details

### IVYHOOK001 - Invalid Hook Usage (Error)

**Severity:** Error  
**Message:** `Ivy hook '{hookName}' can only be used directly inside the Build() method`  
**Description:** Hooks must be called directly inside the Build() method, not inside lambdas, local functions, or other methods.

### IVYHOOK002 - Hook Called Conditionally (Warning)

**Severity:** Warning  
**Message:** `Ivy hook '{hookName}' cannot be called conditionally. Hooks must be called in the same order on every render.`  
**Description:** Hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside if statements, ternary operators, or other conditional logic.

### IVYHOOK003 - Hook Called in Loop (Warning)

**Severity:** Warning  
**Message:** `Ivy hook '{hookName}' cannot be called inside a loop. Hooks must be called in the same order on every render.`  
**Description:** Hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside for, foreach, while, or do-while loops.

### IVYHOOK004 - Hook Called in Switch Statement (Warning)

**Severity:** Warning  
**Message:** `Ivy hook '{hookName}' cannot be called inside a switch statement. Hooks must be called in the same order on every render.`  
**Description:** Hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside switch statements.

### IVYHOOK005 - Hook Not at Top of Build Method (Warning)

**Severity:** Warning  
**Message:** `Ivy hook '{hookName}' must be called at the top of the Build() method, before any other statements.`  
**Description:** All hooks must be called at the very top of the Build() method, before any other non-hook statements. This ensures hooks are called in a consistent order on every render.

### IVYSERVICE001 - UseService Should Use Interface (Warning)

**Severity:** Warning  
**Message:** `UseService<{ConcreteType}> should use interface I{ConcreteType} instead. Using concrete types breaks testability and violates dependency inversion.`  
**Description:** When calling UseService<T>(), always prefer interface types over concrete types when an interface is available. This ensures proper dependency injection, testability, and adherence to SOLID principles.

#### ✅ Valid Usage

```csharp
public override object? Build()
{
    var config = UseService<IConfigService>();     // ✅ Valid - using interface
    var job = UseService<IJobService>();           // ✅ Valid - using interface
    var git = UseService<IGitService>();           // ✅ Valid - using interface
    
    return new Button();
}
```

#### ❌ Invalid Usage

```csharp
public override object? Build()
{
    var config = UseService<ConfigService>();     // ⚠️ Warning IVYSERVICE001 - use IConfigService
    var job = UseService<JobService>();           // ⚠️ Warning IVYSERVICE001 - use IJobService
    
    return new Button();
}
```

**Note:** This warning only appears when a corresponding interface (prefixed with 'I') exists in the same namespace. If no interface exists, no warning is shown.

#### Quick Fix

The analyzer includes a CodeFixProvider that can automatically fix IVYSERVICE001 warnings with a single click.

**How to use:**
1. Place your cursor on the warning (red/yellow squiggle)
2. Trigger the quick action menu:
   - **Visual Studio:** Press `Ctrl+.` or click the lightbulb/screwdriver icon
   - **VS Code:** Press `Ctrl+.` or click the lightbulb icon
   - **Rider:** Press `Alt+Enter` or click the lightbulb icon
3. Select **"Use interface I{ConcreteType}"** from the menu
4. The concrete type will be automatically replaced with its interface

**Example:**
```csharp
// Before fix:
var config = UseService<ConfigService>();  // ⚠️ Warning IVYSERVICE001

// After applying quick fix:
var config = UseService<IConfigService>(); // ✅ Fixed
```

**Fix All:**
You can apply the fix to multiple violations at once using "Fix All" in Visual Studio:
- **Fix All in Document:** Fixes all IVYSERVICE001 warnings in the current file
- **Fix All in Project:** Fixes all IVYSERVICE001 warnings in the project
- **Fix All in Solution:** Fixes all IVYSERVICE001 warnings in the solution

**Note:** The quick fix only appears when the corresponding interface (prefixed with 'I') exists in the same namespace. If no interface exists, the warning will still appear but no automatic fix will be offered.

## Configuration

The analyzer runs automatically when you build your project. No additional configuration is needed.

### Suppressing Warnings (Not Recommended)

If you need to suppress the analyzer for specific cases, you can use:

```csharp
#pragma warning disable IVYHOOK001  // Suppress error
#pragma warning disable IVYHOOK002  // Suppress conditional warning
#pragma warning disable IVYHOOK003  // Suppress loop warning
#pragma warning disable IVYHOOK004  // Suppress switch warning
#pragma warning disable IVYHOOK005  // Suppress not-at-top warning
#pragma warning disable IVYSERVICE001  // Suppress UseService interface warning
var state = UseState(false); // This will not trigger the analyzer
#pragma warning restore IVYHOOK001
#pragma warning restore IVYHOOK002
#pragma warning restore IVYHOOK003
#pragma warning restore IVYHOOK004
#pragma warning restore IVYHOOK005
#pragma warning restore IVYSERVICE001
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
