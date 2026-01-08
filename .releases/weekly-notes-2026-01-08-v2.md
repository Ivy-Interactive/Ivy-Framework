# Ivy Framework Weekly Notes - Week of 2026-01-08

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## New Features

### GitHub Authentication Provider

We've added a new built-in authentication provider for GitHub OAuth 2.0. This makes it incredibly easy to let users sign in to your Ivy application using their GitHub accounts.

The new `Ivy.Auth.GitHub` package handles the entire OAuth flow, including:

- Secure token exchange
- Retrieving user profile information (email, name, avatar)
- Long-lived session management

**Usage:**

First, install the `Ivy.Auth.GitHub` package, then configure it in your `Program.cs`:

```csharp
using Ivy.Auth.GitHub;

var server = new Server();

// 1. Configure HttpClient for GitHub
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});

// 2. Add the provider
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

await server.RunAsync();
```

You'll need to provide your Client ID and Secret via configuration (User Secrets or Environment Variables):

```json
{
  "GitHub": {
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret",
    "RedirectUri": "https://your-app.com/ivy/webhook"
  }
}
```

### Clerk Authentication Provider

In addition to GitHub, we've also added support for **Clerk Authentication**. The `Ivy.Auth.Clerk` package provides a robust integration with Clerk, supporting multiple OAuth providers (Google, GitHub, Microsoft, etc.) and email/password flows.

```csharp
var authProvider = new ClerkAuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub();

server.UseAuth<ClerkAuthProvider>(...);
```

### Ivy Filters Library

We've introduced `Ivy.Filters`, a new library for robust filter parsing. It supports parsing filter strings into Abstract Syntax Trees (AST), enabling advanced filtering capabilities across the framework.

### Custom Login Views

You can now fully customize the login experience by replacing the default login view with your own `App`.

```csharp
server.UseAuth<BasicAuthProvider>(viewFactory: () => new MyCustomLoginApp());
```

### LLM-Friendly Documentation (llms.txt)

Ivy now supports the `llms.txt` standard out of the box! This feature automatically generates documentation optimized for Large Language Models (LLMs) and AI agents.

When enabled, your Ivy application will serve:

- `/llms.txt`: A manifest of available documentation.
- `/*.md`: Markdown representations of your pages and documentation.

This allows AI coding assistants (like Cursor, Windsurf, or GitHub Copilot) to easily "read" your application's structure and documentation to provide better context-aware assistance.

### Dynamic Page Titles

Browser page titles now automatically update to reflect the current active application or tab. This improves navigation and accessibility.

- When navigating to an App, the browser title updates to the App's Title.
- You can also set the title programmatically via the Client API.

```csharp
// In your view or logic
client.SetTitle("My Custom Page Title");
```

### New Loading Widget

We've added a dedicated `Loading` primitive widget. This provides a standard, consistent way to display loading states across your application.

```csharp
var loader = new Loading();
// Renders a standard circular spinner
```

## Breaking Changes

### Audio Widget Renamed to AudioPlayer

The `Audio` widget has been renamed to `AudioPlayer` for better clarity and consistency.

**Migration:**
Replace `new Audio(...)` with `new AudioPlayer(...)`.

```csharp
// Before
var audio = new Audio("music.mp3");

// After
var audio = new AudioPlayer("music.mp3");
```

### NavigationPurpose Renamed to HistoryOp

The `NavigationPurpose` enum used in navigation signals has been renamed to `HistoryOp` to better reflect its function (manipulating the browser history stack).

**Migration:**

- `NavigationPurpose.NewDestination` -> `HistoryOp.Push`
- `NavigationPurpose.HistoryTraversal` -> `HistoryOp.Pop`

```csharp
// Before
await navigateSignal.Send(new NavigateArgs(appId, Purpose: NavigationPurpose.NewDestination));

// After
await navigateSignal.Send(new NavigateArgs(appId, HistoryOp: HistoryOp.Push));
```

## Improvements

### User Interface Improvements

**ColorInput Validation & Polish:**
The `ColorInput` widget now automatically validates hex color values. We've also polished the `AsyncSelectInput` styling and removed the focus outline from `Sheet` containers for a cleaner look.

**DateTimeInput Split:**
We've internally refactored the `DateTimeInput` widget to better separate `Date`, `Time`, and `DateTime` logic.

**Scale Inheritance:**
Nested widgets now automatically inherit the `Scale` of their parent container (e.g., inside `Details`).

**Theme & Layout Updates:**

- **Popover Colors**: Added dedicated theme presets for Popovers.
- **Sidebar**: Fixed arrow positioning for nested grouped items.
- **Calendar**: Made dates clickable for better interaction.
- **Typography**: Refactored typography system for better consistency.

### Chart Improvements

**Multiple Reference Lines & Areas:**
You can now add multiple reference lines, areas, and dots to your charts (`AreaChart`, `BarChart`, `LineChart`).

```csharp
var chart = new AreaChart(data)
    .ReferenceLines(new[]
    {
        new MarkLine { /* Threshold 1 */ },
        new MarkLine { /* Threshold 2 */ }
    });
```

**ECharts v6 Upgrade:**
We've upgraded the underlying charting engine to ECharts v6.0.0, bringing significant performance improvements and better rendering.

**PieChart Polishing:**
We've refined the `PieChart` toolbox to disable incompatible "magic type" switching, which previously caused rendering issues.

### Widget Serialization Optimization

We've optimized how widgets are serialized to the frontend. Properties that match their default values are now omitted from the JSON payload. This significantly reduces network traffic and speeds up initial page loads.

### Other Notes

- **Documentation**: Significant updates including a new **Layouts** documentation page and a complete typography refactor.
- **Safari OAuth Fix**: Resolved an issue where OAuth popup windows were blocked on Safari.
- **Datatable Performance**: Lazy initialization of `GrpcTableService` improves startup time for apps that don't use data tables.
- **Code Snippets**: improved display of code blocks (removed cropping).
- **TerminalWidget**: Added a copy-to-clipboard button.
- **Icons**: Updated Lucide icons to v0.562.0, adding new icons and fixing existing ones.
- **Fonts**: Fixed font flickering issues by migrating to the new `@ivy-interactive/ivy-design-system` package.
- **Spacer**: Made the `Spacer` constructor public.
- **Validation**: Fixed tooltips word breaks and max height.
- **NumberInput**: Improved `null` value handling and strict equality checks.
- **Auth**: Users can now reliably log out even if their specific user info session has expired or is unavailable.
- **Database Generator**: The generator now cleans up initial migration history after applying, ensuring a cleaner state for new projects.
- **Internal**: Various internal cleanups including GitHub contributor display names, parameterless widget constructors, and improved core widget patching.
