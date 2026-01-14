---
searchHints:
  - args
  - useargs
  - parameters
  - route-parameters
  - navigation-args
  - component-args
---

# Args

<Ingress>
The `UseArgs` [hook](../02_RulesOfHooks.md) provides access to arguments passed to a [component](../../../01_Onboarding/02_Concepts/02_Views.md), such as route parameters or navigation arguments.
</Ingress>

## Overview

The `UseArgs` [hook](../02_RulesOfHooks.md) allows you to access component arguments:

- **Navigation Arguments** - Retrieve arguments passed during [navigation](./23_UseNavigation.md)
- **Type Safety** - Strongly typed argument access with compile-time checking
- **JSON Serialization** - Arguments are automatically serialized and deserialized
- **Optional Arguments** - Returns null if arguments are not available

<Callout type="Tip">
`UseArgs` is the primary way to pass data between [components](../../../01_Onboarding/02_Concepts/02_Views.md) during navigation. Arguments are serialized as JSON, making them perfect for passing simple data structures like records or DTOs.
</Callout>

## Basic Usage

### Defining Argument Types

Arguments are typically defined as records or classes:

```csharp
public record UserProfileArgs(int UserId, string Tab = "overview");

public record ProductDetailArgs(string ProductId, bool ShowReviews = false);

public class SearchArgs
{
    public string Query { get; set; } = "";
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
}
```

### Passing Arguments During Navigation

Use the [navigation hook](./23_UseNavigation.md) to pass arguments to target components:

```csharp
public class DashboardView : ViewBase
{
    public override object? Build()
    {
        var navigator = UseNavigation();
        
        return Layout.Vertical(
            new Button("View User Profile")
                .HandleClick(() => 
                {
                    navigator.Navigate(typeof(UserProfileApp), 
                        new UserProfileArgs(123, "details"));
                }),
            
            new Button("Search Products")
                .HandleClick(() => 
                {
                    navigator.Navigate(typeof(ProductSearchApp),
                        new SearchArgs 
                        { 
                            Query = "laptop",
                            Category = "electronics",
                            Page = 1
                        });
                })
        );
    }
}
```

### Receiving Arguments

Use `UseArgs` to retrieve arguments in the target component:

```csharp
public class UserProfileApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<UserProfileArgs>();
        
        if (args == null)
        {
            return Text.Literal("No user ID provided");
        }
        
        return Layout.Vertical(
            Text.Heading($"User Profile: {args.UserId}"),
            Text.Literal($"Tab: {args.Tab}")
        );
    }
}
```

## How Args Work

### Argument Flow

```mermaid
sequenceDiagram
    participant S as Source Component
    participant N as Navigator
    participant AC as AppContext
    participant T as Target Component
    
    Note over S,T: Navigation with Arguments
    S->>N: Navigate(typeof(App), args)
    N->>AC: Serialize args to JSON
    AC->>AC: Store JSON in AppContext
    AC->>T: Component loads
    T->>AC: UseArgs<T>()
    AC->>AC: Deserialize JSON to T
    AC-->>T: Return args or null
```

### Argument Serialization

Arguments are automatically serialized to JSON when passed and deserialized when accessed:

```csharp
// Arguments are serialized to JSON
var args = new UserProfileArgs(123, "details");
// Becomes: {"UserId":123,"Tab":"details"}

// When UseArgs is called, JSON is deserialized back
var receivedArgs = UseArgs<UserProfileArgs>();
// Returns: UserProfileArgs { UserId = 123, Tab = "details" }
```

## When to Use Args

### Use Args For

- **Navigation Data** - Passing data when navigating between components
- **Deep Linking** - Supporting URLs with query parameters
- **Component Initialization** - Providing initial state to components
- **Simple Data Transfer** - Passing small, serializable data structures

### Use [State](./03_State.md) or [Context](./12_Context.md) Instead For

- **Component State** - Data that changes within a component
- **Shared Component Data** - Data shared across a component tree
- **Complex Objects** - Objects with circular references or non-serializable types
- **Real-time Updates** - Data that needs to update reactively

## Examples

### User Profile with Tab Navigation

