# Rules of Hooks

Ivy hooks (functions starting with `Use...`) are a powerful feature that lets you use [state](./05_State.md) and other Ivy features without writing a class. However, hooks rely on a strict call order to function correctly. This page explains the rules you must follow and how to troubleshoot common errors.

## The Rules

### 1. Only Call Hooks at the Top Level

**Don't call hooks inside loops, conditions, or nested functions.** Instead, always use hooks at the top level of your component's `Build` method (or custom hook). By following this rule, you ensure that hooks are called in the same order each time a component renders. That's what allows Ivy to correctly preserve the state of hooks between multiple `Build` calls.

### 2. Only Call Hooks from Ivy Views

**Don't call hooks from regular C# functions.** Instead, you can:

- Call hooks from Ivy [Views](./02_Views.md) (inside `Build` method).
- Call hooks from custom hooks (functions starting with `Use...`).

## Troubleshooting

The **Ivy.Analyser** package automatically enforces these rules at compile time. Here are common errors you might encounter and how to fix them.

### IVYHOOK001: Hook used outside valid context

This error occurs when you try to use a hook outside of a View's `Build` method or another hook.

**❌ Bad:**

```csharp
public class MyService
{
    public void DoSomething()
    {
        var state = UseState(0); // Error! Not inside a View.
    }
}
```

**✅ Good:**

```csharp
public class MyView : ViewBase
{
    public override object Build()
    {
        var state = UseState(0); // OK
        return Text($"Value: {state.Value}");
    }
}
```

### IVYHOOK002: Conditional Hook Usage

This error occurs if a hook call is wrapped in an `if` statement. Hook calls must be unconditional.

**❌ Bad:**

```csharp
if (condition) {
    var state = UseState(0); // Error!
}
```

**✅ Good:**

```csharp
// Always call the hook, handle logic afterwards
var state = UseState(0);
if (condition) {
    // use state...
}
```

### IVYHOOK003: Hook inside a loop

Hooks cannot be called inside `for`, `foreach`, `while` loops.

**❌ Bad:**

```csharp
foreach (var item in items) {
    var state = UseState(item); // Error!
}
```

**✅ Good:**
create a separate View component for the item, and use the hook inside that component.

```csharp
foreach (var item in items) {
    // Use a sub-component instead
    new ItemView(item); 
}
```

### IVYHOOK005: Hook not at top of method

Hooks must be called before any other statements (like `return`, `throw`, etc) and mostly before other logic to ensure consistency. Use hooks at the very beginning of your method.

**❌ Bad:**

```csharp
public override object Build()
{
    if (User == null) return Text("Login required");

    var state = UseState(0); // Error! This hook might not run if the function returns early.
    // ...
}
```

**✅ Good:**

```csharp
public override object Build()
{
    var state = UseState(0); // Always runs

    if (User == null) return Text("Login required");
    // ...
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
