---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - toolbar
  - actions
  - buttons
  - controls
  - floating
  - editor
---

# Toolbar

<Ingress>
A container for organizing action buttons and controls with grouping and separators.
</Ingress>

The `Toolbar` widget provides a flexible way to create toolbars for applications, editors, and floating action bars. It uses `MenuItem` objects for toolbar items, leveraging variants for separators and groups.

## Basic Usage

Create a toolbar by adding `MenuItem` objects. Use `MenuItem.Separator()` for visual dividers:

```csharp demo-below
new Toolbar()
    | new MenuItem(Label: "Save", Icon: Icons.Save, Tag: "save")
        .OnSelect(_ => client.Toast("Saved!"))
    | new MenuItem(Label: "Undo", Icon: Icons.Undo, Tag: "undo")
        .OnSelect(_ => client.Toast("Undo"))
    | MenuItem.Separator()
    | MenuItem.Default(Icons.ZoomIn, tag: "zoom-in")
        .Tooltip("Zoom In")
        .OnSelect(_ => client.Toast("Zoom In"))
```

## Grouping Items

Use `MenuItem` with `MenuItemVariant.Group` to create logical groupings:

```csharp demo-below
new Toolbar(
    new MenuItem(
        Variant: MenuItemVariant.Group,
        Children: [
            MenuItem.Default(Icons.Bold, tag: "bold").Tooltip("Bold"),
            MenuItem.Default(Icons.Italic, tag: "italic").Tooltip("Italic"),
            MenuItem.Default(Icons.Underline, tag: "underline").Tooltip("Underline")
        ]
    ),
    MenuItem.Separator(),
    new MenuItem(
        Variant: MenuItemVariant.Group,
        Children: [
            MenuItem.Default(Icons.TextAlignStart, tag: "left").Tooltip("Left"),
            MenuItem.Default(Icons.TextAlignCenter, tag: "center").Tooltip("Center")
        ]
    )
)
```

## Active State

Use the `Checked` property to indicate selected or active toolbar buttons:

```csharp demo-tabs
new Toolbar()
    | MenuItem.Default(Icons.Bold, tag: "bold")
        .Tooltip("Bold")
        .Checked(true)
    | MenuItem.Default(Icons.Italic, tag: "italic")
        .Tooltip("Italic")
        .Checked(false)
```

## Icon-Only Buttons

Toolbar buttons without labels are rendered as icon-only buttons. Always provide tooltips for accessibility:

```csharp demo-tabs
new Toolbar()
    | MenuItem.Default(Icons.Copy, tag: "copy")
        .Tooltip("Copy")
        .OnSelect(_ => client.Toast("Copied"))
    | MenuItem.Default(Icons.Scissors, tag: "cut")
        .Tooltip("Cut")
        .OnSelect(_ => client.Toast("Cut"))
    | MenuItem.Default(Icons.ClipboardPaste, tag: "paste")
        .Tooltip("Paste")
        .OnSelect(_ => client.Toast("Pasted"))
```

## Disabled State

Disable the entire toolbar or individual items:

```csharp demo-tabs
new Toolbar()
    | MenuItem.Default("Enabled", tag: "enabled").Icon(Icons.Check)
    | MenuItem.Default("Disabled", tag: "disabled").Icon(Icons.Ban).Disabled()
```

```csharp demo-tabs
(new Toolbar()
    | MenuItem.Default("Action 1", tag: "action1").Icon(Icons.Wand)
    | MenuItem.Default("Action 2", tag: "action2").Icon(Icons.Sparkles)
).Disabled()
```

<WidgetDocs Type="Ivy.Toolbar" ExtensionTypes="Ivy.ToolbarExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Toolbar.cs"/>
