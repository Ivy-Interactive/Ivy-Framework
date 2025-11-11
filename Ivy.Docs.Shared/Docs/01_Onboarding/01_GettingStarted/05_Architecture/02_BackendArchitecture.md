---
searchHints:
  - backend
  - csharp
  - asp.net
  - server
  - signalr
  - views
  - widgets
---

# Backend Architecture

<Ingress>
The Ivy backend is built on ASP.NET Core with SignalR for real-time communication. The Server class serves as the main configuration entry point, providing a fluent API for setting up applications, authentication, and services.
</Ingress>

For information about the React frontend system and widget rendering, see [Frontend Architecture](./01_FrontendArchitecture.md). For details on SignalR communication patterns, see [Communication](./03_Communication.md).

## Core Server Architecture

The Ivy backend is built around the `Server` class which serves as the main configuration and hosting entry point. The server provides a fluent API for configuring applications, authentication, chrome (UI shell), and middleware.

### Server Startup Flow

The `Server` class manages three primary concerns:

- **Application Discovery**: Through `AppRepository` which finds and instantiates application classes
- **Service Configuration**: Via the `Services` property exposing ASP.NET Core's `IServiceCollection`
- **Web Host Setup**: In the `RunAsync` method which configures the ASP.NET Core pipeline

```csharp
public class Server
{
    private readonly WebApplicationBuilder _builder;
    private readonly List<Type> _appTypes = new();
    private readonly List<Type> _serviceTypes = new();
    private Type? _authProviderType;
    private Chrome? _chrome;
    private bool _hotReloadEnabled;

    public Server(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    // ... configuration methods ...
}
```

### Core Server Components

The server exposes several key properties and methods for configuration:

- **Services**: Direct access to ASP.NET Core's dependency injection container
- **AddApp()**: Register individual applications
- **AddAppsFromAssembly()**: Auto-discover applications via reflection
- **UseChrome()**: Configure the application shell/layout
- **UseAuth<T>()**: Configure authentication providers
- **UseHotReload()**: Enable development hot reload

```csharp
public async Task RunAsync()
{
    var app = _builder.Build();
    
    // Configure SignalR hub
    app.MapHub<AppHub>("/messages");
    
    // Register applications
    var appRepository = new AppRepository(_appTypes);
    app.Services.AddSingleton(appRepository);
    
    // Enable hot reload if configured
    if (_hotReloadEnabled)
    {
        var hotReloadService = app.Services.GetRequiredService<HotReloadService>();
        hotReloadService.StartWatching();
    }
    
    await app.RunAsync();
}
```

## Application System

The application system is built around a pattern where each application is defined by an `AppDescriptor` that specifies how to create `ViewBase` instances. Applications are discovered through reflection or explicitly registered.

### Application Registration and Discovery

| Registration Method | Description | Example Usage |
|-------------------|-------------|---------------|
| `AddApp(Type)` | Register a single app type | `server.AddApp<MyApp>()` |
| `AddApp(AppDescriptor)` | Register with explicit descriptor | Custom configuration |
| `AddAppsFromAssembly()` | Auto-discover apps with `[App]` attribute | Scan entire assembly |
| Constructor with `FuncBuilder` | Single-app server | Quick prototyping |

```csharp
public Server AddAppsFromAssembly(Assembly assembly)
{
    var appTypes = assembly.GetTypes()
        .Where(t => t.IsSubclassOf(typeof(ViewBase)) && t.GetCustomAttribute<AppAttribute>() != null)
        .ToList();

    _appTypes.AddRange(appTypes);
    return this;
}

public Server AddApp<T>() where T : ViewBase
{
    _appTypes.Add(typeof(T));
    return this;
}
```

### ViewBase and Application Lifecycle

Applications in Ivy inherit from `ViewBase`, which provides access to state management hooks, memoization, and service injection. The `ViewBase` class serves as the foundation for all application logic and UI composition.

**Key Features:**

- **Build()**: Method that returns widgets or other views
- **Hooks**: State management (`UseState`, `UseEffect`, etc.)
- **Services**: Dependency injection via `UseService<T>()`
- **Memoization**: Automatic caching of expensive computations
- **Lifecycle**: Automatic cleanup and resource management

```csharp
[App(icon: Icons.Calendar, name: "Todo App")]
public class TodoApp : ViewBase
{
    public override object? Build()
    {
        var todos = UseState(ImmutableArray.Create<Todo>());
        var client = UseService<IClientProvider>();
        
        return Layout.Vertical(
            new Button("Add Todo", onClick: _ => {
                todos.Set(todos.Value.Add(new Todo("New Task", false)));
                client.Toast("Todo added!");
            }),
            todos.Value.Select(todo => new TodoItem(todo))
        );
    }
}
```

## Widget System Architecture

The backend widget system defines the data models and type hierarchy for all UI components. Widgets are implemented as C# records that serialize to JSON for frontend consumption.

### Widget Serialization and Props

Widget properties marked with `[Prop]` attribute are serialized to JSON and sent to the frontend. As an optimization, **null values are not serialized** - properties with null values are omitted from the JSON payload to reduce network traffic.

### Widget Type Hierarchy