```csharp
public record UserProfileArgs(int UserId, string Tab = "overview");

public class UserListView : ViewBase
{
    public override object? Build()
    {
        var users = UseState(new[] { 
            new { Id = 1, Name = "Alice" },
            new { Id = 2, Name = "Bob" }
        });
        
        var navigator = UseNavigation();
        
        return Layout.Vertical(
            users.Value.Select(user => 
                new Button(user.Name)
                    .HandleClick(() => 
                    {
                        navigator.Navigate(typeof(UserProfileApp),
                            new UserProfileArgs(user.Id, "details"));
                    })
            )
        );
    }
}

public class UserProfileApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<UserProfileArgs>();
        
        if (args == null)
        {
            return Text.Literal("Invalid user profile");
        }
        
        var activeTab = UseState(args.Tab);
        
        return Layout.Vertical(
            Text.Heading($"User {args.UserId}"),
            Layout.Horizontal(
                new Button("Overview", 
                    active: activeTab.Value == "overview",
                    onClick: _ => activeTab.Set("overview")),
                new Button("Details",
                    active: activeTab.Value == "details",
                    onClick: _ => activeTab.Set("details")),
                new Button("Settings",
                    active: activeTab.Value == "settings",
                    onClick: _ => activeTab.Set("settings"))
            ),
            RenderTabContent(args.UserId, activeTab.Value)
        );
    }
    
    private object RenderTabContent(int userId, string tab)
    {
        return tab switch
        {
            "overview" => Text.Literal($"Overview for user {userId}"),
            "details" => Text.Literal($"Details for user {userId}"),
            "settings" => Text.Literal($"Settings for user {userId}"),
            _ => Text.Literal("Unknown tab")
        };
    }
}
```

### Product Search with Filters

```csharp
public record ProductSearchArgs(
    string Query,
    string? Category = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1
);

public class ProductSearchApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<ProductSearchArgs>();
        var navigator = UseNavigation();
        
        // Use provided args or defaults
        var query = UseState(args?.Query ?? "");
        var category = UseState(args?.Category ?? "");
        var minPrice = UseState(args?.MinPrice ?? 0m);
        var maxPrice = UseState(args?.MaxPrice ?? 1000m);
        
        var results = UseQuery(
            () => SearchProducts(query.Value, category.Value, minPrice.Value, maxPrice.Value),
            query, category, minPrice, maxPrice
        );
        
        return Layout.Vertical(
            Layout.Horizontal(
                query.ToTextInput("Search").Placeholder("Enter search term"),
                category.ToTextInput("Category").Placeholder("Filter by category"),
                minPrice.ToNumberInput("Min Price"),
                maxPrice.ToNumberInput("Max Price")
            ),
            new Button("Search")
                .HandleClick(() =>
                {
                    navigator.Navigate(typeof(ProductSearchApp),
                        new ProductSearchArgs(
                            query.Value,
                            category.Value,
                            minPrice.Value,
                            maxPrice.Value
                        ));
                }),
            results.Value != null 
                ? new ProductList(results.Value)
                : Text.Literal("Loading...")
        );
    }
    
    private Task<List<Product>> SearchProducts(
        string query, string? category, decimal minPrice, decimal maxPrice)
    {
        // Implementation
        return Task.FromResult(new List<Product>());
    }
}
```

### Conditional Rendering Based on Args

```csharp
public record DashboardArgs(string? View = null, int? ItemId = null);

public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<DashboardArgs>();
        
        // Render different views based on arguments
        if (args?.View == "details" && args.ItemId.HasValue)
        {
            return new ItemDetailView(args.ItemId.Value);
        }
        
        if (args?.View == "settings")
        {
            return new SettingsView();
        }
        
        // Default dashboard view
        return new DashboardOverview();
    }
}
```

### URL Query Parameters

Arguments can also be passed via URL query parameters:

```csharp
// Navigate with URL and args
navigator.Navigate("app://products/search?appArgs=" + 
    Uri.EscapeDataString(JsonSerializer.Serialize(
        new ProductSearchArgs("laptop", "electronics"))));

// In the target component
public class ProductSearchApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<ProductSearchArgs>();
        // args will contain the deserialized ProductSearchArgs
        return RenderSearchResults(args);
    }
}
```

## Best Practices

### Use Records for Simple Arguments

Records are ideal for argument types because they're immutable and provide value equality:

```csharp
// Good: Simple record
public record UserArgs(int UserId, string Tab);

// Less ideal: Complex class with methods
public class UserArgs
{
    public int UserId { get; set; }
    public string Tab { get; set; }
    public void DoSomething() { } // Methods don't serialize
}
```

### Provide Default Values

Use default parameter values to make arguments optional:

