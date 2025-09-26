---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# TabsLayout

The TabsLayout widget creates a tabbed interface that allows users to switch between different content sections. It supports both traditional tabs and content-based variants, with features such as closable tabs, badges, icons, and drag-and-drop reordering.

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
.Variant(TabsVariant.Tabs)
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

This example demonstrates how to handle all available events. The event handlers receive the tab index and can perform custom actions such as logging, state updates, or API calls.

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

### Tabs Variant

The Tabs variant displays tabs as clickable buttons with an underline indicator for the active tab, providing a traditional tab interface.

```csharp demo-tabs 
new TabsLayout(null, null, null, null, 0,
    new Tab("First", "First tab content"),
    new Tab("Second", "Second tab content"),
    new Tab("Third", "Third tab content")
).Variant(TabsVariant.Tabs)
```

### Content Variant

The Content variant emphasizes the content area with subtle tab indicators. This is ideal for content-heavy apps where the focus should be on the displayed information.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "Overview content here"),
    new Tab("Details", "Detailed information here"),
    new Tab("Settings", "Configuration options here")
).Variant(TabsVariant.Content)
```

## Customize

### With Icons and Badges

Enhance tabs with icons and badges for better visual representation:

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
).Variant(TabsVariant.Tabs)
```

<WidgetDocs Type="Ivy.TabsLayout" ExtensionTypes="Ivy.Views.Tabs.TabsLayoutExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/TabsLayout/TabsLayout.cs"/>
