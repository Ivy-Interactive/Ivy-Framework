# Ivy Framework Weekly Notes - Week of 2026-02-06

## LLM Framework Documentation

Added comprehensive framework documentation specifically designed for AI assistants and LLMs working with Ivy. The new `AGENTS.md` guide provides a complete introduction to the framework, including:

- Core concepts: Views, Widgets, Hooks, and Apps
- Common widgets with links to detailed documentation
- Layout system overview with code examples
- Text helpers and styling modifiers
- Event handling patterns
- Complete hooks reference (UseState, UseEffect, UseMemo, etc.)
- Input binding with state
- Best practices for building Ivy applications

This documentation serves as a quick reference for AI-assisted development and includes all essential patterns and APIs needed to work effectively with Ivy.

## New Widgets

### Terminal Widget

The `Terminal` widget has been promoted from internal API to a public primitive widget. You can now use it to display terminal-like output in your applications.

```csharp
new Terminal()
    .Lines(new[]
    {
        new TerminalLine("$ npm install", IsCommand: true),
        new TerminalLine("Installing packages..."),
        new TerminalLine("Done!")
    })
```

## New Features

### Configurable Authentication Cookie Options

You can now customize authentication cookie settings when configuring your Ivy server.

Configure authentication cookies in your `Program.cs`:

```csharp
// Configure authentication cookie options
Server.ConfigureAuthCookieOptions = options =>
{
    options.Domain = ".yourdomain.com";
    options.SameSite = SameSiteMode.Strict;
    options.Expires = DateTimeOffset.UtcNow.AddDays(30);
    options.IsEssential = true;
};

var server = new Server()
    .UseAuth<YourAuthProvider>();
```

## Widget Enhancements

All widgets now better support theming. We have also updated the theme customizer app. This also includes improvements to Scale API, default margins and paddings.

### ColorInput Swatch Variant

The `ColorInput` widget now includes a new **Swatch** variant that displays a grid of predefined theme-aware colors. Instead of entering hex codes or using a color picker, users can select from the full set of Ivy theme colors (`Colors` enum) presented in a clean, visual grid.

When you bind a `ColorInput` to a `Colors` enum state, the Swatch variant is automatically selected, making color selection more intuitive:

```csharp
// Automatic swatch for Colors enum
var colorState = UseState(Colors.Blue);
colorState.ToColorInput()  // Automatically uses Swatch variant

// Or explicitly set the variant
var colorState = UseState("red");
colorState.ToColorInput().Variant(ColorInputs.Swatch)
```

## Breaking Changes

### Card Border Customization Removed

The `Card` widget no longer supports border customization methods. Previously, you could customize card borders with methods like `.BorderThickness()`, `.BorderStyle()`, `.BorderColor()`, and `.BorderRadius()`. These APIs have been removed to simplify the Card widget and maintain consistent styling.

### Table Default Width Behavior

Tables now default to `Size.Full()` width instead of a calculated smart width. Previously, the framework would automatically calculate table widths based on column content (between 100-400 units).

### Text.Literal() No Longer Supports Styling Methods

The `Text.Literal()` method has been streamlined and now returns a `TextBlock` directly instead of a `TextBuilder`. This means you can no longer chain styling methods like `.Bold()`, `.Italic()`, or `.Color()` on literal text.

## Bug Fixes

### Navigation State Preservation on Reconnect

Fixed an issue where navigation arguments were lost when the application reconnected after a disconnection. The framework now properly preserves and restores all navigation state when reconnecting:

- **App Arguments Restoration**: When navigating to an app after reconnection, the framework now passes along any app arguments that were present before the disconnection
- **State Continuity**: Users will no longer lose their navigation context when experiencing temporary connection issues
- **Intentional Disconnect Handling**: The disconnect modal is now suppressed when the connection is intentionally closed (e.g., during cleanup), preventing misleading error messages/

### MetricView API

The `MetricView` constructor signature requires the icon as the second parameter (can be null):

```csharp
// Without icon
new MetricView("Total Sales", null, ctx => UseStaticMetric(ctx, ...))

// With icon
new MetricView("Total Sales", Icons.DollarSign, ctx => UseStaticMetric(ctx, ...))
```

### GridLayout Cell Text Overflow

The `GridLayout` cell overflow behavior has been simplified. Previously, the grid applied complex hover behavior where truncated text would expand on hover using absolute positioning. This hover-to-reveal functionality has been removed in favor of simpler, more predictable cell rendering.

### Form Scaffolding Nullable Input Detection

Fixed an issue where the form scaffolder wasn't properly detecting nullable properties and fields when automatically generating form inputs. The scaffolder now correctly identifies nullable types and applies the appropriate behavior:

- **Nullable Detection**: The scaffolder now uses `NullabilityInfoContext` to accurately detect nullable reference types in addition to nullable value types
- **Clear Button Logic**: Input widgets only show the clear (X) button for nullable fields that are not marked as required
- **Consistent Behavior**: All input types (text, email, phone, URL, password, number, date/time, bool, color, select) now properly respect nullable annotations
