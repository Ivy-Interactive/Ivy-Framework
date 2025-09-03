# State

<Ingress>
Master reactive state management in Ivy using hooks like UseState, UseSignal, and UseEffect to build dynamic, responsive applications.
</Ingress>

State management is a fundamental concept in Ivy that allows you to handle and update data within your views. Ivy provides several mechanisms for managing state, each suited for different use cases.

## Basic Usage

The `UseState` hook is the primary way to create reactive state in Ivy views:

```csharp demo-below
public class CounterApp : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var name = UseState("World");
        var client = UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical(
                Text.Literal($"Hello, {name.Value}!"),
                Text.Literal($"Count: {count.Value}"),
                Layout.Horizontal(
                    new Button("Increment", _ => count.Set(count.Value + 1)),
                    new Button("Decrement", _ => count.Set(count.Value - 1)),
                    new Button("Reset", _ => count.Set(0))
                ),
                new Separator(),
                Layout.Horizontal(
                    name.ToTextInput().Placeholder("Your Name"),
                    new Button("Greet", _ => client.Toast($"Hello, {name.Value}!", "Greeting"))
                )
            )
        ).Title("Counter Demo");
    }
}
```

### State with Factory Functions

For complex initialization or when you need to defer object creation, use factory functions with UseState. This pattern is useful for expensive computations, dependency injection, and lazy loading:

```csharp demo-tabs
public class FactoryStateDemo : ViewBase
{
    public override object? Build()
    {
        var expensiveData = UseState(() => ComputeExpensiveData());
        
        var service = UseState(() => new DataService(GetConfig()));
        
        return Layout.Vertical(
            Text.Large("Factory Functions Demo"),
            Text.Literal($"Data: {expensiveData.Value}"),
            Text.Literal($"Service: {service.Value.Status}"),
            new Button("Refresh", _ => expensiveData.Set(ComputeExpensiveData()))
        );
    }
    
    private string ComputeExpensiveData() => $"Computed at {DateTime.Now:HH:mm:ss}";
    private string GetConfig() => "production";
}

public class DataService
{
    public string Status { get; } = "Connected";
    public DataService(string config) { }
}
```

### State Types and Patterns

Ivy supports various state types including primitives, collections, complex objects, and nullable types. Each type has specific update patterns and considerations for optimal performance and maintainability:

```csharp demo-tabs
public class StatePatternsDemo : ViewBase
{
    public override object? Build()
    {
        // Primitive types
        var count = UseState(0);
        var name = UseState("Guest");
        
        // Collections
        var items = UseState(() => new List<string> { "Item 1", "Item 2" });
        
        // Complex objects
        var user = UseState(() => new User { Name = "John", Age = 25 });
        
        // Nullable types
        var selectedItem = UseState(() => (string?)null);
        
        return Layout.Vertical(
            Text.Large("State Types & Patterns"),
            
            // Primitive state
            Layout.Horizontal(
                new Button($"Count: {count.Value}", _ => count.Set(count.Value + 1)),
                new Button(name.Value, _ => name.Set("User " + Random.Shared.Next(100)))
            ),
            
            // Collection state
            Layout.Horizontal(
                new Button("Add Item", _ => {
                    var newList = new List<string>(items.Value) { $"Item {items.Value.Count + 1}" };
                    items.Set(newList);
                }),
                new Button("Clear", _ => {
                    var emptyList = new List<string>();
                    items.Set(emptyList);
                })
            ),
            
            // Object state
            Layout.Horizontal(
                new Button($"User: {user.Value.Name}", _ => {
                    var newUser = new User { Name = "Jane", Age = 30 };
                    user.Set(newUser);
                })
            ),
            
            // Nullable state
            Layout.Horizontal(
                new Button("Set Item", _ => selectedItem.Set("Selected Item")),
                new Button("Clear Item", _ => {
                    string? nullValue = null;
                    selectedItem.Set(nullValue);
                })
            ),
            
            new Separator(),
            Text.Literal($"Items: {string.Join(", ", items.Value)}"),
            Text.Literal($"Selected: {selectedItem.Value ?? "None"}")
        );
    }
}

public class User
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
```

