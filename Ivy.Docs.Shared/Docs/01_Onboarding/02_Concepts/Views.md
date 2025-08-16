---
prepare: |
    var client = this.UseService<IClientProvider>();
---

# Views

<Ingress>
Understand how Views work as the core building blocks of Ivy apps, similar to React components but written entirely in C#.
</Ingress>

Views are the fundamental building blocks of Ivy apps. They are similar to React components, providing a way to encapsulate UI logic and state management in a reusable way. Every view inherits from `ViewBase` and implements a `Build()` method that returns the UI structure.

## Basic Usage

Here's a simple view that displays a greeting:

```csharp demo-below 
public class GreetingView : ViewBase
{
    public override object? Build()
    {
        return Text.P("Hello, World!");
    }
}
```

```csharp demo-tabs 
Text.P("Hello from a View!")
```

### The ViewBase Class

All views inherit from the abstract `ViewBase` class, which provides:

- **Build() method**: The core method that returns the UI structure
- **Lifecycle management**: Automatic disposal and cleanup
- **Hook access**: Built-in state management and effect hooks
- **Service injection**: Access to application services
- **Context management**: Shared data between parent and child views

### Build Method

The `Build()` method is the heart of every view. It can return:

- Widgets (Button, Card, Text, etc.)
- Other Views (for composition)
- Layouts (to arrange multiple elements)
- Primitive types (strings, numbers)
- Collections (arrays, lists)
- `null` (to render nothing)

```csharp demo-tabs 
public class FlexibleContentView : ViewBase
{
    public override object? Build()
    {
        var showContent = this.UseState(true);
        
        return Layout.Vertical()
            | new Button($"{(showContent.Value ? "Hide" : "Show")} Content", 
                onClick: _ => showContent.Set(!showContent.Value))
            | (showContent.Value ? "This content can be toggled!" : null);
    }
}
```

### State Management with Hooks

Views use React-like hooks for state management. The most common hook is `UseState()`:

```csharp demo-tabs 
public class CounterView : ViewBase
{
    public override object? Build()
    {
        var count = this.UseState(0);
        
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

### State Initialization

You can initialize state in multiple ways:

```csharp
// Direct value
var count = this.UseState(0);

// Lazy initialization (called only once)
var expensiveData = this.UseState(() => ComputeExpensiveData());

// State that doesn't trigger rebuilds
var cache = this.UseState(new Dictionary<string, object>(), buildOnChange: false);
```

## Service Injection

Views can access application services using the `UseService<T>()` hook:

```csharp demo
new Button("Show Toast",
    onClick: _ => client.Toast("Hello from service!", "Service Demo"))
```

## Effects and Side Effects

Use `UseEffect()` for side effects like API calls, timers, or subscriptions:

```csharp demo-below
public class TimerView : ViewBase
{
    public override object? Build()
    {
        var time = this.UseState(DateTime.Now);
        
        // Update time every second
        this.UseEffect(async () =>
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
            | Text.Small("alice@example.com").Color(Colors.Gray)
            | new Badge("Admin").Secondary()
    )
    | new Card(
        Layout.Vertical()
            | Text.H4("Bob Johnson")
            | Text.Small("bob@example.com").Color(Colors.Gray)
            | new Badge("User").Secondary()
    )
    | new Card(
        Layout.Vertical()
            | Text.H4("Carol Brown")
            | Text.Small("carol@example.com").Color(Colors.Gray)
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

- `icon`: Icon to display in navigation
- `title`: Display name (defaults to class name)
- `path`: Navigation path array
- `isVisible`: Whether to show in navigation

## Advanced Patterns

### Conditional Rendering

```csharp demo-tabs 
public class ConditionalView : ViewBase
{
    public override object? Build()
    {
        var isLoggedIn = this.UseState(false);
        
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
        var todos = this.UseState(new List<string>());
        var newTodo = this.UseState("");
        
        return Layout.Vertical()
            | new Card(
                Layout.Vertical()
                    | Layout.Horizontal()
                        | newTodo.ToTextInput(placeholder: "Add a todo...").Width(Size.Grow())
                        | new Button("Add", onClick: _ => {
                            if (!string.IsNullOrWhiteSpace(newTodo.Value))
                            {
                                todos.Set([..todos.Value, newTodo.Value]);
                                newTodo.Set("");
                            }
                        }).Icon(Icons.Plus)
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
        | Layout.Horizontal()
            | new Avatar("John Doe", "JD")
            | Layout.Vertical()
                | Text.P("John Doe")
                | Text.Small("42 posts")
        | new Button("Follow")
            .Variant(ButtonVariant.Primary)
            .Width(Size.Full())
)
```
