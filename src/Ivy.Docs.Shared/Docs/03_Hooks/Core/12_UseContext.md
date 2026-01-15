---
searchHints:
  - context
  - usecontext
  - createcontext
  - component-context
  - scoped
  - provider
---

# UseContext

<Ingress>
The `UseContext` and `CreateContext` [hooks](../02_RulesOfHooks.md) enable component-level context management, allowing you to share data and services within a component tree without prop drilling.
</Ingress>

## Overview

Context [hooks](../02_RulesOfHooks.md) provide a way to share data and services across a component tree:

- **Component Scoping** - Context values are scoped to the component and its children
- **Avoid Prop Drilling** - Share data without passing props through every level
- **Hierarchical Resolution** - Context values can be resolved from parent components
- **Lifecycle Management** - Context values are automatically disposed when the component is disposed

<Callout type="Tip">
Context is different from [services](./11_UseService.md). Services are registered globally in your application, while context is scoped to a specific component and its children. Use context for component-specific data and services for application-wide functionality.
</Callout>

## Basic Usage

Use `CreateContext` to create a context value and `UseContext` to retrieve it:

```csharp demo-below
public record AppSettings(string Theme, int FontSize);

public class SettingsProvider : ViewBase
{
    public override object? Build()
    {
        CreateContext(() => new AppSettings("dark", 14));
        return new SettingsConsumer();
    }
}

public class SettingsConsumer : ViewBase
{
    public override object? Build()
    {
        var settings = UseContext<AppSettings>();
        return Text.Block($"Theme: {settings.Theme}, Size: {settings.FontSize}px");
    }
}
```

## How Context Works

### Context Resolution Flow

```mermaid
sequenceDiagram
    participant C as Child Component
    participant CC as Current Context
    participant PC as Parent Context
    participant AC as Ancestor Context
    
    Note over C,AC: Child calls UseContext<T>()
    C->>CC: Check current context
    CC-->>C: Not found
    C->>PC: Check parent context
    PC-->>C: Not found
    C->>AC: Check ancestor context
    AC-->>C: Found! Return value
```

### Context Scoping

Context values are scoped to the component where they are created:

```csharp
public class AppView : ViewBase
{
    public override object? Build()
    {
        // Context created here
        var userContext = CreateContext(() => new UserContext { UserId = "123" });
        
        return Layout.Vertical(
            new SectionView(),  // Can access userContext
            new AnotherView()   // Can also access userContext
        );
    }
}

public class SectionView : ViewBase
{
    public override object? Build()
    {
        var user = UseContext<UserContext>(); // Works - found in parent
        
        // Create a new context for this section's children
        var sectionConfig = CreateContext(() => new SectionConfig { Title = "Settings" });
        
        return new NestedView(); // Can access both userContext and sectionConfig
    }
}

public class NestedView : ViewBase
{
    public override object? Build()
    {
        var user = UseContext<UserContext>();        // Works - found in ancestor
        var config = UseContext<SectionConfig>();    // Works - found in parent
        
        return Text.Literal($"{config.Title} for {user.UserId}");
    }
}
```

## When to Use Context

| Use Context For | Use Services Instead For |
|-----------------|--------------------------|
| Component-Specific Configuration | Application-Wide Services |
| Shared State (avoid prop drilling) | Singleton Services |
| Component-Scoped Services | Infrastructure Services (logging, database, HTTP) |
| Theme and Styling | |
| Feature Flags (component tree specific) | |

## Lifecycle Management

Context values that implement `IDisposable` are automatically disposed when the component is disposed:

```csharp
public class ResourceContext : IDisposable
{
    private readonly FileStream _fileStream;
    
    public ResourceContext(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.Open);
    }
    
    public void Dispose()
    {
        _fileStream?.Dispose();
    }
}

public class FileView : ViewBase
{
    public override object? Build()
    {
        // ResourceContext will be automatically disposed when FileView is disposed
        var resource = CreateContext(() => new ResourceContext("data.txt"));
        
        return new FileContentView();
    }
}
```

## Common Patterns

### Provider Component

Create a provider component that sets up context for its children:

```csharp
public class ThemeProvider : ViewBase
{
    private readonly ThemeContext _theme;
    
    public ThemeProvider(ThemeContext theme)
    {
        _theme = theme;
    }
    
    public override object? Build()
    {
        CreateContext(() => _theme);
        return Children; // Render children passed to this component
    }
}
```

### Context with Factory

Use factory functions for lazy initialization:

```csharp
public class DataView : ViewBase
{
    public override object? Build()
    {
        // Context is only created when first accessed
        var cache = CreateContext(() => 
        {
            var c = new MemoryCache();
            c.Initialize();
            return c;
        });
        
        return new DataListView();
    }
}
```

