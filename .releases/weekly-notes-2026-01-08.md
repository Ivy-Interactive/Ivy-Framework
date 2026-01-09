# Ivy Framework Weekly Notes - Week of 2026-01-08

## Breaking Changes

### UseBuilder Renamed to UseWebApplicationBuilder

The `Server.UseBuilder()` method has been renamed to `UseWebApplicationBuilder()` for better clarity and to distinguish it from the new `UseWebApplication()` method. [Read more here](https://docs.ivy.app/onboarding/concepts/program)

### NavigationPurpose Renamed to HistoryOp

- `NavigationPurpose` - `HistoryOp`
- `NavigationPurpose.NewDestination` - `HistoryOp.Push`
- `NavigationPurpose.HistoryTraversal` - `HistoryOp.Pop`

### Audio Widget Renamed to AudioPlayer

The `Audio` widget has been renamed to `AudioPlayer` for better clarity and consistency. [Read more here](https://docs.ivy.app/widgets/primitives/audio-player)

### Icons Enum Updated to Match Lucide React 0.562.0

The `Icons` enum [has been updated](https://github.com/Ivy-Interactive/Ivy-Framework/pull/1912) to align with the latest version of lucide-react (0.562.0).
- 10 icons were renamed (e.g., AlignCenter → TextAlignCenter, Chrome → Chromium)
- 16 icons were removed (mostly *2 variants like FileCheck2, plus Text, FileJson, LetterText)
More info on how to use Icons can be found [here](https://docs.ivy.app/widgets/primitives/icon)

## Improvements

### ReadOnlyInput Simplified Constructor

The `ReadOnlyInput` widget now includes a non-generic constructor for string values. [Read more here](https://docs.ivy.app/widgets/inputs/read-only)

```csharp
var readOnly = new ReadOnlyInput("User ID: 12345");
```
### LLM-Friendly Documentation

[Ivy.Docs](https://docs.ivy.app/onboarding/getting-started/introduction) had gotten a lot of LLM-friendly updates. 

- `UseSitemap()` - Automatically generates `/robots.txt` and `/sitemap.xml` based on your visible apps.
- `UseSsrMarkdown()` - Detects bot user agents (ChatGPT, Claude, Perplexity, etc.) and serves simplified HTML with markdown content
- `UseMarkdownFiles()` - Serves embedded `.md` files directly at URLs like `/api/button.md`, with caching for performance

### Enhanced Calendar Navigation

Calendar widgets (`DateInput`, `DateTimeInput`, `DateRangeInput`) now include dropdown selectors for month and year. Instead of clicking through months one-by-one, you can now:

- **Select year from dropdown** - Choose any year from 1900 to 2100 directly
- **Select month from dropdown** - Jump to any month instantly
- **Improved date visibility** - Fixed an issue where some dates could appear invisible in certain scenarios

More info on the [Date Time input here](https://docs.ivy.app/widgets/inputs/date-time) and on [Date Range input here](https://docs.ivy.app/widgets/inputs/date-range)

### Font Loading Performance

Font flickering during page load has been eliminated by migrating to the Ivy Design System package.

### Automatic Color Input Validation

The `ColorInput` widget now automatically validates color values and displays an error state when invalid formats are entered.

**What's validated:**

- **Hex colors** - Must match valid formats: `#RGB`, `#RRGGBB`, or `#RRGGBBAA`
- **Color enums** - Must be a valid value from the `Colors` enum
- **Invalid entries** - Automatically marked with "Invalid color format" error message

### Widget Serialization Optimization

Added `AlwaysSerialize` property to `Prop` attribute to bypass default value optimization for specific properties. This is used by default for values in `Select` input.

### Scale API inheritance in widgets

Widgets now inherit scale settings from their parent widgets, ensuring consistent sizing throughout nested component hierarchies. When you set a scale on a parent widget, all children automatically inherit that scale unless explicitly overridden.

### Terminal Widget Copy Button

The `Terminal` widget now includes a convenient copy-to-clipboard button that automatically extracts and copies only command lines from the terminal display.

### Code Snippet Copy Button Enhancement

## Bug Fixes

- Fixed Page Padding in Non-Chrome Applications
- Improved Tooltip Text Handling
- Fixed bugs related with Root Widget Replacement
- Implemented Authentication Logout When User Info Unavailable
- Fixed OAuth Popup Blocking in Safari
- Chart Configuration Consistency
- GrpcDataTableService is now loaded lazily
- Fixed incorrent display of x-axis labels in some charts
- When applying a new theme, if some variables were not defined in the new theme, defaults are now applied

## New Features

### Clerk Authentication Provider

A new authentication provider for Clerk (<https://clerk.com>) has been added to the framework, allowing you to leverage Clerk's complete user management platform in your Ivy applications.

```bash
dotnet add package Ivy.Auth.Clerk
```

1. Create a Clerk application at [clerk.com](https://clerk.com)
2. Configure your Clerk keys using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "Clerk:SecretKey" "your_secret_key"
dotnet user-secrets set "Clerk:PublishableKey" "your_publishable_key"
```

```csharp
var server = new Server();

var authProvider = new ClerkAuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft();

server.UseAuth(authProvider);

await server.RunAsync();
```

Read more on [Clerk Auth Provider here](https://docs.ivy.app/onboarding/cli/authentication/clerk)

### GitHub OAuth Authentication Provider

A new authentication provider for GitHub OAuth 2.0 has been added to the framework, allowing users to sign in to your Ivy applications using their GitHub accounts.

```bash
dotnet add package Ivy.Auth.GitHub
```

```csharp
var server = new Server();

// Register HttpClient for GitHub API
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});
server.Services.AddSingleton(server.Configuration);
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());
await server.RunAsync();
```

Configure your GitHub OAuth App credentials using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "GitHub:ClientId" "your_client_id"
dotnet user-secrets set "GitHub:ClientSecret" "your_client_secret"
dotnet user-secrets set "GitHub:RedirectUri" "http://localhost:5010/ivy/webhook"
```

### Dynamic Page Titles

The framework now automatically updates the browser page title to reflect your current application route.

When you define an `AppDescriptor`, the framework automatically uses its `Title` property to set the browser page title:

```csharp
yield return new AppDescriptor(
    Id: "dashboard",
    Title: "Dashboard",  // This becomes the browser page title
    Component: typeof(DashboardView),
    MenuItems: [/* ... */]
)
```