---
searchHints:
  - app-context
  - base-path
  - environment
  - configuration
  - reverse-proxy
---

# App Context

<Ingress>
Understand how to access runtime environment information and host your Ivy applications under custom subpaths using the App Context and Base Path features.
</Ingress>

The `AppContext` provides your Ivy application with essential information about the current connection, the hosting environment, and application-specific arguments. It is particularly useful when deploying applications behind reverse proxies or when hosting multiple applications under different URL prefixes.

## Accessing App Context

The `AppContext` is automatically registered in the dependency injection container for every application session. You can access it in your [Views](./02_Views.md) using the `UseService<T>` hook.

```csharp
public class MyView : ViewBase
{
    public override object? Build()
    {
        var context = UseService<AppContext>();
        
        return Layout.Vertical()
            | Text.P($"Current App ID: {context.AppId}")
            | Text.P($"Connection ID: {context.ConnectionId}");
    }
}
```

## Available Properties

| Property | Description | Example |
|----------|-------------|---------|
| `Scheme` | The request protocol (http or https). | `"https"` |
| `Host` | The hostname of the current request. | `"example.com"` |
| `BasePath` | The configured subpath for the application. | `"/studio"` |
| `BaseUrl` | The full base URL including scheme, host, and base path. | `"https://example.com/studio/"` |
| `AppId` | The ID of the currently running application. | `"MyApp"` |
| `NavigationAppId` | The app ID as it appears in the URL. | `"MyApp"` |
| `ConnectionId` | Unique ID for the current SignalR connection. | `"abc-123..."` |
| `MachineId` | Unique ID for the client browser/machine. | `"uuid-..."` |

### Application Arguments

You can also access application arguments passed via the URL using `GetArgs<T>()`:

```csharp
var args = context.GetArgs<MyArgs>();
```

## Base Path Configuration

By default, Ivy applications are hosted at the root of the domain. If you need to host your application under a subpath (e.g., `https://example.com/my-app/`), you can configure the `BasePath` in your `Program.cs`.

```csharp
await IvyServer.Run(new IvyServerArgs
{
    BasePath = "/my-app",
    // ... other args
});
```

### Why use a Base Path?

1. **Reverse Proxies**: When your application is behind a proxy like Nginx or Cloudflare Tunnel that maps a path to your service.
2. **Multi-tenancy**: Hosting different apps under different paths on the same domain.
3. **Studio Integration**: Running apps inside the Ivy Studio environment.

## Frontend Utilities

The framework automatically injects the necessary metadata into your HTML to ensure that the frontend can resolve paths correctly. In your React code, you can use the following utility functions from `@/lib/utils`:

- `getIvyBasePath()`: Returns the configured base path string.
- `getIvyHost()`: Returns the full origin including the base path if present.

These utilities ensure that `app://` protocol links and API calls are automatically routed through the correct base path.