### Conditional Context

Create context conditionally based on state:

```csharp
public class ConditionalView : ViewBase
{
    public override object? Build()
    {
        var isAuthenticated = UseState(false);
        
        if (isAuthenticated.Value)
        {
            var user = UseService<IUserService>().GetCurrentUser();
            CreateContext(() => new UserContext 
            { 
                UserId = user.Id,
                UserName = user.Name 
            });
        }
        
        return isAuthenticated.Value 
            ? new AuthenticatedView() 
            : new LoginView();
    }
}
```

## Troubleshooting

### Context Not Found Error

If `UseContext` throws an exception, the context wasn't created in a parent component:

```csharp
// Error: Context not found
public class ChildView : ViewBase
{
    public override object? Build()
    {
        var theme = UseContext<ThemeContext>(); // Throws InvalidOperationException
        return Text.Literal(theme.PrimaryColor);
    }
}

// Solution: Create context in parent
public class ParentView : ViewBase
{
    public override object? Build()
    {
        CreateContext(() => new ThemeContext { PrimaryColor = "blue" });
        return new ChildView();
    }
}
```

### Context Value Changes

Context values are created once per component. If you need reactive updates, use [state](./03_UseState.md) instead:

```csharp
// Context value doesn't update reactively
var config = CreateContext(() => new Config { Value = 10 });
// Changing config.Value won't trigger re-renders

// Use state for reactive updates
var config = UseState(new Config { Value = 10 });
// Changing config.Value will trigger re-renders
```

## Best Practices

- **Use context for component-scoped data** - Use services for app-wide data
- **Keep context values simple** - Data containers or lightweight services; use DI for heavy services
- **Use type safety** - Always use `UseContext<T>()` instead of runtime type checking
- **Avoid frequently changing data** - Use [state](./03_UseState.md) for reactive updates
- **Document context dependencies** - Make it clear when a component requires a parent context

## See Also

- [Services](./11_UseService.md) - Application-wide dependency injection
- [State](./03_UseState.md) - Reactive state management
- [Rules of Hooks](../02_RulesOfHooks.md) - Understanding hook rules and best practices
- [Views](../../../01_Onboarding/02_Concepts/02_Views.md) - Understanding Ivy views and components

## Examples

<Details>
<Summary>
User Context
</Summary>
<Body>

```csharp demo-below
public class UserContext
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public List<string> Permissions { get; set; } = new();
    
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }
}

public class AuthenticatedView : ViewBase
{
    public override object? Build()
    {
        CreateContext(() => new UserContext
        {
            UserId = "123",
            UserName = "John Doe",
            Permissions = new List<string> { "settings:edit", "profile:view" }
        });
        
        return Layout.Vertical()
            | new UserProfileView()
            | new UserSettingsView();
    }
}

public class UserProfileView : ViewBase
{
    public override object? Build()
    {
        var user = UseContext<UserContext>();
        
        return Layout.Vertical()
            | Text.H3($"Welcome, {user.UserName}!")
            | Text.Block($"User ID: {user.UserId}");
    }
}

public class UserSettingsView : ViewBase
{
    public override object? Build()
    {
        var user = UseContext<UserContext>();
        
        if (!user.HasPermission("settings:edit"))
        {
            return Text.Block("You don't have permission to edit settings.");
        }
        
        return Layout.Vertical()
            | Text.Block("Settings Form")
            | Text.P($"Editing settings for {user.UserName}").Small();
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Component-Scoped Service
</Summary>
<Body>

```csharp
public interface IDataCache
{
    void Set<T>(string key, T value);
    T? Get<T>(string key);
    void Clear();
}

public class MemoryCache : IDataCache, IDisposable
{
    private readonly Dictionary<string, object> _cache = new();
    
    public void Set<T>(string key, T value)
    {
        _cache[key] = value!;
    }
    
    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out var value) ? (T?)value : default;
    }
    
    public void Clear()
    {
        _cache.Clear();
    }
    
    public void Dispose()
    {
        _cache.Clear();
    }
}

public class DataView : ViewBase
{
    public override object? Build()
    {
        // Create a cache scoped to this component and its children
        var cache = CreateContext(() => new MemoryCache());
        
        return Layout.Vertical(
            new DataListView(),
            new DataDetailView()
        );
    }
}

public class DataListView : ViewBase
{
    public override object? Build()
    {
        var cache = UseContext<IDataCache>();
        
        var cachedData = cache.Get<List<Data>>("dataList");
        if (cachedData == null)
        {
            cachedData = LoadDataFromDatabase();
            cache.Set("dataList", cachedData);
        }
        
        return new Table(cachedData);
    }
    
    private List<Data> LoadDataFromDatabase() => new();
}
```

</Body>
</Details>
