---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# Button

The Button widget is one of the most fundamental interactive elements in Ivy. It allows users to trigger actions and navigate through your application.

## Basic Usage

Here's a simple example of a button that shows a toast message when clicked:

```csharp
var client = this.UseService<IClientProvider>();
new Button("Click Me", onClick: _ => client.Toast("Hello!"));
```

```csharp demo
new Button("Click Me", onClick: _ => client.Toast("Hello!"))
```

## Variants

Buttons come in several variants to suit different use cases.

```csharp demo-tabs
Layout.Horizontal()
    | new Button("Default")
    | new Button("Destructive").Destructive()
    | new Button("Secondary").Secondary()
    | new Button("Outline").Outline()
    | new Button("Ghost").Ghost()
    | new Button("Link").Link()
```

## Styling Options

### Button States

#### Disabled State
```csharp demo-tabs
Layout.Grid().Columns(3)
    | new Button("Primary").Disabled()
    | new Button("Secondary").Secondary().Disabled()
    | new Button("Destructive").Destructive().Disabled()
```

#### Loading State
```csharp demo-tabs
Layout.Grid().Columns(3)
    | new Button("Primary").Loading()
    | new Button("Secondary").Secondary().Loading()
    | new Button("Destructive").Destructive().Loading()
```

### Button Sizes

```csharp demo-tabs
Layout.Grid().Columns(3)
    | new Button("Small").Small()
    | new Button("Default")
    | new Button("Large").Large()
    | new Button("Small").Secondary().Small()
    | new Button("Default").Secondary()
    | new Button("Large").Secondary().Large()
    | new Button("Small").Destructive().Small()
    | new Button("Default").Destructive()
    | new Button("Large").Destructive().Large()
```

### Border Radius

```csharp demo-tabs
Layout.Grid().Columns(3)
    | new Button("None").BorderRadius(BorderRadius.None)
    | new Button("Rounded").BorderRadius(BorderRadius.Rounded)
    | new Button("Full").BorderRadius(BorderRadius.Full)
    | new Button("None").Secondary().BorderRadius(BorderRadius.None)
    | new Button("Rounded").Secondary().BorderRadius(BorderRadius.Rounded)
    | new Button("Full").Secondary().BorderRadius(BorderRadius.Full)
    | new Button("None").Destructive().BorderRadius(BorderRadius.None)
    | new Button("Rounded").Destructive().BorderRadius(BorderRadius.Rounded)
    | new Button("Full").Destructive().BorderRadius(BorderRadius.Full)
```

### Custom Colors

```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Button("Blue").Foreground(Colors.Blue)
    | new Button("Green").Foreground(Colors.Green)
    | new Button("Purple").Foreground(Colors.Purple)
    | new Button("Orange").Foreground(Colors.Orange)
    | new Button("Blue").Secondary().Foreground(Colors.Blue)
    | new Button("Green").Secondary().Foreground(Colors.Green)
    | new Button("Purple").Secondary().Foreground(Colors.Purple)
    | new Button("Orange").Secondary().Foreground(Colors.Orange)
    | new Button("Blue").Destructive().Foreground(Colors.Blue)
    | new Button("Green").Destructive().Foreground(Colors.Green)
    | new Button("Purple").Destructive().Foreground(Colors.Purple)
    | new Button("Orange").Destructive().Foreground(Colors.Orange)
```

### Icons

#### Icons on the Left
```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Button("Save").Icon(Icons.Save)
    | new Button("Download").Icon(Icons.Download)
    | new Button("Upload").Icon(Icons.Upload)
    | new Button("Settings").Icon(Icons.Settings)
    | new Button("Save").Secondary().Icon(Icons.Save)
    | new Button("Download").Secondary().Icon(Icons.Download)
    | new Button("Upload").Secondary().Icon(Icons.Upload)
    | new Button("Settings").Secondary().Icon(Icons.Settings)
    | new Button("Delete").Destructive().Icon(Icons.Trash)
    | new Button("Remove").Destructive().Icon(Icons.X)
    | new Button("Cancel").Destructive().Icon(Icons.Ban)
    | new Button("Close").Destructive().Icon(Icons.CircleX)
```

#### Icons on the Right
```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Button("Next").Icon(Icons.ArrowRight, Align.Right)
    | new Button("Continue").Icon(Icons.ChevronRight, Align.Right)
    | new Button("Submit").Icon(Icons.Send, Align.Right)
    | new Button("Export").Icon(Icons.ExternalLink, Align.Right)
    | new Button("Next").Secondary().Icon(Icons.ArrowRight, Align.Right)
    | new Button("Continue").Secondary().Icon(Icons.ChevronRight, Align.Right)
    | new Button("Submit").Secondary().Icon(Icons.Send, Align.Right)
    | new Button("Export").Secondary().Icon(Icons.ExternalLink, Align.Right)
    | new Button("Next").Destructive().Icon(Icons.ArrowRight, Align.Right)
    | new Button("Continue").Destructive().Icon(Icons.ChevronRight, Align.Right)
    | new Button("Submit").Destructive().Icon(Icons.Send, Align.Right)
    | new Button("Export").Destructive().Icon(Icons.ExternalLink, Align.Right)
```

### Icon-Only Buttons

```csharp demo-tabs
Layout.Grid().Columns(4)
    | new Button(null, icon: Icons.Plus)
    | new Button(null, icon: Icons.Pen, variant: ButtonVariant.Outline)
    | new Button(null, icon: Icons.Trash, variant: ButtonVariant.Destructive)
    | new Button(null, icon: Icons.Settings, variant: ButtonVariant.Ghost)
```

### Combined Styling

```csharp demo-below
new Button("Advanced Button")
    .Icon(Icons.ArrowRight, Align.Right)
    .BorderRadius(BorderRadius.Full)
    .Large()
    .Foreground(Colors.Blue)
```

<WidgetDocs Type="Ivy.Button" ExtensionTypes="Ivy.ButtonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Button.cs"/>
