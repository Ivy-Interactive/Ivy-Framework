---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# TabsLayout

The TabsLayout widget creates a tabbed interface that allows users to switch between different content sections. It supports both traditional tabs and content-based variants, with features like closable tabs, badges, icons, and drag-and-drop reordering.

## Basic Usage

### Simple Static Tabs

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer management interface"),
    new Tab("Orders", "Order processing system"),
    new Tab("Settings", "Application configuration")
)
```

### With Event Handlers

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

### Closable Tabs

```csharp demo-tabs
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: (e) => Console.WriteLine($"Closed: {e.Value}"),
    onRefresh: null,
    onReorder: null,
    selectedIndex: 0,
    new Tab("Tab 1", "First tab content"),
    new Tab("Tab 2", "Second tab content"),
    new Tab("Tab 3", "Third tab content")
).Variant(TabsVariant.Tabs)
```

## Tab Variants

### Tabs Variant (Default)

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Profile", "User profile information"),
    new Tab("Security", "Security settings"),
    new Tab("Preferences", "User preferences")
).Variant(TabsVariant.Tabs)
```

### Content Variant

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "Dashboard overview"),
    new Tab("Analytics", "Data analytics"),
    new Tab("Reports", "Generated reports")
).Variant(TabsVariant.Content)
```

## Tab Features

### Icons and Badges

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
).Variant(TabsVariant.Tabs)
```

### Tab Refresh

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

### Tab Reordering

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

- `onSelect`: Event handler for tab selection
- `onClose`: Event handler for tab closing (optional)
- `onRefresh`: Event handler for tab refresh (optional)
- `onReorder`: Event handler for tab reordering (optional)
- `selectedIndex`: The initially selected tab index (0-based)
- `tabs`: Variable number of Tab objects

## Tab Properties

- `Title`: The display text for the tab
- `Icon`: Optional icon to display alongside the tab title
- `Badge`: Optional badge to display on the tab
- `Key`: Optional key for forcing re-renders (used in refresh scenarios)

## Extension Methods

- `Variant(TabsVariant)`: Set tabs or content variant
- `Padding(int)`: Set padding around the layout
- `RemoveParentPadding()`: Remove parent container padding
