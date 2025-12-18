# Ivy Framework Weekly Notes - Week of 2025-12-17

## Breaking Changes

### .NET 10 Upgrade

Ivy Framework now targets .NET 10, bringing the latest runtime performance improvements and language features. To upgrade your project:

1. Update your project's target framework:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

2. Install the .NET 10 SDK from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

All Ivy packages and dependencies have been updated to support .NET 10.

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

https://github.com/user-attachments/assets/0ecebc3b-37f3-47c7-84e0-7778db25036a

**Microsoft Entra authentication users**: If you're using the Microsoft Entra auth provider, the underlying Microsoft.Identity.Client library has been updated, which may include security fixes and performance improvements. No code changes are required on your part.

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

### Multi-Tab Authentication Synchronization

Authentication now works seamlessly across multiple tabs and windows. When you sign in or sign out in one tab, all other tabs from the same browser automatically sync.

**Key capabilities:**

- **Sign in once, authenticated everywhere**: Sign in on one tab and all your other open tabs instantly get authenticated without manual refresh
- **Sign out once, logged out everywhere**: Logging out in one tab immediately logs you out across all tabs for better security
- **Automatic session recovery**: Opening a new tab picks up your existing authentication state

### New `IAuthSession` Interface

The authentication system now uses `IAuthSession` instead of passing tokens directly. This provides better encapsulation and type safety:

```csharp
public interface IAuthSession
{
    AuthToken? AuthToken { get; set; }
    string? AuthSessionData { get; set; }
}
```

**For custom auth provider implementations**, you'll need to update your method signatures:

```csharp
// New way
public Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
{
    // Access token via authSession.AuthToken
    // Store additional session data in authSession.AuthSessionData
}
```

All `IAuthProvider` methods now receive an `IAuthSession` parameter instead of raw tokens.

### Session Data Storage

You can now store additional authentication-related data beyond just access tokens:

```csharp
// Store custom session data (automatically serialized to JSON)
authSession.SetAuthSessionData(new { UserId = "123", Preferences = userPrefs });

// Retrieve session data
var sessionData = authSession.GetAuthSessionData<MySessionData>();
```

Session data is stored in cookies and persists across page reloads.

### Centralized JSON Serialization

A new `Ivy.Core.Helpers.JsonHelper` class has been introduced to centralize `JsonSerializerOptions` across the framework. This ensures consistent serialization behavior and improves compatibility with Native AOT and Single-File publishing.

```csharp
// Use the centralized options for consistent serialization
var json = JsonSerializer.Serialize(myObject, JsonHelper.DefaultOptions);
```

This ensures that all parts of the framework use the same serialization settings, reducing subtle bugs related to naming policies or type resolution.

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
new Button("Generate with AI", onClick, variant: ButtonVariant.Ai)
    .Icon(Icons.Sparkles)
```

https://github.com/user-attachments/assets/64d7913e-34b8-4911-a763-b170d0447fe0

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

<img width="1276" height="312" alt="image" src="https://github.com/user-attachments/assets/efbcf1ab-a5e8-4e6a-9ca2-fd31326d16c5" />


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
<img width="2284" height="775" alt="image" src="https://github.com/user-attachments/assets/ff78f8a8-bf08-4403-a63a-0e610eecc4ee" />


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

<img width="598" height="235" alt="image" src="https://github.com/user-attachments/assets/3cb4f82a-4a89-4abe-8ebf-4f95c7df9396" />

**Simplified Header API**: The `.Header()` method now provides a more streamlined way to set both title and description:

```csharp
// Combined title and description
new Card("Content here")
    .Header("Card Title", "This is the description")
