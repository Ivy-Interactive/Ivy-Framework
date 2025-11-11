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

## Server Configuration

The `Server` class provides a fluent configuration API for setting up your Ivy application:

```42:106:Ivy/Server.cs
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

    /// <summary>
    /// Adds applications from the specified assembly by discovering all types that inherit from ViewBase
    /// and are decorated with the [App] attribute.
    /// </summary>
    public Server AddAppsFromAssembly(Assembly assembly)
    {
        var appTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ViewBase)) && t.GetCustomAttribute<AppAttribute>() != null)
            .ToList();

        _appTypes.AddRange(appTypes);
        return this;
    }

    /// <summary>
    /// Configures the application chrome (layout wrapper) that will be used for all apps.
    /// </summary>
    public Server UseChrome(Chrome chrome)
    {
        _chrome = chrome;
        return this;
    }

    /// <summary>
    /// Enables hot reload functionality for development.
    /// </summary>
    public Server UseHotReload()
    {
        _hotReloadEnabled = true;
        return this;
    }

    /// <summary>
    /// Configures an authentication provider for the application.
    /// </summary>
    public Server UseAuth<T>() where T : class, IAuthProvider
    {
        _authProviderType = typeof(T);
        return this;
    }
```

### Key Configuration Methods

- **`AddAppsFromAssembly()`**: Auto-discovers application classes decorated with `[App]` attribute
- **`UseChrome()`**: Set up application chrome/layout wrapper
- **`UseHotReload()`**: Enable hot reload during development
- **`UseAuth<T>()`**: Configure authentication providers

```164:184:Ivy/Server.cs
    /// <summary>
    /// Configures an authentication provider for the application.
    /// </summary>
    public Server UseAuth<T>() where T : class, IAuthProvider
    {
        _authProviderType = typeof(T);
        return this;
    }

    /// <summary>
    /// Adds a service to the dependency injection container.
    /// </summary>
    public Server AddService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _serviceTypes.Add(typeof(TImplementation));
        _builder.Services.AddScoped<TService, TImplementation>();
        return this;
    }
```

## Application and Service Architecture

### Application Discovery

Applications are discovered automatically by scanning assemblies for classes that:

1. Inherit from `ViewBase`
2. Are decorated with the `[App]` attribute

```csharp
[App(icon: Icons.Calendar, name: "Todo App")]
public class TodoApp : ViewBase
{
    public override object? Build()
    {
        // Application implementation
    }
}
```

### View System

Views are the core building blocks of Ivy applications:

- **ViewBase**: Base class for all views
- **Build()**: Method that returns widgets or other views
- **Hooks**: State management and side effects (`UseState`, `UseEffect`, etc.)
- **Services**: Dependency injection via `UseService<T>()`

### Widget System

Widgets represent UI components that are serialized to JSON and sent to the frontend:

- **WidgetBase**: Base class for all widgets
- **Serialization**: Widgets are serialized to JSON using System.Text.Json
- **Type Mapping**: Widget types map to React components on the frontend
- **Props**: Widget properties are serialized and passed to React components

## Communication System

### SignalR Hub

The `AppHub` handles WebSocket connections and message routing:

- **Connection Management**: Establishes connections per app instance
- **Message Routing**: Routes messages between frontend and backend
- **State Management**: Tracks widget tree state per connection
- **Event Handling**: Processes user interaction events

### Message Types

The backend sends several types of messages:

- **Refresh**: Complete widget tree replacement
- **Update**: JSON patch updates for incremental changes
- **Toast**: Notification messages
- **Error**: Error reporting with stack traces

## Hot Reload System

During development, the `HotReloadService` monitors file changes and triggers rebuilds:

```322:330:Ivy/Server.cs
        if (_hotReloadEnabled)
        {
            var hotReloadService = app.Services.GetRequiredService<HotReloadService>();
            hotReloadService.StartWatching();
        }
```

The hot reload system:

- Watches for file changes in the application directory
- Triggers rebuilds when changes are detected
- Sends refresh messages to connected clients
- Maintains application state during reloads

## Production Deployment

In production, the frontend is embedded as resources in the C# assembly:

```364:427:Ivy/Server.cs
    public static void UseFrontend(this WebApplication app)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var embeddedProvider = new EmbeddedFileProvider(assembly, "Ivy.frontend");

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = ""
        });

        app.UseStaticFiles(new StaticFilesOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = "",
            OnPrepareResponse = ctx =>
            {
                // Set cache headers for static assets
                var path = ctx.File.Name;
                if (path.EndsWith(".html"))
                {
                    ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                }
                else
                {
                    ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                }
            }
        });

        // Handle SPA routing - serve index.html for all non-API routes
        app.MapFallback(async context =>
        {
            context.Response.ContentType = "text/html";
            await using var stream = assembly.GetManifestResourceStream("Ivy.frontend.index.html");
            if (stream != null)
            {
                await stream.CopyToAsync(context.Response.Body);
            }
        });
    }
```

The embedded file provider:

- Serves the built frontend assets from the assembly
- Handles HTML template injection for metadata
- Sets appropriate caching headers
- Provides ETag generation for cache invalidation

## Dependency Injection

Ivy integrates with ASP.NET Core's dependency injection system:

- **Service Registration**: Services are registered in the `Server` configuration
- **Service Resolution**: Views can access services via `UseService<T>()`
- **Scoped Services**: Each request gets its own service scope
- **Built-in Services**: `IClientProvider`, `ILogger`, etc. are available automatically

