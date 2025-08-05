---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# TabsLayout

The TabsLayout widget creates a tabbed interface that allows users to switch between different content sections. It supports both traditional tabs and content-based variants, with features like closable tabs, badges, icons, and drag-and-drop reordering.

## Basic Usage

### Simple Static Tabs

There is a recomended way to create tabs layout. 
When create new tab, you need to give five parametres before creation of the new tab.

- `onSelect`: Event handler for tab selection
- `onClose`: Event handler for tab closing (optional)
- `onRefresh`: Event handler for tab refresh (optional)
- `onReorder`: Event handler for tab reordering (optional)
- `selectedIndex`: The initially selected tab index (0-based)
- `tabs`: Variable number of Tab objects

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer management interface"),
    new Tab("Orders", "Order processing system"),
    new Tab("Settings", "Application configuration")
)
```

### With Event Handlers

Here is a demo with parametres are being used. 
Every parameter has an implementation what it should do.
`onSelect` gives an opportunity to work with selected tab.
`onClose` is for adding a close symbol on the tab.
`onRefresh` adds a refresh button near the tab name and can give posibilities to work with button.
`onReorder` enables reorder tabs in the specific way with grabing them between themselves. 
`selectedIndex` determines start selected tab.  

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

In the base usage, tabs variant is `TabsVariant.Tabs`, which organise tabs as default. 
This way of tab using is more familiar for tabs in browser. 

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Profile", "User profile information"),
    new Tab("Security", "Security settings"),
    new Tab("Preferences", "User preferences")
).Variant(TabsVariant.Tabs)
```

### Content Tabs Variant

With `TabsVariant.Content` you can get animations when click on other tab. 
This way of tabs creation is more like using some apps. 

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Overview", "Dashboard overview"),
    new Tab("Analytics", "Data analytics"),
    new Tab("Reports", "Generated reports")
).Variant(TabsVariant.Content)
```

## Tab Features

### Icons and Badges

You can create custom tabs and add items near tabs names and badges.

```csharp demo-tabs
new TabsLayout(null, null, null, null, 0,
    new Tab("Customers", "Customer list").Icon(Icons.User).Badge("10"),
    new Tab("Orders", "Order management").Icon(Icons.DollarSign).Badge("0"),
    new Tab("Settings", "Configuration").Icon(Icons.Settings).Badge("999")
).Variant(TabsVariant.Tabs)
```

### Tab Refresh

You also can activate `onRefresh` state to enable refreshing of the tab. The important thing is that you decide what refresh button should do by your own. 
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
