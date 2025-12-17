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

If you have the root `Ivy` namespace already imported, no changes are needed.

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

## New Features

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

## Documentation Improvements

### Blade Component Layout Structure

The Blades documentation has been updated to show the proper layout structure for blade components. When using the `UseBlades` hook, wrap the blade view in a horizontal layout with an explicit height to ensure proper rendering.

### Form Validation Best Practices

The Forms documentation has been updated with comprehensive validation examples:

- **Prefer `[Length]` over `[StringLength]`**: The modern `[Length]` attribute is now recommended.
- **Collection Validation**: `[AllowedValues]` now works correctly on collection properties.
- **Improved Email Validation**: Uses `MailAddress` for robust validation, requiring a domain with at least one dot.

### Apps and the `[App]` Attribute

New comprehensive documentation explains how to create and configure apps using the `[App]` attribute, covering route generation, navigation integration, and search hints.

### Stepper Widget Documentation

The `Stepper` widget now has comprehensive documentation with interactive examples showing how to build step-by-step wizards.

### Chat Widget with Streaming Responses

The `Chat` widget documentation now includes an interactive example showing how to implement real-time streaming responses.

### Sheet Widget with Triggers Pattern

The `Sheet` widget documentation now details the "Triggers" pattern for opening sheets with parameters, perfect for edit dialogs and detail views.

## Layout Improvements

### Customizable Sidebar Width

You can now customize the width of sidebars when using `ChromeSettings`. The default sidebar width remains 16rem (256px), but can be adjusted:

```csharp
ChromeSettings.Default()
    .Width(Size.Rem(20))  // Wider sidebar
```

## New Widgets

### AI Button Variant

The `Button` widget now includes an eye-catching AI variant with an animated rainbow gradient border.

```csharp
new Button("Generate with AI", onClick, variant: ButtonVariant.Ai)
    .Icon(Icons.Sparkles)
```

### Expandable Widget

The new `Expandable` widget allows you to create collapsible content sections.

```csharp
new Expandable("Click to expand", "Hidden content")
```

### AsyncSelectInput Widget

The new `AsyncSelectInput` widget provides a powerful async dropdown with search, perfect for loading options from APIs.

```csharp
new AsyncSelectInput<User>(
    selectedUser,
    searchTerm => LoadUsersFromApi(searchTerm),
    user => user.Name
)
```

## Widget Updates

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

## New Hooks

### `UseRef` Hook

Ivy now includes a `UseRef` hook for storing values that persist across renders without triggering re-renders, similar to React's useRef.

```csharp
var counterRef = this.UseRef(0);
counterRef.Set(counterRef.Value + 1); // No re-render
```

### Improved Reliability for `UseAlert` and `UseTrigger`

These hooks have been refactored internally for better stability and edge-case handling.

## Theming & Design System

### Expanded Color Palette

The design system now includes a comprehensive set of neutral and chromatic colors (Slate, Zinc, Red, Emerald, Sky, Indigo, etc.), each with proper foreground variants for accessibility.

## Performance Improvements

### Font Loading Optimization

Ivy now preloads essential font weights (Geist, Geist Mono) to eliminate font flicker on page load.

## Developer Tools

### Enhanced Roslyn Analyzer for Hook Rules

The `Ivy.Analyser` package now strictly enforces Rules of Hooks at compile time, catching errors like conditional hooks or hooks in loops.

### Widget Tree Debug Logging

You can now enable detailed widget tree update logging by setting `IVY_DUMP_WIDGET_TREES=1` environment variable.
