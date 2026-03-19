---
prepare: |
    var client = UseService<IClientProvider>();
searchHints:
  - components
  - viewbase
  - build
  - render
  - lifecycle
  - composition
---

# Views

Understand how Views work as the core building blocks of Ivy [apps](./10_Apps.md), similar to React components but written entirely in C#.

Views are the fundamental building blocks of Ivy apps. They are similar to React components, providing a way to encapsulate UI logic and [state management](../../03_Hooks/02_Core/03_UseState.md) in a reusable way. Every view inherits from `ViewBase` and implements a `Build()` method that returns the UI structure.

## Basic Usage

Here's a simple view that displays a greeting:

```csharp demo-below
Text.P("Hello, World!")
```

### The ViewBase Class

All views inherit from the abstract `ViewBase` class, which provides:

- **Build() method**: The core method that returns the UI structure
- **Lifecycle management**: Automatic disposal and cleanup
- **Hook access**: Built-in state management and effect [hooks](../../03_Hooks/02_RulesOfHooks.md)
- **Service injection**: Access to [application services](../../03_Hooks/02_Core/11_UseService.md)
- **Context management**: Shared data between parent and child views

### Build Method

The `Build()` method is the heart of every view. It can return:

- [Widgets](./03_Widgets.md) ([Button](../../02_Widgets/03_Common/01_Button.md), [Card](../../02_Widgets/03_Common/04_Card.md), Text, etc.)
- Other Views (for composition)
- [Layouts](./04_Layout.md) (to arrange multiple elements)
- Primitive types (strings, numbers)
- Collections (arrays, lists)
- `null` (to render nothing)

```csharp demo-tabs
public class FlexibleContentView : ViewBase
{
    public override object? Build()
    {
        var showContent = UseState(true);
        
        return Layout.Vertical()
            | new Button($"{(showContent.Value ? "Hide" : "Show")} Content", 
                onClick: _ => showContent.Set(!showContent.Value))
            | (showContent.Value ? "This content can be toggled!" : null);
    }
}
```

### State Management with Hooks

Views use React-like hooks for state management. The most common hook is [UseState()](../../03_Hooks/02_Core/03_UseState.md):

```csharp demo-tabs
public class CounterView : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        
        return new Card(
            Layout.Vertical().Align(Align.Center).Gap(4)
                | Text.P($"{count.Value}")
                | (Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("-", onClick: _ => count.Set(count.Value - 1))
                    | new Button("Reset", onClick: _ => count.Set(0))
                    | new Button("+", onClick: _ => count.Set(count.Value + 1)))
        ).Title("Counter");
    }
}
```

### Rules of Hooks

To ensure correct state management, Ivy hooks must follow specific rules.
Read the full guide on **[Rules of Hooks](../../03_Hooks/02_RulesOfHooks.md)** to learn more and troubleshoot common errors.

### State Initialization

You can initialize state in multiple ways:

```csharp
// Direct value
var count = UseState(0);

// Lazy initialization (called only once)
var expensiveData = UseState(() => ComputeExpensiveData());

// State that doesn't trigger rebuilds
var cache = UseState(new Dictionary<string, object>(), buildOnChange: false);
```

## Service Injection

Views can access application services using the [UseService<T>()](../../03_Hooks/02_Core/11_UseService.md) hook:

```csharp demo
new Button("Show Toast",
    onClick: _ => client.Toast("Hello from service!", "Service Demo"))
```

## Effects and Side Effects

Use `UseEffect()` for [side effects](../../03_Hooks/02_Core/04_UseEffect.md) like API calls, timers, or [subscriptions](../../03_Hooks/02_Core/10_UseSignal.md):

```csharp demo-below
public class TimerView : ViewBase
{
    public override object? Build()
    {
        var time = UseState(DateTime.Now);
        
        // Update time every second
        UseEffect(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                time.Set(DateTime.Now);
            }
        });
        
        return Text.P($"Current time: {time.Value:HH:mm:ss}");
    }
}
```

### View Composition

Views can be composed together to create complex UIs:

```csharp demo-below
Layout.Vertical()
    | Text.H2("Team Members")
    | new Card(
        Layout.Vertical()
            | Text.H4("Alice Smith")
            | Text.P("alice@example.com").Small().Color(Colors.Gray)
            | new Badge("Admin").Secondary()
    )
    | new Card(
        Layout.Vertical()
            | Text.H4("Bob Johnson")
            | Text.P("bob@example.com").Small().Color(Colors.Gray)
            | new Badge("User").Secondary()
    )
    | new Card(
        Layout.Vertical()
            | Text.H4("Carol Brown")
            | Text.P("carol@example.com").Small().Color(Colors.Gray)
            | new Badge("Manager").Secondary()
    )
```

### App Attribute

To make a view available as an app, use the `[App]` attribute:

```csharp
[App(icon: Icons.Home, title: "My App")]
public class MyApp : ViewBase
{
    public override object? Build()
    {
        return Text.H1("Welcome to My App!");
    }
}
```

The `[App]` attribute supports several properties:

- `icon`: [Icon](../../02_Widgets/01_Primitives/02_Icon.md) to display in [navigation](./09_Navigation.md)
- `title`: Display name (defaults to class name)
- `group`: [Navigation](./09_Navigation.md) path array for hierarchical organization
- `isVisible`: Whether to show in navigation
- `searchHints`: Alternative keywords for search discoverability
- `order`: Sort order within group
- `description`: Brief description of the app

For enhanced search discoverability, use `searchHints` to provide alternative keywords:

```csharp
[App(icon: Icons.TextCursorInput, 
     group: ["Widgets", "Inputs"], 
     searchHints: ["password", "textarea", "search", "email"])]
public class TextInputApp : ViewBase
{
    public override object? Build()
    {
        return Text.H1("Text Input Examples");
    }
}
```

### Sizing Views

Views (`ViewBase` subclasses) have `.Width()` and `.Height()` extension methods that automatically wrap the view in a layout:

```csharp
// Apply width directly — wraps in a layout under the hood
new MyView().Width(Size.Fraction(0.5f))

// You can also use WithLayout() explicitly
new MyView().WithLayout().Width(Size.Fraction(0.5f))
```

**Note:** These methods return a `LayoutView`, not the original view type. This means you cannot chain view-specific methods after `.Width()` or `.Height()` — apply sizing as the last step, or use `WithLayout()` explicitly if you need further layout configuration.

## Advanced Patterns

### Conditional Rendering

```csharp demo-tabs
public class ConditionalView : ViewBase
{
    public override object? Build()
    {
        var isLoggedIn = UseState(false);
        
        return Layout.Vertical()
            | new Button(isLoggedIn.Value ? "Logout" : "Login", 
                onClick: _ => isLoggedIn.Set(!isLoggedIn.Value))
            | (isLoggedIn.Value 
                ? Text.Success("Welcome back!")
                : Text.Muted("Please log in to continue"));
    }
}
```

### Dynamic Lists

```csharp demo-tabs
public class TodoApp : ViewBase
{
    public override object? Build()
    {
        var todos = UseState(new List<string>());
        var newTodo = UseState("");
        
        return Layout.Vertical()
            | new Card(
                Layout.Vertical()
                    | (Layout.Horizontal()
                        | newTodo.ToTextInput(placeholder: "Add a todo...").Width(Size.Grow())
                        | new Button("Add", onClick: _ => {
                            if (!string.IsNullOrWhiteSpace(newTodo.Value))
                            {
                                todos.Set([..todos.Value, newTodo.Value]);
                                newTodo.Set("");
                            }
                        }).Icon(Icons.Plus))
                    | todos.Value.Select((todo, index) => 
                        Layout.Horizontal()
                            | Text.Literal(todo).Width(Size.Grow())
                            | new Button("Remove", onClick: _ => {
                                var list = todos.Value.ToList();
                                list.RemoveAt(index);
                                todos.Set(list);
                            }).Icon(Icons.Trash).Variant(ButtonVariant.Outline).Small()
                    )
            ).Title("Todo List");
    }
}
```

### Simple User Profile Example

```csharp demo-tabs
new Card(
    Layout.Vertical()
            | new Avatar("John Doe", "JD")
        | (Layout.Horizontal()
            | Text.P("John Doe")
            | Text.P("42 posts"))
        | new Button("Follow")
            .Variant(ButtonVariant.Primary)
            .Width(Size.Full())
)
```
