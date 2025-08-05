---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# TabsLayout

The TabsLayout widget creates a tabbed interface that allows users to switch between different content sections. It supports both traditional tabs and content-based variants, with features like closable tabs, badges, icons, and drag-and-drop reordering.

## Basic Usage

### Simple Static Tabs

Create a basic tabs layout with static content:

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer management interface"),
    new Tab("Orders", "Order processing system"),
    new Tab("Settings", "Application configuration")
)
```

### With Event Handlers

This example demonstrates how to use all the available parameters. Each parameter serves a specific purpose:

- `onSelect`: Handles tab selection events
- `onClose`: Adds close functionality to tabs
- `onRefresh`: Adds refresh buttons to tabs
- `onReorder`: Enables drag-and-drop tab reordering
- `selectedIndex`: Sets the initially selected tab

```csharp demo-tabs
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: (e) => Console.WriteLine($"Closed: {e.Value}"),
    onRefresh: (e) => Console.WriteLine($"Refreshed: {e.Value}"),
    onReorder: null,
    selectedIndex: 0,
    new Tab("Tab 1", "First tab content"),
    new Tab("Tab 2", "Second tab content"),
    new Tab("Tab 3", "Third tab content")
).Variant(TabsVariant.Tabs)
```

## Tab Variants

### Default Tabs Variant

The default variant provides a traditional browser-like tab interface:

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Profile", "User profile information"),
    new Tab("Security", "Security settings"),
    new Tab("Preferences", "User preferences")
).Variant(TabsVariant.Tabs)
```

### Content Tabs Variant

The content variant offers smooth animations and modern styling:

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "Dashboard overview"),
    new Tab("Analytics", "Data analytics"),
    new Tab("Reports", "Generated reports")
).Variant(TabsVariant.Content)
```

## Tab Features

### Icons and Badges

Enhance tabs with icons and badges for better visual representation:

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
).Variant(TabsVariant.Tabs)
```

### Padding Control

Customize padding around the tabs layout:

```csharp demo-tabs
Layout.Vertical()
    | Text.Block("Default Padding")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content 1"),
        new Tab("Tab 2", "Content 2")
    ).Variant(TabsVariant.Tabs)
    | Text.Block("Custom Padding (16px)")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content 1"),
        new Tab("Tab 2", "Content 2")
    ).Variant(TabsVariant.Tabs).Padding(16)
    | Text.Block("No Parent Padding. Using default affects the tab content area only. RemoveParentPadding removes external spacing from the parent container.")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content 1"),
        new Tab("Tab 2", "Content 2")
    ).Variant(TabsVariant.Tabs).RemoveParentPadding()
```

### Tab Refresh

Enable refresh functionality for individual tabs:

```csharp demo-tabs
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: null,
    onRefresh: (e) => Console.WriteLine($"Refreshed: {e.Value}"),
    onReorder: null,
    selectedIndex: 0,
    new Tab("Data", "Current data"),
    new Tab("Logs", "System logs"),
    new Tab("Status", "System status")
).Variant(TabsVariant.Tabs)
```

Use keys to force tab re-rendering when content changes:

```csharp demo-tabs
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: null,
    onRefresh: (e) => Console.WriteLine($"Refreshed: {e.Value}"),
    onReorder: null,
    selectedIndex: 0,
    new Tab("Data", "Current data").Key("data-tab-1"),
    new Tab("Logs", "System logs").Key("logs-tab-1"),
    new Tab("Status", "System status").Key("status-tab-1")
).Variant(TabsVariant.Tabs)
```

### Tab Reordering

Enable drag-and-drop reordering of tabs:

```csharp demo-tabs
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: null,
    onRefresh: null,
    onReorder: (e) => Console.WriteLine($"Reordered: {string.Join(",", e.Value)}"),
    selectedIndex: 0,
    new Tab("First", "First tab content"),
    new Tab("Second", "Second tab content"),
    new Tab("Third", "Third tab content")
).Variant(TabsVariant.Tabs)
```

## Constructor Parameters

The TabsLayout constructor accepts the following parameters:

- `onSelect`: Event handler for tab selection
- `onClose`: Event handler for tab closing (optional)
- `onRefresh`: Event handler for tab refresh (optional)
- `onReorder`: Event handler for tab reordering (optional)
- `selectedIndex`: The initially selected tab index (0-based)
- `tabs`: Variable number of Tab objects

## Tab Properties

Each Tab object supports the following properties:

- `Title`: The display text for the tab
- `Icon`: Optional icon to display alongside the tab title
- `Badge`: Optional badge to display on the tab
- `Key`: Optional key for forcing re-renders (used in refresh scenarios)

## Extension Methods

Customize the TabsLayout behavior with these extension methods:

- `Variant(TabsVariant)`: Set tabs or content variant
- `Padding(int)`: Set padding around the layout
- `RemoveParentPadding()`: Remove parent container padding