```csharp
// Good: Default values make args optional
public record SearchArgs(string Query, int Page = 1, string? SortBy = null);

// Usage
var args = UseArgs<SearchArgs>();
// args.Query is required, but Page and SortBy have defaults
```

### Handle Null Arguments

Always check for null when using `UseArgs`:

```csharp
// Good: Null check
var args = UseArgs<UserArgs>();
if (args == null)
{
    return Text.Literal("Invalid arguments");
}

// Bad: No null check
var args = UseArgs<UserArgs>();
return Text.Literal($"User: {args.UserId}"); // Could throw NullReferenceException
```

### Keep Arguments Simple

Arguments should be simple data structures that serialize well:

```csharp
// Good: Simple, serializable types
public record SimpleArgs(string Name, int Count, DateTime Created);

// Bad: Complex types that don't serialize well
public record ComplexArgs(
    Action Callback,           // Delegates don't serialize
    Stream Data,               // Streams don't serialize
    IDisposable Resource       // Resources don't serialize
);
```

### Use Descriptive Names

Make argument types descriptive and specific to their use case:

```csharp
// Good: Descriptive and specific
public record UserProfileArgs(int UserId, string Tab);
public record ProductSearchArgs(string Query, string? Category);

// Bad: Generic and unclear
public record Args(int Id, string Value);
public record Data(object Payload);
```

## Common Patterns

### Default Arguments

Provide default behavior when args are null:

```csharp
public class ProductListApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<ProductListArgs>();
        
        // Use defaults if args are null
        var category = args?.Category ?? "all";
        var sortBy = args?.SortBy ?? "name";
        var page = args?.Page ?? 1;
        
        return RenderProductList(category, sortBy, page);
    }
}
```

### Argument Validation

Validate arguments and show errors if invalid:

```csharp
public class UserDetailApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<UserDetailArgs>();
        
        if (args == null || args.UserId <= 0)
        {
            return Layout.Vertical(
                Text.Heading("Error"),
                Text.Literal("Invalid user ID provided"),
                new Button("Go Back")
                    .HandleClick(() => UseNavigation().Navigate(typeof(UserListApp)))
            );
        }
        
        return RenderUserDetails(args.UserId);
    }
}
```

### Argument-Based Routing

Use arguments to determine which view to render:

```csharp
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<MainAppArgs>();
        
        return args?.View switch
        {
            "dashboard" => new DashboardView(),
            "settings" => new SettingsView(),
            "profile" => new ProfileView(args.UserId),
            _ => new DefaultView()
        };
    }
}
```

## Troubleshooting

### Args Are Always Null

If `UseArgs` always returns null, check:

1. **Arguments were passed during navigation**:

```csharp
// Correct: Passing args
navigator.Navigate(typeof(TargetApp), new MyArgs("value"));

// Incorrect: Not passing args
navigator.Navigate(typeof(TargetApp));
```

2. **Argument type matches**:

```csharp
// Correct: Types match
navigator.Navigate(typeof(App), new UserArgs(123));
var args = UseArgs<UserArgs>(); // Works

// Incorrect: Types don't match
navigator.Navigate(typeof(App), new UserArgs(123));
var args = UseArgs<ProductArgs>(); // Returns null
```

### Serialization Errors

If arguments fail to serialize, ensure:

1. **All properties are serializable**:

```csharp
// Good: All properties serialize
public record GoodArgs(string Name, int Count);

// Bad: Non-serializable property
public record BadArgs(string Name, Action Callback);
```

2. **No circular references**:

```csharp
// Bad: Circular reference
public class Parent { public Child Child { get; set; } }
public class Child { public Parent Parent { get; set; } }
```

### Type Mismatch Errors

Ensure the argument type used in `UseArgs` matches the type passed during navigation:

```csharp
// Correct: Types match exactly
navigator.Navigate(typeof(App), new UserArgs(123));
var args = UseArgs<UserArgs>();

// Incorrect: Different types
navigator.Navigate(typeof(App), new UserArgs(123));
var args = UseArgs<DifferentArgs>(); // Returns null
```

## See Also

- [Navigation](./23_UseNavigation.md) - Programmatic navigation between components
- [State](./03_State.md) - Component state management
- [Context](./12_Context.md) - Component-scoped data sharing
- [Rules of Hooks](../02_RulesOfHooks.md) - Understanding hook rules and best practices
- [Views](../../../01_Onboarding/02_Concepts/02_Views.md) - Understanding Ivy views and components
