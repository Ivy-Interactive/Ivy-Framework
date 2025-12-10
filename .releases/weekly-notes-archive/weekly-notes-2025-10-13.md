# Ivy Framework Weekly Update - October 13, 2025

## Overview

This week's update focuses on maintenance and dependency updates to ensure the framework stays secure and up-to-date with the latest versions of underlying libraries.

## Documentation Improvements

### MetricView Widget Documentation

The `MetricView` widget now has comprehensive documentation with practical examples. This specialized dashboard component is built on top of `Card` and is designed for displaying business metrics with visual indicators.

#### Usage Example

```csharp
new MetricView(
    "Total Sales",  
    Icons.DollarSign,  
    () => Task.FromResult(new MetricRecord(
        "$84,250",      // Current metric value
        0.21,           // 21% increase from previous period
        0.21,           // 21% of goal achieved
        "$800,000"      // Goal target
    ))
)
```

### Table of Contents Enhancement

Fixed an issue where headings from code examples were incorrectly appearing in the table of contents.

## Code Input Widget Improvements

### Enhanced Text Selection

The `CodeInputWidget` has been significantly improved with better text selection handling. This now also works better across different themes.

## Layout Widgets

### TabsLayout Enhanced Mobile Support

The `TabsLayout` widget now provides improved mobile responsiveness for both Tab and Content variants. When tabs don't fit the available width, they automatically collapse into a dropdown menu, making the interface more usable on smaller screens.

**Example usage:**

```csharp
// Create a tabs layout that adapts to different widths
var tabsLayout = new TabsLayout(OnTabSelect, OnTabClose, null, null, selectedIndex.Value,
    tabs.Value.ToArray()
).Variant(TabsVariant.Content).Width(0.8); // 80% width

// For responsive behavior, you can bind to a state
var width = this.UseState(1.0);
var responsiveTabsLayout = new TabsLayout(OnTabSelect, OnTabClose, null, null, selectedIndex.Value,
    tabs.Value.ToArray()
).Variant(TabsVariant.Tabs).Width(width.Value);
```

The sample app now includes a width slider that lets you test the responsive behavior by adjusting the tabs container width in real-time.

### Enhanced Widget Documentation with Visual Examples

The framework documentation now includes comprehensive visual demonstrations for all widget categories, making it much easier to understand and use Ivy's widgets. Each widget type now comes with complete, runnable code examples in the widget concepts documentation.

## Embed Widget Enhancements

### GitHub Codespace Support

The Embed widget now supports GitHub Codespace links, allowing users to embed interactive development environments directly in their applications.

**Usage:**

```csharp
new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
```

### Enhanced Responsive Design

- Improved responsive design for embed cards with container queries (`@container`)
- Better layout handling with two display modes:
  - **Button layout** (on wider screens): Shows full title, description, and action button
  - **Clickable card layout** (on narrower screens): Compact format with the entire card clickable
- Enhanced error handling with more descriptive error messages and appropriate icons
- Improved spacing in sidebar layouts for better embed display

The embed widget now automatically adapts between layouts for better user experience across devices, making embedded content more accessible on mobile and desktop.

## Sidebar Navigation

### Enhanced Search with Search Hints

The sidebar search functionality has been significantly improved with the addition of search hints support. Apps can now include search hints (tags) that make them discoverable through alternative keywords.

**Usage Example:**

```csharp
[App(icon: Icons.TextCursorInput, 
     path: ["Widgets", "Inputs"], 
     searchHints: ["password", "textarea", "search", "email"])]
public class TextInputApp : SampleBase
{
    // App implementation
}
```

## Getting Started & Community

### Enhanced Onboarding Support

We've added a new way to get personalized help with Ivy Framework! You can now [book a free 1-on-1 session](https://calendly.com/mikael-ivy/30min) to get personalized onboarding and support directly from the team.

This complements our existing [waitlist signup](https://ivy.app/join-waitlist) for early access to the framework.

## Testing Improvements

**New Fully E2E Tested Components:**

- Audio Player Widget - includes `TestId()` method for test automation
- Badge Widget - all variants, sizes, and icon combinations
- Button Widget - all variants, states, and accessibility features

## Notes

These updates primarily improve security, performance, and compatibility with newer tooling. No breaking changes or new user-facing features were introduced in this update.
