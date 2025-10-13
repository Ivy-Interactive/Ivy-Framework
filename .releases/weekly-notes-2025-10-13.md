# Ivy Framework Weekly Update - October 13, 2025

## Overview

This week's update focuses on maintenance and dependency updates to ensure the framework stays secure and up-to-date with the latest versions of underlying libraries.

## Package Updates

### Backend Dependencies (.NET)

- Updated Microsoft.NET.Test.Sdk to v18.0.0
- Updated Microsoft.CodeAnalysis packages to v4.14.0
- Updated Entity Framework Core packages to v9.0.9
- Updated various authentication and security packages (Auth0, Microsoft Entra, Azure Identity)
- Updated database provider packages (Npgsql, MySQL, SQL Server)
- Updated JSON handling and reactive extensions libraries

### Frontend Dependencies (npm)

- Updated React to v19.2.0 and React DOM to v19.2.0
- Updated CodeMirror language support packages
- Updated Radix UI components to latest versions
- Updated TailwindCSS to v4.1.14
- Updated various development and testing tools

## Documentation Improvements

### MetricView Widget Documentation

The `MetricView` widget now has comprehensive documentation with practical examples. This specialized dashboard component is built on top of `Card` and is designed for displaying business metrics with visual indicators.

Key features documented:

- **Basic Usage**: Display KPIs with trend indicators and goal progress
- **Async Data Loading**: Automatic handling of loading states and error handling
- **Trend Indicators**: Green up arrows for positive trends, red down arrows for negative trends
- **Dashboard Layouts**: Examples of combining multiple MetricViews in grid layouts

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

The documentation includes complete examples for e-commerce and SaaS dashboards, showing real-world usage patterns for tracking revenue, user metrics, and KPIs.

### Table of Contents Enhancement

Fixed an issue where headings from code examples were incorrectly appearing in the Table of Contents. The framework now properly filters out headings that are inside demo boxes (example widgets), ensuring a cleaner and more accurate table of contents structure.

**Technical Details:** The `TableOfContents` widget now uses a `data-demo-box` attribute to identify and exclude headings from example code blocks, preventing them from cluttering the navigation structure.

## Code Input Widget Improvements

### Enhanced Text Selection

The `CodeInputWidget` has been significantly improved with better text selection handling. The previous complex custom selection highlighting system has been replaced with a cleaner, more reliable approach using native CSS selection styling.

**Key Improvements:**

- **Simplified Selection Logic**: Removed complex custom selection decorations in favor of native browser selection
- **Better Visual Feedback**: Text selection now uses a semi-transparent background color that blends with the theme
- **Improved Performance**: Reduced CodeMirror extension complexity for better rendering performance
- **Disabled Conflicting Shortcuts**: Disabled `Ctrl-d` and `Ctrl-Shift-l` shortcuts that could interfere with text selection

**Technical Changes:**

- Replaced custom StateField-based selection tracking with native CSS `::selection` pseudo-element
- Simplified extension setup by removing complex decoration management
- Updated theme configuration to use `color-mix()` for better selection visibility

This improvement makes text selection in code editors more intuitive and visually consistent across different themes and contexts.

## Layout Widgets

### TabsLayout Enhanced Mobile Support

The `TabsLayout` widget now provides improved mobile responsiveness for both Tab and Content variants. When tabs don't fit the available width, they automatically collapse into a dropdown menu, making the interface more usable on smaller screens.

**Key improvements:**

- Both `TabsVariant.Tabs` and `TabsVariant.Content` now support responsive dropdown behavior
- Better width calculation and tab overflow handling
- Enhanced sample app demonstrating responsive behavior with width control

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

The sample app now includes a width slider that lets you test the responsive behavior by adjusting the tabs container width in real-time. This makes it easier to see how tabs will behave on different screen sizes and helps optimize the user experience across devices.

### Enhanced Widget Documentation with Visual Examples

The framework documentation now includes comprehensive visual demonstrations for all widget categories, making it much easier to understand and use Ivy's widgets. Each widget type now comes with complete, runnable code examples in the widget concepts documentation.

**Key Additions:**

#### Complete Widget Demos

- **Common Widgets**: Interactive examples for buttons, badges, progress bars, cards, tables, lists, and tooltips
- **Input Widgets**: All input types including text, number, boolean, file, date/time, color, code, and async select inputs
- **Primitive Widgets**: Text variants, images, icons, avatars, callouts, boxes, separators, and content rendering (JSON, XML, HTML, code)
- **Layout Widgets**: Grid layouts, header/footer layouts, tabs, floating panels, and resizable panel groups
- **Charts**: Line, bar, area, and pie charts with real data examples
- **Effects**: Animation and confetti effects with different triggers
- **Advanced Components**: Sheet panels and chat interfaces

#### Real-World Usage Examples

Each demo shows practical implementation patterns that can be directly copied into applications:

```csharp
// Example from the Common Widgets demo
public class CommonWidgetsDemo : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        return Layout.Grid().Columns(2).Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2)
                    | new Button("Click Me", onClick: _ => client.Toast("Hello!"))
                    | new Button("Destructive").Destructive()
                    | new Button("Secondary").Secondary()
            ).Title("Buttons").Description("Interactive button variants");
    }
}
```

