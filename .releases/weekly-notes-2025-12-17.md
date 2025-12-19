# Ivy Framework Weekly Notes - Week of 2025-12-17

## Breaking Changes

### .NET 10 Upgrade

Ivy Framework now targets .NET 10, bringing the latest runtime performance improvements and language features. To upgrade your project:

1. Install the .NET 10 SDK from [Microsoft .NET download page](https://dotnet.microsoft.com/download)

2. Update your project's target framework:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

### File Based Apps Support

Ivy works great with .NET 10's file based apps feature, allowing you to create self-contained .NET apps with a single file.

<https://github.com/user-attachments/assets/0ecebc3b-37f3-47c7-84e0-7778db25036a>

## Authentication Improvements

### HTTP Tunneling for Authentication

Authentication providers can now make HTTP requests through the frontend using a new tunneling system. This enables auth providers to communicate with external APIs securely by routing requests through the client's browser, which is particularly useful for OAuth flows and development mode scenarios where the backend can't directly reach certain endpoints.

The implementation includes:

* `HttpTunnelingController` for handling tunneled requests
* `TunneledHttpMessageHandler` that integrates with .NET's `HttpClient`
* Automatic request/response serialization and header forwarding
* 30-second timeout with proper cancellation handling

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

* Email/password and username/password authentication
* Social logins (Google, GitHub, Microsoft, Apple, Twitter)
* Separate development and production environments for safe testing
* JWT-based session tokens with automatic refresh
* Built-in development OAuth credentials for quick local setup
* Session management across multiple tabs and devices

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

* **Sign in once, authenticated everywhere**: Sign in on one tab and all your other open tabs instantly get authenticated without manual refresh
* **Sign out once, logged out everywhere**: Logging out in one tab immediately logs you out across all tabs for better security
* **Automatic session recovery**: Opening a new tab picks up your existing authentication state

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

<https://github.com/user-attachments/assets/64d7913e-34b8-4911-a763-b170d0447fe0>

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

The `NumberInput` widget now supports explicit width customization through the `.Width()` extension method, allowing for better control over form layouts.

The `Details` widget now supports cascading size variants (`.Small()`, `.Medium()`, `.Large()`) for improved density control. Furthermore, Card headers now support full layout widgets for rich content, accompanied by a streamlined `.Header()` API for setting titles and descriptions simultaneously.

### SelectInput Nullable Value Handling

The `SelectInput` widget now properly handles nullable values when cleared. Both the Toggle and Radio variants correctly set empty values to an empty string instead of `undefined`, ensuring consistent behavior and better compatibility with form validation.

## New Hooks

### `UseRef` Hook for Non-Reactive State

Ivy now includes a `UseRef` hook for storing values that persist across renders without triggering re-renders, similar to React's useRef.

```csharp
var counterRef = this.UseRef(0);
counterRef.Set(counterRef.Value + 1); // No re-render
```

**Key differences from `UseState`:**

* `UseRef` values persist across renders but **don't trigger re-renders** when changed
* `UseState` values trigger re-renders when changed, updating the UI
* Use `UseRef` for internal tracking, timers, previous values, or any state that shouldn't affect rendering

### Improved Reliability for `UseAlert` and `UseTrigger`

The `UseAlert` and `UseTrigger` hooks have been refactored internally to be more reliable and consistent. They now use `UseRef` for internal state tracking (instead of `UseState`), which prevents unnecessary re-renders and improves performance.

## Theming & Design System

### Expanded Color Palette

The design system now includes a comprehensive set of neutral and chromatic colors (Slate, Zinc, Red, Emerald, Sky, Indigo, etc.), each with proper foreground variants for accessibility.

**Neutral colors available:**

* Black, White, Slate, Gray, Zinc, Neutral, Stone

**Chromatic colors available:**

* Red, Orange, Amber, Yellow, Lime, Green, Emerald, Teal, Cyan, Sky, Blue, Indigo, Violet, Purple, Fuchsia, Pink, Rose

## Performance Improvements

### Font Loading Optimization

Ivy now preloads all essential Geist and Geist Mono font weights (Regular, Medium, SemiBold, Bold) in the initial HTML document. This eliminates the font flicker that could occur during page load when the browser discovers fonts late in the rendering process.

The `Embed` and `VideoPlayer` widgets now feature enhanced URL validation to automatically protect against hostname-based injection attacks.

## Developer Tools

* **Beta Roslyn Analyzer**: Strictly enforces Rules of Hooks at compile time, catching conditional hooks or hooks in loops. Implemented in `Ivy.Analyser`, but is currently in beta and subject to change.
* **Widget Tree Debugging**: Enable detailed logging with the `IVY_DUMP_WIDGET_TREES=1` environment variable.

## Layout Improvements

* **Loading Widget**: Simplified internal state for more lightweight and direct rendering.
* **Code Widget**: Added XML syntax highlighting support via `Languages.Xml`.
* **Callout Colors**: Now uses consistent text-foreground colors across all variants for better readability.
* **Chart Tooltips**: Improved positioning logic ensures tooltips are no longer cut off by container boundaries.
* **EmbedCard Focus**: Removed the green focus ring for a cleaner appearance during keyboard navigation.
* **Details Scaling**: Now supports `.Small()`, `.Medium()`, and `.Large()` variants with refined typography scaling.
* **Card Headers**: Added support for full layout widgets, allowing precise alignment and rich content.
* **Box Defaults**: Now defaults to a neutral appearance with no background and a subtle border.
* **SelectInput**: Properly handles nullable values by setting cleared fields to an empty string.
* **TableBuilder**: Added a `Reset()` method to restore all columns to their initial smart defaults.

## Bug Fixes

* **App Routing**: Improved system to prevent internal system apps from being automatically selected as defaults.
* **DropdownMenu**: Fixed click event propagation to prevent selection from triggering parent click handlers.

## Widget Updates

* **Badge Icons**: Improved placement and scaling across all size variants.

## What's Changed

* (card): align titles with horizontal content by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1722>
* (fix): Fixed inconsistencies in the Forms app (Ivy.Samples) by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1758>
* (security): fix Incomplete string escaping or encoding by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1724>
* (forms): update validation section and clean up form test apps by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1728>
* (security): fix url sanitization warning in the embed widget by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1723>
* [Docs]: Add interactive chat with streaming output example by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1716>
* (chore): Added Form apps into one menu folder. Removed FromSizeApp by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1692>
* docs: remove several Json widget examples from documentation by @joshuauaua in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1801>
* Update README to remove sign-up links by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1806>
* [Docs]: adjust Setters column width in Properties table by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1805>
* (fix): Added width inline style for number input by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1807>
* [FileInput]: enable tooltip for invalid icon by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1804>
* (chore): Readded Form app with scaffolding after compile errors by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1803>
* (chore): Ability to set new sidebar width by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1721>
* (theme): Added neutral and chromatic colors to design system and themeing by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1720>
* [Details]: implement scale api  by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1680>
* (docs): a lot of bug fixes by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1817>
* [badge]: improved icon placement and styles by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1821>
* [blades]: Set minimum height for blade demo containers in documentation by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1824>
* (kanban): remove margin top by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1826>
* [dropdown]: stop click propagation in DropdownMenuContent by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1828>
* Changed hardcoded color to Ivy Design color by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1836>
* (fix): Fixed text cutoff in charts by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1835>
* (fix): Fixed bug for AsyncSelect Icon placement by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1832>
* [forms]: fix multiple boolean input validation error by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1823>
* (button): create AI animated button by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1811>
* [blades]: fixed spacing issue in blades header by @joshuauaua in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1830>
* (callout): use text-foreground on callouts by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1837>
* fix(docs): spaces in title "C L I Overview" by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1839>
* [code]: Add XML syntax highlighting support for code blocks by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1841>
* (docs): rewrite frontend architecture documentation by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1838>
* [fonts]: prevent font flicker by changing font-display to fallback by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1829>
* (codex): implement hook rules by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1550>
* (codex): upgrade to .NET 10 by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1825>
* [details]: Update samples with better Details demonstration by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1850>
* (fix): Email validation checks for dots after @ symbol now by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1854>
* feat: add Clerk auth provider by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1452>
* [SelectInput]: nullable when cleared should have no selected value by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1853>

**Full Changelog**: <https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.1.4...v1.2.5>