### State Updates

State updates in Ivy are handled through the Set method, which can accept direct values or computed values. Updates trigger automatic re-renders of the affected components, ensuring the UI stays synchronized with the current state:

```csharp demo-tabs
public class StateUpdatesDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var text = UseState("Hello");
        var items = UseState(() => new List<string> { "Item 1", "Item 2" });
        
        return Layout.Vertical(
            Text.Large("State Updates Demo"),
            
            // Direct updates
            Layout.Horizontal(
                new Button($"Count: {count.Value}", _ => count.Set(count.Value + 1)),
                new Button("Reset", _ => count.Set(0))
            ),
            
            // String updates
            Layout.Horizontal(
                text.ToTextInput("Enter text"),
                new Button("Clear", _ => text.Set("")),
                new Button("Uppercase", _ => text.Set(text.Value.ToUpper()))
            ),
            
            // Collection updates
            Layout.Horizontal(
                new Button("Add Item", _ => {
                    var newItems = new List<string>(items.Value) { $"Item {items.Value.Count + 1}" };
                    items.Set(newItems);
                }),
                new Button("Clear", _ => items.Set(new List<string>()))
            ),
            
            Text.Literal($"Text: {text.Value}"),
            Text.Literal($"Items: {string.Join(", ", items.Value)}")
        );
    }
}
```

### State in Forms

[Forms](../../01_Onboarding/02_Concepts/Forms.md) in Ivy use state variables bound to input widgets, allowing for real-time validation, live previews, and easy form submission handling. The ToTextInput, ToNumberInput, and ToBoolInput extensions provide seamless state binding:

```csharp demo-tabs
public class FormStateDemo : ViewBase
{
    public override object? Build()
    {
        var name = UseState("");
        var email = UseState("");
        var age = UseState(18);
        var isSubscribed = UseState(false);
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
            Text.Large("Form State Demo"),
            
            name.ToTextInput("Name").Placeholder("Enter your name"),
            email.ToTextInput("Email").Placeholder("Enter your email"),
            age.ToNumberInput("Age").Min(0).Max(120),
            isSubscribed.ToBoolInput("Subscribe to newsletter"),
            
            new Separator(),
            
            Layout.Horizontal(
                new Button("Submit", _ => client.Toast($"Hello, {name.Value}!", "Greeting")),
                new Button("Clear", _ => {
                    name.Set("");
                    email.Set("");
                    age.Set(18);
                    isSubscribed.Set(false);
                })
            ),
            
            Text.Literal($"Preview: {name.Value} ({email.Value}) - Age: {age.Value}")
        );
    }
}
```

### State with Effects

The [UseEffect](../../01_Onboarding/02_Concepts/Effects.md) hook allows you to perform side effects when state changes, such as updating derived state, making API calls, or triggering other actions. Effects run automatically when their dependencies change:

```csharp demo-tabs
public class EffectsStateDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var lastUpdate = UseState(DateTime.Now);
        var isEven = UseState(false);
        
        // Effect that runs when count changes
        UseEffect(() => {
            lastUpdate.Set(DateTime.Now);
            isEven.Set(count.Value % 2 == 0);
        }, [count]);
        
        return Layout.Vertical(
            Text.Large("State with Effects Demo"),
            
            Layout.Horizontal(
                new Button($"Count: {count.Value}", _ => count.Set(count.Value + 1)),
                new Button("Reset", _ => count.Set(0))
            ),
            
            new Separator(),
            
            Text.Literal($"Last Update: {lastUpdate.Value:HH:mm:ss}"),
            Text.Literal($"Is Even: {(isEven.Value ? "Yes" : "No")}")
        );
    }
}
```
