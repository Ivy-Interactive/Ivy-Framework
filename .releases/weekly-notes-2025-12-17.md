# Ivy Framework Weekly Notes - Week of 2025-12-17

## Breaking Changes

### Namespace Simplification

The `AppAttribute` and `ViewBase` classes have been moved to the root `Ivy` namespace for a simpler, cleaner import structure:

**What changed:**

- `AppAttribute` moved from `Ivy.Apps` to `Ivy`
- `ViewBase` moved from `Ivy.Core` to `Ivy`

**Migration:**

Update your using statements:

```csharp
// Old imports
using Ivy.Apps;
using Ivy.Core;

// New imports
using Ivy;
```

If you have the root `Ivy` namespace already imported, no changes are needed. Your existing code will continue to work, as the old namespaces still contain the classes (they're just now also available in the root namespace).

```csharp
// This now works with just: using Ivy;
[App(title: "My App", icon: Icons.Home)]
public class MyApp : ViewBase
{
    public override object Build()
    {
        // Your implementation
    }
}
```

## Authentication Improvements

### HTTP Tunneling for Authentication

Authentication providers can now make HTTP requests through the frontend using a new tunneling system. This enables auth providers to communicate with external APIs securely by routing requests through the client's browser, which is particularly useful for OAuth flows and development mode scenarios where the backend can't directly reach certain endpoints.

The implementation includes:

- `HttpTunnelingController` for handling tunneled requests
- `TunneledHttpMessageHandler` that integrates with .NET's `HttpClient`
- Automatic request/response serialization and header forwarding
- 30-second timeout with proper cancellation handling

This infrastructure is used internally by authentication providers and requires no configuration from your end.

### New Clerk Authentication Provider

Ivy now supports Clerk as an authentication provider, bringing modern authentication features including OAuth social logins, passwordless authentication, and comprehensive user management.

**Setup:**

```csharp
// Configure in your Program.cs
server.UseAuth<ClerkAuthProvider>(provider => provider
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft()
    .UseApple()
    .UseTwitter()
);
```

**Key features:**

- Email/password and username/password authentication
- Social logins (Google, GitHub, Microsoft, Apple, Twitter)
- Separate development and production environments for safe testing
- JWT-based session tokens with automatic refresh
- Built-in development OAuth credentials for quick local setup
- Session management across multiple tabs and devices

**Configuration:**

Set your Clerk API keys using environment variables or .NET user secrets:

```bash
Clerk:SecretKey=sk_test_...        # or sk_live_... for production
Clerk:PublishableKey=pk_test_...   # or pk_live_... for production
```

The provider automatically detects whether you're using development or production keys and adjusts its behavior accordingly. Development keys (`sk_test_*`, `pk_test_*`) include built-in OAuth credentials, while production keys require custom OAuth app configuration for each social provider.

See the [Clerk authentication documentation](https://docs.ivy-framework.com/authentication/clerk) for detailed setup instructions.

## Error Handling Improvements

### Better Exception Details in Development

When exceptions occur during the initial connection to your Ivy app, you'll now see detailed error information instead of a generic "Not Found" page:

- **Error title**: "Internal Server Error" heading
- **Error message**: The actual exception message explaining what went wrong
- **Stack trace**: Full stack trace for debugging

This makes it much easier to diagnose startup issues and connection errors during development. Previously, exceptions thrown in the AppHub connection handler would display a `NotFoundApp` view, which masked the actual error and made debugging difficult.

## Layout Improvements

### Customizable Sidebar Width

You can now customize the width of sidebars when using `ChromeSettings`. The default sidebar width remains 16rem (256px), but you can now adjust it to fit your application's design:

```csharp
ChromeSettings.Default()
    .Width(Size.Rem(20))  // Wider sidebar
```

## New Widgets

### AI Button Variant

The `Button` widget now includes an eye-catching AI variant with an animated rainbow gradient border, perfect for highlighting AI-powered features and actions:

```csharp
// Basic AI button
new Button("Generate with AI", onClick, variant: ButtonVariant.Ai)

// With icon
new Button("Ask AI", onClick, variant: ButtonVariant.Ai)
    .Icon(Icons.Sparkles)

// Different sizes
new Button("Small AI", onClick, variant: ButtonVariant.Ai).Small()
new Button("Large AI", onClick, variant: ButtonVariant.Ai).Large()

// Fully rounded corners
new Button("AI Assistant", onClick, variant: ButtonVariant.Ai)
    .BorderRadius(BorderRadius.Full)
```

## Widget Updates

### Code Widget XML Language Support

The `Code` widget now supports XML syntax highlighting, making it easier to display configuration files, markup, and structured documents:

```csharp
// Display XML with proper syntax highlighting
new Code("""
    <!-- Project configuration file -->
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
      </PropertyGroup>
    </Project>
    """, Languages.Xml)
```

### DropdownMenu Click Handling Fix

Dropdown menus now properly stop click event propagation, preventing clicks inside dropdown content from triggering actions on parent elements. This fixes issues where selecting a dropdown item could unintentionally trigger click handlers on containers or parent widgets.

### Number Input Width Control

The `NumberInput` widget now supports width customization through the `Width()` extension method, giving you better control over form layouts:

```csharp
// Set a specific width for number inputs
new NumberInput(value, min: 0, max: 100)
    .Width("200px")

// Use with forms for precise layout control
new Form(model)
    | new NumberInput(model, m => m.Quantity)
        .Width("150px")
        .Label("Quantity")
```

### Details Widget Size Control

The `Details` widget now supports three size variants through the scale API, giving you better control over detail view density and typography:

```csharp
// Small size - compact for dense information displays
record.ToDetails().Small()

// Medium size - the default, balanced appearance
record.ToDetails()  // or .Medium()

// Large size - spacious for important information
record.ToDetails().Large()
```

**What changed:**

- Added `.Small()`, `.Medium()`, and `.Large()` extension methods to `DetailsBuilder`
- The scale setting cascades to nested details through context

This is perfect for creating information hierarchies where primary details are larger and more prominent while nested or secondary details can be smaller:

```csharp
var record = new
{
    FirstName = "John",
    LastName = "Doe",
    Age = 30,
    Address = new
    {
        Street = "123 Elm St",
        City = "Springfield",
        State = "IL",
        Zip = "62701"
    }.ToDetails().Small()  // Nested details shown smaller
};

new Card(record.ToDetails().Large())  // Parent details shown larger
```

### Enhanced Card Header Layout

The `Card` widget now supports more flexible header layouts with proper alignment between titles, icons, and other content. Headers can now use full layout widgets for better control:

```csharp
// New approach: Use Layout.Horizontal() for full control
new Card(
    content: "Card content here",
    header: Layout.Horizontal().Align(Align.Center)
            | Text.H4("Card Title").WithLayout().Grow()
            | Icons.Info.ToIcon().Color(Colors.Black)
)
```

**What changed:**

- Card headers can now contain any widget, not just simple text
- Icons and titles automatically align horizontally with proper spacing
- The `Description()` extension method now accepts any object (not just strings), giving you more flexibility with formatting
- The `.Title()` extension method now accepts any object (not just strings), allowing rich widgets as titles

**For metric cards and dashboards**, this makes it much easier to create professional-looking cards with properly aligned icons:

```csharp
new Card(
    content: Layout.Horizontal().Align(Align.Left).Gap(2)
             | Text.Large("$84,250")
             | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
             | Text.Small("21%").Color(Colors.Emerald),
    footer: new Progress(21).Goal("800000"),
    header: Layout.Horizontal().Align(Align.Center)
            | Text.H4("Total Sales").WithLayout().Grow()
            | Icons.DollarSign.ToIcon().Color(Colors.Black)
)
```

**Simplified Header API**: The `.Header()` method now provides a more streamlined way to set both title and description:

```csharp
// Combined title and description
new Card("Content here")
    .Header("Card Title", "This is the description")

// With icon
new Card("Content here")
    .Header(Text.H4("Card Title"), Text.Muted("Description"), Icons.Info)
```

### Simplified `Box` Widget Defaults

The `Box` widget now has cleaner, more neutral defaults that work better as a general-purpose container.

### SelectInput Nullable Value Handling

The `SelectInput` widget now properly handles nullable values when cleared. Both the Toggle and Radio variants correctly set empty values to an empty string instead of `undefined`, ensuring consistent behavior and better compatibility with form validation.

### TableBuilder Reset Method

`TableBuilder` now includes a `Reset()` method that restores all columns to their initial smart defaults, undoing any customizations you've made.

## New Hooks

### `UseRef` Hook for Non-Reactive State

Ivy now includes a `UseRef` hook for storing values that persist across renders without triggering re-renders when they change. This is perfect for storing references, cached values, or internal state that doesn't affect the UI:

```csharp
// Store a value without triggering re-renders
var counterRef = this.UseRef(0);

// Update the value (doesn't cause a re-render)
counterRef.Set(counterRef.Value + 1);

// Access the current value
var currentCount = counterRef.Value;
```

```csharp
// Initialize with a factory function
var expensiveRef = this.UseRef(() => CalculateExpensiveValue());
```

**Key differences from `UseState`:**

- `UseRef` values persist across renders but **don't trigger re-renders** when changed
- `UseState` values trigger re-renders when changed, updating the UI
- Use `UseRef` for internal tracking, timers, previous values, or any state that shouldn't affect rendering

### Improved Reliability for `UseAlert` and `UseTrigger`

The `UseAlert` and `UseTrigger` hooks have been refactored internally to be more reliable and consistent. They now use `UseRef` for internal state tracking (instead of `UseState`), which prevents unnecessary re-renders and improves performance.

## Theming & Design System

### Expanded Color Palette

The design system now includes a comprehensive set of neutral and chromatic colors, all with proper foreground color variants for accessible text. These colors are automatically injected as CSS variables and work seamlessly in both light and dark themes.

**Neutral colors available:**

- Black, White, Slate, Gray, Zinc, Neutral, Stone

**Chromatic colors available:**

- Red, Orange, Amber, Yellow, Lime, Green, Emerald, Teal, Cyan, Sky, Blue, Indigo, Violet, Purple, Fuchsia, Pink, Rose

Each color includes both a background variant and a foreground variant (e.g., `--red` and `--red-foreground`), ensuring text remains readable when placed on colored backgrounds.

## Performance Improvements

### Font Loading Optimization

Ivy now preloads all essential Geist and Geist Mono font weights (Regular, Medium, SemiBold, Bold) in the initial HTML document. This eliminates the font flicker that could occur during page load when the browser discovers fonts late in the rendering process.

## Platform Updates

### .NET 10 Upgrade

Ivy Framework now targets .NET 10, bringing the latest runtime performance improvements and language features. To upgrade your project:

1. Update your project's target framework:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

2. Install the .NET 10 SDK from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

All Ivy packages and dependencies have been updated to support .NET 10, including Entity Framework Core, Microsoft Identity libraries, and other core dependencies.

**Microsoft Entra authentication users**: If you're using the Microsoft Entra auth provider, the underlying Microsoft.Identity.Client library has been updated, which may include security fixes and performance improvements. No code changes are required on your part.

## Security Improvements

### Enhanced URL Validation for Embed and VideoPlayer Widgets

Fixed a security vulnerability in the Embed and VideoPlayer widgets where malicious URLs could potentially bypass hostname validation through substring matching or subdomain attacks. The new validation properly validates hostnames to prevent attacks like:

- `https://evil.com/youtube.com` (substring matching - platform name in path)
- `https://youtube.com.evil.com` (subdomain attack - parent domain)

**What changed:**

- Introduced `validateEmbedUrl()` function with explicit hostname mapping for supported platforms
- Validation now uses exact hostname matching or proper subdomain checking
- Only allows HTTP and HTTPS protocols (blocks `javascript:`, `data:`, `file:`, etc.)
- Hostname comparison is case-insensitive
- Comprehensive test suite with 291 test cases covering security scenarios

**Supported platforms validated:**

- YouTube (`youtube.com`, `youtu.be`)
- Twitter/X (`twitter.com`, `x.com`)
- Facebook, Instagram, TikTok
- LinkedIn, Pinterest (`pinterest.com`, `pin.it`)
- GitHub (`github.com`, `gist.github.com`)
- Reddit

This security fix is automatic and requires no code changes in your applications. Your embed and video player widgets are now protected against URL-based injection attacks.

### Enhanced String Escaping in Document Tools

Fixed a security vulnerability in the document copy-to-markdown functionality where incomplete string escaping could potentially allow injection attacks. The fix ensures proper escaping order when processing table cells:

**What changed:**

- Backslashes are now escaped first (converted to `\\`), before pipe characters
- This prevents edge cases where malformed input could bypass escaping logic
- The escaping order is critical: backslashes must be escaped before other special characters to prevent injection

This security fix applies to the DocumentTools widget's table cell processing and ensures that markdown table generation is safe from injection attacks. The improvement is automatic and requires no code changes in your applications.

## Bug Fixes

### Improved App Routing and Default App Selection

The framework's app routing system has been improved to prevent certain system apps from being automatically selected as the default app

## Widget Updates

### Badge Icon Improvements

The `Badge` widget now has better icon placement and sizing across all scale variants.
