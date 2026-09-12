---
searchHints:
  - startup
  - configuration
  - bootstrap
  - server
  - main
  - entry-point
---

# Program

<Ingress>
Configure and bootstrap your Ivy [application](./10_Apps.md) with dependency injection, [services](../../03_Hooks/02_Core/11_UseService.md), and middleware for production-ready deployment.
</Ingress>

The `Program.cs` file is the entry point for your Ivy application. It configures and starts the Ivy server using the `Server` class, which provides a fluent API for setting up apps, authentication, middleware, and other services.

<Callout Type="tip">
Want to try Ivy without a full project? You can run a [file-based app](./19_FileBasedApps.md) from a single `.cs` file using `dotnet run YourFile.cs`.
</Callout>

## Basic Structure

Every Ivy application follows a similar startup pattern:

```csharp
var server = new Server();
server.UseCulture("en-US");
server.UseHotReload();
server.AddAppsFromAssembly();
server.UseAppShell();
await server.RunAsync();
```

## Server Configuration

The `Server` class accepts optional `ServerArgs` for configuration:

```csharp
// Default configuration
var server = new Server();

// Custom configuration
var server = new Server(new ServerArgs
{
    Port = 8080,
    BasePath = "/my-app",
    Verbose = true,
    Browse = true,
    Silent = false
});
```

### ServerArgs Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Port` | `int` | `5010` | Port number for the server |
| `BasePath` | `string?` | `null` | URL base path prefix (for example `/my-app`) |
| `Verbose` | `bool` | `false` | Enable verbose logging |
| `Browse` | `bool` | `false` | Automatically open browser on startup |
| `Silent` | `bool` | `false` | Suppress startup messages |
| `DefaultAppId` | `string?` | `null` | Set the default app to load |
| `LocalFileRoots` | `string[]` | `[]` | Directories `GET /ivy/local-file` may serve from. Empty means any readable file on the machine. |
| `LocalFileExtensions` | `string[]` | `[]` | File extensions `GET /ivy/local-file` may serve. Empty means any extension. |
| `AllowedCorsOrigins` | `string[]` | `[]` | Cross-origin origins the default CORS policy reflects. Empty allows loopback origins only, and only when the server binds loopback. |
| `Metadata.Title` | `string?` | `null` | HTML meta title |
| `Metadata.Description` | `string?` | `null` | HTML meta description |
| `Metadata.GitHubUrl` | `string?` | `null` | GitHub repository URL meta tag |
| `Metadata.OgImage` | `string?` | `null` (auto from GitHub URL) | Open Graph image URL. Auto-generated from `Metadata.GitHubUrl` + title when not set. |
| `Metadata.OgSiteName` | `string?` | `null` (auto from assembly) | Site name for `og:site_name`. Auto-derived from entry assembly name when not set. |
| `Metadata.OgType` | `string?` | `"website"` | Open Graph type |
| `Metadata.OgLocale` | `string?` | `"en_US"` | Open Graph locale |
| `Metadata.TwitterCard` | `string?` | `"summary_large_image"` | Twitter card type |

## Adding Applications

### From Assembly

The most common approach is to automatically discover apps from an assembly:

```csharp
// Discover apps from the calling assembly
server.AddAppsFromAssembly();

// Discover apps from a specific assembly
server.AddAppsFromAssembly(typeof(MyApp).Assembly);
```

### Individual Apps

You can also add apps individually:

```csharp
// Add by type
server.AddApp(typeof(MyApp));

// Add by type and set as default
server.AddApp(typeof(MyApp), isDefault: true);

// Add using AppDescriptor
server.AddApp(new AppDescriptor
{
    Id = "my-app",
    Title = "My Application",
    ViewFunc = (context) => new MyView(),
    Group = ["Apps", "MyApp"],
    IsVisible = true
});
```

## Hot Reload

Enable hot reload for development:

```csharp
server.UseHotReload();
```

This automatically refreshes the browser when C# code changes during development.

For more information about configuring the application shell (sidebar, header, footer), see [AppShell](./11_AppShell.md).

## Authentication

<Callout Type="tip">
Use the `ivy auth add` command to automatically configure authentication providers in your project. This [CLI](../03_CLI/_Index.md) command will update your `Program.cs` and manage [secrets](./14_Secrets.md) for you. See the [Authentication CLI documentation](../03_CLI/04_Authentication/01_AuthenticationOverview.md) for details.
</Callout>

Ivy supports various authentication providers:

```csharp
// Supabase authentication
server.UseAuth<SupabaseAuthProvider>(c => 
    c.UseEmailPassword().UseGoogle());

// Auth0 authentication
server.UseAuth<Auth0AuthProvider>(c => 
{
    c.Domain = "your-domain.auth0.com";
    c.ClientId = "your-client-id";
});

// Microsoft Entra authentication
server.UseAuth<MicrosoftEntraAuthProvider>();
```

## Services and Dependency Injection

Register services for dependency injection:

```csharp
// Register services
server.Services.AddSingleton<IMyService, MyService>();
server.Services.AddScoped<IRepository, Repository>();

// Configure Entity Framework
server.UseBuilder(builder =>
{
    builder.Services.AddDbContext<MyDbContext>(options =>
        options.UseSqlServer(connectionString));
});
```

## Environment Configuration

### Environment Variables

The server automatically reads configuration from environment variables:

- `PORT` - Override the default port
- `BASE_PATH` - Serve the app from a URL prefix
- `VERBOSE` - Enable verbose logging
- `IVY_TLS` - Control whether the server uses HTTPS (`true`, `1`, `yes`, `on`) or HTTP (`false`, `0`, `no`, `off`). When unset, the default is HTTPS for local development on Windows only. On macOS and Linux, the default is HTTP unless you set `IVY_TLS=true` after generating a dev certificate with `dotnet dev-certs https`. In containers (`DOTNET_RUNNING_IN_CONTAINER=true`) or when `PORT` is set, the default is always HTTP on every OS, as a reverse proxy typically handles TLS in those environments. `Ivy.Desktop` behaves differently: it defaults `IVY_TLS` to `true` on Windows and macOS, and supplies its own certificate (falling back to a self-signed one under `~/.ivy/certs` when no ASP.NET Core dev certificate is present). Applied only when `ServerArgs.UseTls` is unset.
- `IVY_CORS_ORIGINS` - Comma- or semicolon-separated list of origins the default CORS policy allows (for example `https://app.example.com,https://admin.example.com`). Applied only when `AllowedCorsOrigins` is empty.
- `AllowedHosts` - Standard ASP.NET Core key, semicolon separated, e.g. `localhost;127.0.0.1`. Setting
  it (including to `*`) overrides the loopback default described under CORS and Host Filtering.

The table below summarizes the default scheme by entry point and platform:

| Entry point | Windows | macOS | Linux | Container, or `PORT` set |
| --- | --- | --- | --- | --- |
| `dotnet run` | HTTPS | HTTP | HTTP | HTTP |
| `Ivy.Desktop` | HTTPS | HTTPS | HTTP | not applicable |

When `BasePath` is set (via `ServerArgs`, CLI, or environment variable), Ivy applies ASP.NET Core `UsePathBase()` middleware to ensure routing and link generation work correctly under that prefix.

### Configuration Sources

```csharp
server.UseBuilder(builder =>
{
    builder.Configuration.AddJsonFile("appsettings.json");
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddUserSecrets<Program>();
});
```

## Production Configuration

### HTTPS Redirection

Enable HTTPS redirection for production:

```csharp
#if !DEBUG
server.UseHttpRedirection();
#endif
```

### Local File Access

`server.DangerouslyAllowLocalFiles()` enables the `/ivy/local-file` proxy endpoint that `Markdown` and
`Image` use to render files from disk. **The endpoint is unauthenticated**, so the roots you pass are the
only server-side confinement. The no-argument form serves **any readable file on the machine** and always
warns on stderr; it is retained deliberately for consumers that mitigate the endpoint another way (e.g.,
with middleware that validates the request origin and host). For most cases, pass the directories you
actually need and Ivy answers 404 for everything outside them:

```csharp
server.DangerouslyAllowLocalFiles("C:/Users/me/Photos", "D:/Screenshots");
```

Narrow it further with an extension allowlist. Anything else — including an extensionless path — answers 404:

```csharp
server.AllowLocalFileExtensions(".png", ".jpg", ".webp");
```

Both are additive, so repeated calls extend the sets. Every rejection is a 404 rather than a 403, so the
endpoint never reveals whether a path exists. Symlinks and junctions are resolved along the whole path, so a
link inside a root that points outside it answers 404.

### CORS and Host Filtering

Ivy serves its own frontend, so browsers reach the hub and the controllers same-origin and never consult
CORS. The default policy reflects loopback origins (which is what keeps the Vite dev server working) and
only when the server itself binds loopback — a container or hosted server that binds `*` allows nothing
it has not been told about. Add a genuinely cross-origin consumer explicitly:

```csharp
server.AllowCorsOrigins("https://app.example.com");
```

CORS cannot stop DNS rebinding: a page on a hostname rebound to `127.0.0.1` is *same-origin* with the
server, so no CORS check runs. Validating the `Host` header is what rejects it, so when the server
binds loopback (the `dotnet run` default) Ivy validates it for you: `localhost`, `127.0.0.1` and
`[::1]` are accepted and anything else answers 400 with `Bad Request - Invalid Hostname`. A server that
binds `*`, `+` or a public address (a container, a hosted deployment, or anything started with `PORT`
set) accepts any `Host` header, because a reverse proxy in front of it forwards its own public
hostname.

Name hosts explicitly to validate them on a non-loopback bind:

```csharp
server.AllowHosts("app.example.com");
```

`AllowHosts` replaces the list rather than extending it, so include the loopback names when you still
want them:

```csharp
server.AllowHosts("localhost", "127.0.0.1", "[::1]", "my-tunnel.ngrok-free.app");
```

That is the case to reach for when a tunnel (ngrok, Cloudflare, or editor port forwarding) forwards its
own hostname to a loopback bind. The standard ASP.NET Core `AllowedHosts` configuration key is the
other way out: it wins over Ivy's default entirely, so `AllowedHosts=*` in the environment or in
`appsettings.json` turns the check off again.

### Metadata

Set HTML metadata for SEO:

```csharp
server.SetMetaTitle("My Ivy Application");
server.SetMetaDescription("A powerful web application built with Ivy");
server.SetMetaGitHubUrl("https://github.com/user/repo");
```

## Complete Examples

### Simple Application

A minimal setup for development with hot reload enabled and basic app shell configuration.

```csharp
var server = new Server();
server.UseCulture("en-US");
server.UseHotReload();
server.AddAppsFromAssembly();
server.UseAppShell();
await server.RunAsync();
```

### Documentation Server

A specialized configuration for documentation sites with custom app shell, version display, and page-based navigation.

```csharp
var server = new Server();
server.UseCulture("en-US");
server.AddAppsFromAssembly(typeof(DocsServer).Assembly);
server.UseHotReload();

var version = typeof(Server).Assembly.GetName().Version!.ToString().EatRight(".0");
server.SetMetaTitle($"Ivy Docs {version}");

var appShellSettings = new AppShellSettings()
    .Header(
        Layout.Vertical().Padding(2)
        | new IvyLogo()
        | Text.Muted($"Version {version}")
    )
    .DefaultApp<IntroductionApp>()
    .UsePages();

server.UseAppShell(() => new DefaultSidebarAppShell(appShellSettings));
await server.RunAsync();
```

### Authentication-Enabled Application

A basic setup with Supabase authentication configured for email/password and Google OAuth login.

```csharp
var server = new Server();
server.UseCulture("en-US");
server.UseHotReload();
server.AddAppsFromAssembly();
server.UseAppShell();
server.UseAuth<SupabaseAuthProvider>(c =>
    c.UseEmailPassword().UseGoogle());
await server.RunAsync();
```

### Production-Ready Configuration

A comprehensive setup with conditional compilation, HTTPS redirection, metadata configuration, and dependency injection services for production deployment.

```csharp
var server = new Server();
server.UseCulture("en-US");

#if !DEBUG
server.UseHttpRedirection();
#endif

#if DEBUG
server.UseHotReload();
#endif

server.AddAppsFromAssembly();
server.UseAppShell();

server.SetMetaTitle("My Production App");
server.SetMetaDescription("Enterprise application built with Ivy");

// Configure services
server.UseBuilder(builder =>
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddSingleton<IEmailService, EmailService>();
});

await server.RunAsync();
```

## Advanced Configuration

### Custom Content Builder

Configure a custom content builder to handle specialized content rendering and processing.

```csharp
server.UseContentBuilder(new CustomContentBuilder());
```

### WebApplication Builder Modifications

Extend the underlying WebApplication builder with custom middleware, services, and logging configuration.

```csharp
server.UseBuilder(builder =>
{
    // Add custom middleware
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    
    // Configure logging
    builder.Logging.AddApplicationInsights();
});
```

### Connection Management

Automatically discover and register SignalR connection classes for real-time communication features.

```csharp
server.AddConnectionsFromAssembly();
```

This automatically discovers and registers SignalR connection classes for real-time communication.

## Faq

<Details>
<Summary>
What namespace are Ivy types in?
</Summary>
<Body>

All Ivy types are in the root `Ivy` namespace. There are no sub-namespaces. You only need:

```csharp
using Ivy;
```

This single using statement gives you access to everything: `ViewBase`, `IState<T>`, `MetricView`, `MetricRecord`, chart views (`LineChartView`, `PieChartView`, `BarChartView`, `AreaChartView`), `DataTable`, `DataTableBuilder<T>`, layout helpers (`Layout.Vertical()`, `Layout.Horizontal()`), `Button`, `TextInput`, all input types, `Card`, `Dialog`, `Sheet`, `Tab`, `Icons`, `RefreshToken`, `IClientProvider`, `IBladeContext`, `IConnection`, `IHaveSecrets`, and all other framework types.

**Do NOT use sub-namespaces** like `Ivy.Components`, `Ivy.Views.Dashboards`, `Ivy.Widgets.DataTables`, `Ivy.Client`, `Ivy.Hooks`, `Ivy.Services`, or `Ivy.Apps`. These do not exist — the framework source code organizes files in subdirectories but all types use `namespace Ivy;`.

Ivy projects include `<ImplicitUsings>enable</ImplicitUsings>` plus a global using for the project's own namespace, so typically the only explicit using you need is `using Ivy;` and `using Microsoft.EntityFrameworkCore;` (for database connections).

</Body>
</Details>
