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
    Verbose = true,
    Browse = true,
    Silent = false
});
```

### ServerArgs Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Port` | `int` | `5010` | Port number for the server |
| `Verbose` | `bool` | `false` | Enable verbose logging |
| `Browse` | `bool` | `false` | Automatically open browser on startup |
| `Silent` | `bool` | `false` | Suppress startup messages |
| `DefaultAppId` | `string?` | `null` | Set the default app to load |
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
- `VERBOSE` - Enable verbose logging

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

This single using statement gives you access to everything: `ViewBase`, `IState<T>`, `MetricView`, `MetricRecord`, chart views (`LineChartView`, `PieChartView`, `BarChartView`, `AreaChartView`), `DataTable`, `DataTableBuilder<T>`, layout helpers (`Layout.Vertical()`, `Layout.Horizontal()`), `Button`, `TextInput`, all input types, `Card`, `Dialog`, `Sheet`, `Tab`, `Icons`, `RefreshToken`, `IClientProvider`, `IBladeService`, `IConnection`, `IHaveSecrets`, and all other framework types.

**Do NOT use sub-namespaces** like `Ivy.Components`, `Ivy.Views.Dashboards`, `Ivy.Widgets.DataTables`, `Ivy.Client`, `Ivy.Hooks`, `Ivy.Services`, or `Ivy.Apps`. These do not exist — the framework source code organizes files in subdirectories but all types use `namespace Ivy;`.

Ivy projects include `<ImplicitUsings>enable</ImplicitUsings>` plus a global using for the project's own namespace, so typically the only explicit using you need is `using Ivy;` and `using Microsoft.EntityFrameworkCore;` (for database connections).

</Body>
</Details>
