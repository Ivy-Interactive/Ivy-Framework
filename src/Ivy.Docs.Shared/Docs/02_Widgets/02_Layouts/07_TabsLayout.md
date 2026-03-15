---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - navigation
  - panels
  - pages
  - switcher
  - tabbed
  - sections
---

# TabsLayout

The TabsLayout [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) creates a tabbed [interface](../../01_Onboarding/02_Concepts/02_Views.md) that allows users to switch between different content sections. It supports both traditional tabs and content-based variants, with features such as closable tabs, [badges](../03_Common/02_Badge.md), [icons](../01_Primitives/02_Icon.md), and drag-and-drop reordering.

## Basic Usage

We recommend using Layout.Tabs to create simple tabbed interfaces.

```csharp demo-tabs
Layout.Tabs(
    new Tab("Profile", "User profile information"),
    new Tab("Security", "Security settings"),
    new Tab("Preferences", "User preferences")
)
```

This example creates a basic layout with three tabs.

### TabView with Customization

This example demonstrates how to combine multiple TabView features, including icons, badges, variant selection, and size control:

```csharp demo-tabs
Layout.Tabs(
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
)
```

It showcases the fluent API of TabView, which allows chaining multiple configuration methods for a complete tab setup with visual indicators and precise layout control.

## TabsLayout usage

If you need more flexibility in creating and managing tabs, TabsLayout offers a comprehensive API for enhanced tab configuration.

The first parameter is the selected tab index (0), and the remaining parameters are the Tab objects.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "This is the overview content"),
    new Tab("Details", "This is the details content"),
    new Tab("Settings", "This is the settings content")
)
```

### With Event Handlers

- `onSelect`: Handles tab selection events
- `onClose`: Adds close functionality to tabs
- `onRefresh`: Adds refresh buttons to tabs
- `onReorder`: Enables drag-and-drop tab reordering
- `selectedIndex`: Sets the initially selected tab

This example demonstrates how to handle all available events. The event handlers receive the tab index and can perform custom actions such as logging, [state](../../03_Hooks/02_Core/03_UseState.md) updates, or API calls.

```csharp demo-tabs
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: (e) => Console.WriteLine($"Closed: {e.Value}"),
    onRefresh: (e) => Console.WriteLine($"Refreshed: {e.Value}"),
    onReorder: null,
    selectedIndex: 0,
    new Tab("Tab 1", "Content 1"),
    new Tab("Tab 2", "Content 2"),
    new Tab("Tab 3", "Content 3")
)
```

## Variant usage

The default variant is `Content`, which emphasizes the content area. Use `TabsVariant.Tabs` only when tab navigation is itself a primary UI concern (e.g., a settings page with many sections).

### Content Variant (default)

The Content variant emphasizes the content area with subtle tab indicators. This is ideal for content-heavy apps where the focus should be on the displayed information. Since `Content` is the default, you don't need to specify it explicitly.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "Overview content here"),
    new Tab("Details", "Detailed information here"),
    new Tab("Settings", "Configuration options here")
)
```

### Tabs Variant

The Tabs variant displays tabs as clickable buttons with an underline indicator for the active tab, providing a traditional tab navigation interface.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("First", "First tab content"),
    new Tab("Second", "Second tab content"),
    new Tab("Third", "Third tab content")
).Variant(TabsVariant.Tabs)
```

## Customize

### With Icons and Badges

Enhance tabs with icons and badges for better visual representation:

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
)
```

## Responsive Overflow

When there are many tabs that don't fit in the available width, the component automatically shows a dropdown menu for hidden tabs. Try resizing your browser window to see this in action.

```csharp demo-tabs
Layout.Tabs(
    new Tab("Home", "Home content"),
    new Tab("Products", "Products content"),
    new Tab("Services", "Services content"),
    new Tab("About", "About content"),
    new Tab("Contact", "Contact content"),
    new Tab("Blog", "Blog content"),
    new Tab("FAQ", "FAQ content"),
    new Tab("Support", "Support content"),
    new Tab("Careers", "Careers content"),
    new Tab("Partners", "Partners content"),
    new Tab("Pricing", "Pricing content"),
    new Tab("Documentation", "Documentation content"),
    new Tab("Community", "Community content")
)
```

<WidgetDocs Type="Ivy.TabsLayout" ExtensionTypes="Ivy.TabsLayoutExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Layouts/TabsLayout.cs"/>
