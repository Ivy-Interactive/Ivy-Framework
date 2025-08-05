---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# TabsLayout

The TabsLayout widget creates a tabbed interface that allows users to switch between different content sections. It supports both traditional tabs and content-based variants, with features like closable tabs, badges, icons, and drag-and-drop reordering.

## Basic Usage

Use the TabView helper te create simple tabs:

```csharp demo-tabs
Layout.Tabs(
    new Tab("Profile", "User profile information"),
    new Tab("Security", "Security settings"),
    new Tab("Preferences", "User preferences")
)
```

This creates a basic tabs layout with three tabs.

### TabView with Customization

This example shows how to combine multiple TabView features including icons, badges, variant selection, and size control:

```csharp demo-tabs
Layout.Tabs(
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
)
.Variant(TabsVariant.Tabs)
.Width(200)
.Height(200)
.Padding(8)
.RemoveParentPadding()
```

This demonstrates the fluent API of TabView, allowing you to chain multiple configuration methods for a complete tab setup with visual indicators and precise layout control.

## TabsLayout usage

The first parameter is the selected tab index (0), and the remaining parameters are the Tab objects.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "This is the overview content"),
    new Tab("Details", "This is the details content"),
    new Tab("Settings", "This is the settings content")
)
```

## With Event Handlers

- `onSelect`: Handles tab selection events
- `onClose`: Adds close functionality to tabs
- `onRefresh`: Adds refresh buttons to tabs
- `onReorder`: Enables drag-and-drop tab reordering
- `selectedIndex`: Sets the initially selected tab

This example shows how to handle all available events. The event handlers receive the tab index and can perform custom actions like logging, state updates, or API calls.

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

## Tabs Variant

The Tabs variant displays tabs as clickable buttons with an underline indicator for the active tab, providing a more traditional tab interface.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("First", "First tab content"),
    new Tab("Second", "Second tab content"),
    new Tab("Third", "Third tab content")
).Variant(TabsVariant.Tabs)
```

## Content Variant

The Content variant emphasizes the content area with subtle tab indicators, ideal for content-heavy applications where the focus should be on the displayed information.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "Overview content here"),
    new Tab("Details", "Detailed information here"),
    new Tab("Settings", "Configuration options here")
).Variant(TabsVariant.Content)
```

## With Icons and Badges

Enhance tabs with icons and badges for better visual representation:

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
).Variant(TabsVariant.Tabs)
```

### Tab Keys for Refresh

Use the Key extension method to force tab re-rendering when content changes:

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

### Advanced Padding Control

Customize padding with various overloads:

```csharp demo-tabs
Layout.Vertical()
    | Text.Block("Default Padding (4px)")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content 1"),
        new Tab("Tab 2", "Content 2")
    ).Variant(TabsVariant.Tabs)
    | Text.Block("Uniform Padding (16px)")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content with uniform padding"),
        new Tab("Tab 2", "Content with uniform padding")
    ).Padding(16)
    | Text.Block("Horizontal/Vertical Padding (20px/8px)")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content with different horizontal/vertical padding"),
        new Tab("Tab 2", "Content with different horizontal/vertical padding")
    ).Padding(20, 8)
    | Text.Block("Custom Padding (Left: 12, Top: 8, Right: 12, Bottom: 16)")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content with custom padding on all sides"),
        new Tab("Tab 2", "Content with custom padding on all sides")
    ).Padding(12, 8, 12, 16)
```

Different padding overloads allow fine-grained control over spacing. Uniform padding is simplest, while custom padding gives precise control over each side.

## Size and Dimensions

Control the TabsLayout dimensions:

```csharp demo-tabs
Layout.Vertical()
    | Text.Block("Fixed Width (300px)")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content 1"),
        new Tab("Tab 2", "Content 2")
    ).Variant(TabsVariant.Tabs).Width(300)
    | Text.Block("Fixed Height (200px)")
    | new TabsLayout(null, null, null, null, 0,
        new Tab("Tab 1", "Content 1"),
        new Tab("Tab 2", "Content 2")
    ).Variant(TabsVariant.Tabs).Height(200)
    | Text.Block("Using TabView with Size Control")
    | Layout.Tabs(
        new Tab("Tab 1", "Content 1"),
        new Tab("Tab 2", "Content 2")
    ).Variant(TabsVariant.Tabs).Size(250).Grow()
```

Width and Height methods control the layout dimensions. TabView provides additional size control methods like Size() and Grow() for more flexible sizing options.

## Default Values

TabsLayout has the following default values:

- `Variant`: `TabsVariant.Content`
- `RemoveParentPadding`: `true`
- `Padding`: `new Thickness(4)` (16px)
- `Width`: `Size.Full()`
- `Height`: `Size.Full()`

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

## Tab Extension Methods

Enhance Tab objects with these extension methods:

- `Icon(Icons?)`: Add an icon to the tab
- `Badge(string)`: Add a badge to the tab
- `Key(object)`: Set a unique key for forcing re-renders

## TabsLayout Extension Methods

Customize the TabsLayout behavior with these extension methods:

- `Variant(TabsVariant)`: Set tabs or content variant
- `Padding(int)`: Set uniform padding around the layout
- `Padding(int, int)`: Set vertical and horizontal padding
- `Padding(int, int, int, int)`: Set left, top, right, bottom padding
- `Padding(Thickness?)`: Set padding using Thickness object
- `RemoveParentPadding()`: Remove parent container padding

## TabView Extension Methods

The TabView helper provides additional methods for size control:

- `Width(Size)`: Set the width of the layout
- `Height(Size)`: Set the height of the layout
- `Size(Size)`: Set both width and height
- `Size(int)`: Set both width and height using units
- `Size(float)`: Set both width and height using fraction
- `Grow()`: Make the layout grow to fill available space