Widgets inherit from `WidgetBase` and can represent input controls, layout containers, or content renderers. The type system ensures type safety while allowing flexible composition.

```csharp
public interface IAnyInput
{
    string Id { get; }
    object? Value { get; }
    bool IsReadOnly { get; }
    string? Placeholder { get; }
    ValidationState ValidationState { get; }
    string? ValidationMessage { get; }
}

public interface IInput<T> : IAnyInput
{
    new T? Value { get; }
    event Action<T?>? OnChange;
}
```

### Input Widget Type System

The input widget system uses a sophisticated type conversion mechanism that allows widgets to bind to various .NET types while maintaining type safety:

```csharp
public abstract class BoolInputBase : InputWidgetBase<bool>
{
    protected override IEnumerable<Type> SupportedStateTypes()
    {
        yield return typeof(bool);
        yield return typeof(bool?);
        yield return typeof(int);
        yield return typeof(int?);
        yield return typeof(long);
        yield return typeof(long?);
    }
}
```

```csharp
public abstract class NumberInputBase : InputWidgetBase<decimal>
{
    protected override IEnumerable<Type> SupportedStateTypes()
    {
        yield return typeof(byte);
        yield return typeof(sbyte);
        yield return typeof(short);
        yield return typeof(ushort);
        yield return typeof(int);
        yield return typeof(uint);
        yield return typeof(long);
        yield return typeof(ulong);
        yield return typeof(float);
        yield return typeof(double);
        yield return typeof(decimal);
        // ... nullable variants
    }
}
```

```csharp
public abstract class DateTimeInputBase : InputWidgetBase<DateTime>
{
    protected override IEnumerable<Type> SupportedStateTypes()
    {
        yield return typeof(DateTime);
        yield return typeof(DateTime?);
        yield return typeof(DateOnly);
        yield return typeof(DateOnly?);
        yield return typeof(TimeOnly);
        yield return typeof(TimeOnly?);
    }
}
```

Each input widget base class implements `SupportedStateTypes()` method that returns compatible .NET types:

| Widget Type | Supported Types | Example |
|------------|----------------|---------|
| `BoolInputBase` | `bool`, `bool?`, numeric types | Checkbox, Switch, Toggle |
| `NumberInputBase` | All numeric types, nullable variants | Number input, Slider |
| `DateTimeInputBase` | `DateTime`, `DateOnly`, `TimeOnly` + nullable | Date picker, Time picker |
| `TextInputBase` | `string`, most convertible types | Text area, Single line |

## State Management

The state management system provides reactive state handling through hooks-style APIs similar to React, but implemented in C#.

### State Hook Patterns

State objects provide automatic change detection and can be bound directly to input widgets through extension methods like `state.ToBoolInput()` or `state.ToNumberInput()`.

```csharp
public static BoolInput ToBoolInput(this IState<bool> state, string? placeholder = null)
{
    return new BoolInput(state.Id)
        .Value(state.Value)
        .Placeholder(placeholder)
        .OnChange(value => state.Set(value));
}
```

**State Features:**

- **Automatic Change Detection**: State changes trigger re-renders
- **Type Safety**: Strongly typed state values
- **Widget Binding**: Direct conversion to input widgets
- **Persistence**: Optional state persistence across sessions
- **Computed Values**: Derived state from other state values

## Service Container and Dependency Injection

The server exposes ASP.NET Core's dependency injection container through the `Services` property, allowing registration of custom services, database contexts, and other dependencies.

### Service Registration Patterns

```csharp
public Server AddService<TService, TImplementation>()
    where TService : class
    where TImplementation : class, TService
{
    _serviceTypes.Add(typeof(TImplementation));
    _builder.Services.AddScoped<TService, TImplementation>();
    return this;
}

public Server AddService<TService>(Func<IServiceProvider, TService> factory)
    where TService : class
{
    _builder.Services.AddScoped(factory);
    return this;
}
```

The framework automatically registers core services like `IClientNotifier` for SignalR communication and `AppSessionStore` for session management.

**Built-in Services:**

- `IClientProvider`: Send messages and notifications to frontend
- `IThemeService`: Manage application themes
- `ILogger<T>`: Logging infrastructure
- `AppSessionStore`: Session state management
- `HotReloadService`: Development hot reload support

## Real-time Communication Infrastructure

The backend provides real-time communication through SignalR with the `AppHub` class serving as the central message hub.

### SignalR Hub Architecture

```csharp
app.MapHub<AppHub>("/messages");

if (_hotReloadEnabled)
{
    var hotReloadService = app.Services.GetRequiredService<HotReloadService>();
    hotReloadService.StartWatching();
}
```

The SignalR hub is registered at the `/messages` endpoint and handles real-time communication including hot reload notifications during development and widget event processing.

**Hub Responsibilities:**

- **Connection Management**: Establishes and maintains WebSocket connections
- **Message Routing**: Routes messages between frontend and backend
- **State Synchronization**: Tracks widget tree state per connection
- **Event Handling**: Processes user interaction events from frontend
- **Hot Reload**: Sends refresh messages when code changes are detected

This backend architecture provides a solid foundation for building internal applications with real-time updates, type-safe widget binding, and flexible service composition while maintaining clean separation between server-side logic and frontend presentation.