```

<img width="580" height="192" alt="image" src="https://github.com/user-attachments/assets/b852351d-b6ad-4430-a1f0-9a17fe9c7303" />


### Simplified `Box` Widget Defaults

The `Box` widget now has cleaner, more neutral defaults that work better as a general-purpose container.

### SelectInput Nullable Value Handling

The `SelectInput` widget now properly handles nullable values when cleared. Both the Toggle and Radio variants correctly set empty values to an empty string instead of `undefined`, ensuring consistent behavior and better compatibility with form validation.

### TableBuilder Reset Method

`TableBuilder` now includes a `Reset()` method that restores all columns to their initial smart defaults, undoing any customizations you've made.

## New Hooks

### `UseRef` Hook for Non-Reactive State

Ivy now includes a `UseRef` hook for storing values that persist across renders without triggering re-renders, similar to React's useRef.

```csharp
var counterRef = this.UseRef(0);
counterRef.Set(counterRef.Value + 1); // No re-render
```

**Key differences from `UseState`:**

- `UseRef` values persist across renders but **don't trigger re-renders** when changed
- `UseState` values trigger re-renders when changed, updating the UI
- Use `UseRef` for internal tracking, timers, previous values, or any state that shouldn't affect rendering

### Improved Reliability for `UseAlert` and `UseTrigger`

The `UseAlert` and `UseTrigger` hooks have been refactored internally to be more reliable and consistent. They now use `UseRef` for internal state tracking (instead of `UseState`), which prevents unnecessary re-renders and improves performance.

## Theming & Design System

### Expanded Color Palette

The design system now includes a comprehensive set of neutral and chromatic colors (Slate, Zinc, Red, Emerald, Sky, Indigo, etc.), each with proper foreground variants for accessibility.

**Neutral colors available:**

- Black, White, Slate, Gray, Zinc, Neutral, Stone

**Chromatic colors available:**

- Red, Orange, Amber, Yellow, Lime, Green, Emerald, Teal, Cyan, Sky, Blue, Indigo, Violet, Purple, Fuchsia, Pink, Rose

## Performance Improvements

### Font Loading Optimization

Ivy now preloads all essential Geist and Geist Mono font weights (Regular, Medium, SemiBold, Bold) in the initial HTML document. This eliminates the font flicker that could occur during page load when the browser discovers fonts late in the rendering process.

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

## Developer Tools

### Enhanced Roslyn Analyzer for Hook Rules

The `Ivy.Analyser` package now strictly enforces Rules of Hooks at compile time, catching errors like conditional hooks or hooks in loops.

### Widget Tree Debug Logging

You can now enable detailed widget tree update logging by setting `IVY_DUMP_WIDGET_TREES=1` environment variable.

## Layout Improvements

### Loading Widget Simplification

The `Loading` widget has been simplified to render directly without complex internal state. This makes it more lightweight and easier to use for simple loading indicators throughout your application.

### Code Widget XML Language Support

The `Code` widget now supports XML syntax highlighting (`Languages.Xml`).

### Callout Text Color Consistency

The `Callout` widget now uses consistent text colors across all variants, improving readability and accessibility.

### Chart Tooltip Improvements

Chart tooltips now render correctly without being cut off by container boundaries, thanks to improved positioning logic.

### EmbedCard Focus Ring Removal

The `EmbedCard` widget no longer shows a green focus ring when keyboard navigating to embedded links, providing a cleaner visual appearance.

### Details Widget Size Control

The `Details` widget now supports `.Small()`, `.Medium()`, and `.Large()` size variants, with refined padding and typography scaling.

### Enhanced Card Header Layout

Card headers now support full layout widgets for better control over alignment and content.

### Simplified Box Widget Defaults

The `Box` widget now defaults to a cleaner, neutral appearance (no background color, 1px border) instead of the previous primary-colored default.

### SelectInput Nullable Value Handling

The `SelectInput` widget now properly handles nullable values when cleared, setting them to an empty string instead of `undefined`.

### TableBuilder Reset Method

`TableBuilder` now includes a `Reset()` method that restores all columns to their initial defaults.

## Bug Fixes

### Improved App Routing and Default App Selection

The framework's app routing system has been improved to prevent certain system apps from being automatically selected as the default app


### DropdownMenu Click Handling Fix

Dropdown menus now properly stop click event propagation, preventing clicks inside dropdown content from triggering actions on parent elements. This fixes issues where selecting a dropdown item could unintentionally trigger click handlers on containers or parent widgets.

## Widget Updates

### Badge Icon Improvements

The `Badge` widget now has better icon placement and sizing across all scale variants.

## What's Changed

- (fix): Email validation checks for dots after @ symbol now by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1854
- [SelectInput]: nullable when cleared should have no selected value by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1853
- [details]: Update samples with better Details demonstration by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1850
- Restructure HTTP tunnel error response null check for clarity by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1845
- [code]: Add XML syntax highlighting support for code blocks by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1841
- fix(docs): spaces in title "C L I Overview" by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1839
- (docs): rewrite frontend architecture documentation by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1838
- (callout): use text-foreground on callouts by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1837
- Changed hardcoded color to Ivy Design color by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1836
- (fix): Fixed text cutoff in charts by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1835
- (fix): Fixed bug for AsyncSelect Icon placement by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1832
- [blades]: fixed spacing issue in blades header by @joshuauaua in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1830
- [fonts]: prevent font flicker by changing font-display to fallback by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1829
- [dropdown]: stop click propagation in DropdownMenuContent by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1828
- (kanban): remove margin top by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1826
- (codex): upgrade to .NET 10 by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1825
- [blades]: Set minimum height for blade demo containers in documentation by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1824
- [forms]: fix multiple boolean input validation error by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1823
- [badge]: improved icon placement and styles by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1821
- (docs): a lot of bug fixes by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1817
- (button): create AI animated button by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1811
- (fix): Added width inline style for number input by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1807
- Update README to remove sign-up links by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1806
- [Docs]: adjust Setters column width in Properties table by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1805
- [FileInput]: enable tooltip for invalid icon by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1804
- (chore): Readded Form app with scaffolding after compile errors by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1803
- docs: remove several Json widget examples from documentation by @joshuauaua in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1801
- (chrome): handle default page padding for chrome false mode by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1785
- refactor: remove Box.Plain() extension by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1759
- (fix): Fixed inconsistencies in the Forms app (Ivy.Samples) by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1758
- Add UseRef hook and enhance UseAlert and UseTrigger logic by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1757
