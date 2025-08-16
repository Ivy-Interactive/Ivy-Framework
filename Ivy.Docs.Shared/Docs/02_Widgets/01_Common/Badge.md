---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# Badge

<Ingress>
Display small pieces of information like counts, statuses, or labels in compact, styled badges with various colors and variants.
</Ingress>

The `Badge` widget is a versatile component used to display small pieces of information, such as counts or statuses, in a compact form.

## Basic Usage

Here's a simple example of a badge:

```csharp demo-below
new Badge("Primary")
```

## Variants

Badges come in several variants to suit different use cases and visual hierarchies.

```csharp demo-tabs
Layout.Horizontal()
    | new Badge("Primary")
    | new Badge("Destructive", variant:BadgeVariant.Destructive)
    | new Badge("Outline", variant:BadgeVariant.Outline)
    | new Badge("Secondary", variant:BadgeVariant.Secondary)
```

### Using Extension Methods

You can also use extension methods for cleaner code:

```csharp demo-tabs
Layout.Horizontal()
    | new Badge("Primary")
    | new Badge("Destructive").Destructive()
    | new Badge("Outline").Outline()
    | new Badge("Secondary").Secondary()
```

## Styling Options

### Badge Sizes

Badges support different sizes to match your design requirements.

```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Badge("Small").Small()
    | new Badge("Medium")
    | new Badge("Large").Large()
    | new Badge("Small").Secondary().Small()
    | new Badge("Medium").Secondary()
    | new Badge("Large").Secondary().Large()
    | new Badge("Small").Destructive().Small()
    | new Badge("Medium").Destructive()
    | new Badge("Large").Destructive().Large()
    | new Badge("Small").Outline().Small()
    | new Badge("Medium").Outline()
    | new Badge("Large").Outline().Large()
```

### Icons

`Badge`s can include icons to enhance their visual appearance and meaning.

### Icons on the Left (Default)

```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Badge("Notification", icon:Icons.Bell)
    | new Badge("Success", icon:Icons.Check)
    | new Badge("Warning", icon:Icons.CircleAlert)
    | new Badge("Error", icon:Icons.X)
    | new Badge("Notification", icon:Icons.Bell).Secondary()
    | new Badge("Success", icon:Icons.Check).Secondary()
    | new Badge("Warning", icon:Icons.CircleAlert).Secondary()
    | new Badge("Error", icon:Icons.X).Secondary()
    | new Badge("Notification", icon:Icons.Bell).Destructive()
    | new Badge("Success", icon:Icons.Check).Destructive()
    | new Badge("Warning", icon:Icons.CircleAlert).Destructive()
    | new Badge("Error", icon:Icons.X).Destructive()
    | new Badge("Notification", icon:Icons.Bell).Outline()
    | new Badge("Success", icon:Icons.Check).Outline()
    | new Badge("Warning", icon:Icons.CircleAlert).Outline()
    | new Badge("Error", icon:Icons.X).Outline()
```

### Icons on the Right

```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Badge("Download").Icon(Icons.Download, Align.Right)
    | new Badge("Upload").Icon(Icons.Upload, Align.Right)
    | new Badge("Settings").Icon(Icons.Settings, Align.Right)
    | new Badge("Info").Icon(Icons.Info, Align.Right)
    | new Badge("Download").Secondary().Icon(Icons.Download, Align.Right)
    | new Badge("Upload").Secondary().Icon(Icons.Upload, Align.Right)
    | new Badge("Settings").Secondary().Icon(Icons.Settings, Align.Right)
    | new Badge("Info").Secondary().Icon(Icons.Info, Align.Right)
    | new Badge("Download").Destructive().Icon(Icons.Download, Align.Right)
    | new Badge("Upload").Destructive().Icon(Icons.Upload, Align.Right)
    | new Badge("Settings").Destructive().Icon(Icons.Settings, Align.Right)
    | new Badge("Info").Destructive().Icon(Icons.Info, Align.Right)
    | new Badge("Download").Outline().Icon(Icons.Download, Align.Right)
    | new Badge("Upload").Outline().Icon(Icons.Upload, Align.Right)
    | new Badge("Settings").Outline().Icon(Icons.Settings, Align.Right)
    | new Badge("Info").Outline().Icon(Icons.Info, Align.Right)
```

### Icon-Only Badges

You can create icon-only badges by passing `null` as the title:

```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Badge(null, icon:Icons.Bell)
    | new Badge(null, icon:Icons.Check, variant:BadgeVariant.Secondary)
    | new Badge(null, icon:Icons.X, variant:BadgeVariant.Destructive)
    | new Badge(null, icon:Icons.CircleAlert, variant:BadgeVariant.Outline)
```

## Common Use Cases

### Status Indicators

```csharp demo-below
Layout.Horizontal()
    | new Badge("Online", icon:Icons.Circle, variant:BadgeVariant.Secondary)
    | new Badge("Offline", icon:Icons.Circle, variant:BadgeVariant.Destructive)
    | new Badge("Away", icon:Icons.Circle, variant:BadgeVariant.Outline)
```

### Notification Counters

```csharp demo-below
Layout.Horizontal()
    | new Badge("3", icon:Icons.Bell).Large()
    | new Badge("12", icon:Icons.Mail).Large()
    | new Badge("99+", icon:Icons.MessageSquare).Large()
```

### Categories and Tags

```csharp demo-below
Layout.Wrap()
    | new Badge("Technology", icon:Icons.Cpu)
    | new Badge("Design", icon:Icons.Palette)
    | new Badge("Marketing", icon:Icons.TrendingUp)
    | new Badge("Development", icon:Icons.Code)
    | new Badge("Analytics", icon:Icons.ChartBar)
    | new Badge("Support", icon:Icons.CircleHelp)
```

### Priority Levels

```csharp demo-below
Layout.Horizontal()
    | new Badge("High", icon:Icons.CircleAlert, variant:BadgeVariant.Destructive)
    | new Badge("Medium", icon:Icons.CircleAlert, variant:BadgeVariant.Secondary)
    | new Badge("Low", icon:Icons.Info, variant:BadgeVariant.Outline)
```

<WidgetDocs Type="Ivy.Badge" ExtensionTypes="Ivy.BadgeExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Badge.cs"/>