These examples demonstrate best practices for state management, service injection, event handling, and widget composition, making it easier for developers to get started with the framework.

## Embed Widget Enhancements

### GitHub Codespace Support

The Embed widget now supports GitHub Codespace links, allowing users to embed interactive development environments directly in their applications.

**Usage:**

```csharp
new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
```

This enhancement allows developers to:

- Share development environments directly through embeds
- Provide "Open in Codespaces" functionality for repositories
- Create interactive documentation with embedded development setups

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

The sidebar search functionality has been significantly improved with the addition of search hints support. Apps can now include search hints (tags) that make them discoverable through alternative keywords, greatly improving the developer and user experience when navigating large applications.

**Key Features:**

- **Search Hints**: Apps can now specify additional keywords that make them searchable beyond just their title
- **Flexible Search**: Search now matches both app titles and search hints using case-insensitive matching
- **Easy Integration**: Simple attribute-based configuration for adding search hints to any app

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

With this example, users can now find the TextInput app by searching for "password", "textarea", "search", or "email" in addition to "text" or "input".

**Technical Implementation:**

- New `SearchHints` property on `AppAttribute` and `AppDescriptor`
- Enhanced search logic in `DefaultSidebarChrome` that checks both titles and search hints
- Updated `MenuItem` record to support search hints for consistent navigation experience

This improvement makes it much easier to discover relevant functionality in applications with many components, especially useful in sample applications and complex business applications.

## Getting Started & Community

### Enhanced Onboarding Support

We've added a new way to get personalized help with Ivy Framework! You can now [book a free 1-on-1 session](https://calendly.com/mikael-ivy/30min) to get personalized onboarding and support directly from the team.

This complements our existing [waitlist signup](https://ivy.app/join-waitlist) for early access to the framework.

## 🧪 Testing Improvements

### Audio Player Widget - Enhanced Testing Support

Added comprehensive testing capabilities for the Audio Player widget, including a new `TestId()` method for better test automation.

**New API:**

```csharp
// Add test identifiers to Audio widgets for E2E testing
var audio = new Audio("path/to/audio.mp3")
    .TestId("my-audio-player");
```

This enhancement makes it easier to write reliable end-to-end tests for applications using Audio widgets, improving the overall development experience when building audio-enabled applications.

The commit also includes extensive E2E test coverage for the Audio Player widget, testing all widget properties including preload strategies, autoplay behavior, muting capabilities, custom sizing, and theme awareness.

### Badge Widget - Comprehensive Test Suite

Added extensive E2E test coverage for the Badge widget in the Samples application. The new test suite provides comprehensive validation for all badge variants and states:

**Test Coverage Areas:**

- **Variant Testing**: Automated tests for all badge variants (Primary, Destructive, Secondary, Outline, Success, Warning, Info)
- **Size Validation**: Tests for Small, Medium, and Large badge sizes with proper dimension verification
- **Icon Support**: Testing for badges with different icon types (Bell, Heart, Star, Check) and icon positioning (Left/Right)
- **Visual Properties**: Verification of CSS classes, color differences between variants, and proper icon sizing
- **Complex Scenarios**: Combined testing of size and icon properties, multi-badge layouts

The test suite includes over 240 lines of comprehensive Playwright tests, covering smoke tests, visual properties validation, and complex interaction scenarios. This enhancement significantly improves the reliability of the Badge widget and provides a solid testing foundation for contributors working with badge components.

### Button Widget - Enhanced Test Coverage

Added comprehensive E2E test coverage for the Button widget in the Samples application. The new test suite provides thorough validation for all button variants, states, and interaction patterns:

**Test Coverage Areas:**

- **Variant Testing**: Comprehensive tests for all button variants (Primary, Destructive, Secondary, Success, Warning, Info, Outline, Ghost, Link)
- **Size Validation**: Tests for Small, Medium, and Large button sizes with proper dimension hierarchy verification
- **Icon Support**: Testing for buttons with icons in different positions (left, right, icon-only) and various icon types
- **State Management**: Validation of disabled states, loading states with spinners, and interactive feedback
- **Styling Properties**: Tests for specialized button styles (Rounded, Full width, With Tooltip)
- **Accessibility**: Keyboard navigation testing with focus management and Enter key activation
- **Complex Interactions**: Multi-step interaction scenarios combining different button types and states

The test suite includes over 280 lines of comprehensive Playwright tests with advanced features like:

- Size hierarchy validation ensuring proper visual scaling (Small < Medium < Large)
- Interactive state testing with click feedback and demo updates
- Icon positioning and aspect ratio verification for icon-only buttons
- Tooltip interaction testing with hover states
- Complex multi-step user workflows combining various button interactions

This enhancement significantly improves the reliability of the Button widget and provides a comprehensive testing foundation for one of the most commonly used UI components in the framework.

## Notes

These updates primarily improve security, performance, and compatibility with newer tooling. No breaking changes or new user-facing features were introduced in this update.
