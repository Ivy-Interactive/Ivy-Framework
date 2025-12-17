---
prepare: |
  var client = this.UseService<IClientProvider>();
searchHints:
  - tag
  - label
  - chip
  - status
  - indicator
  - pill
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
    | new Badge("Success", variant:BadgeVariant.Success)
    | new Badge("Warning", variant:BadgeVariant.Warning)
    | new Badge("Info", variant:BadgeVariant.Info)
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



### Icons

`Badge`s can include icons to enhance their visual appearance and meaning. See [Icon](../03_Primitives/Icon.md) for more details.

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Large("Icons on the Left")
    | (Layout.Horizontal().Gap(4)
        | new Badge("Notification", icon:Icons.Bell)
        | new Badge("Success", icon:Icons.Check).Secondary()
        | new Badge("Error", icon:Icons.X).Destructive())
    | Text.Large("Icons on the Right")
    | (Layout.Horizontal().Gap(4)
        | new Badge("Download").Icon(Icons.Download, Align.Right)
        | new Badge("Next").Icon(Icons.ChevronRight, Align.Right).Secondary())
    | Text.Large("Icon-Only")
    | (Layout.Horizontal().Gap(4)
        | new Badge(null, icon:Icons.Bell)
        | new Badge(null, icon:Icons.X, variant:BadgeVariant.Destructive))
```



<WidgetDocs Type="Ivy.Badge" ExtensionTypes="Ivy.BadgeExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Badge.cs"/>

## Examples

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Large("Status")
    | (Layout.Horizontal().Gap(4)
        | new Badge("Online", icon:Icons.Circle, variant:BadgeVariant.Secondary)
        | new Badge("Offline", icon:Icons.Circle, variant:BadgeVariant.Destructive))
    | Text.Large("Counters")
    | (Layout.Horizontal().Gap(4)
        | new Badge("4").Large()
        | new Badge("12", icon:Icons.Mail).Large())
    | Text.Large("Tags")
    | (Layout.Horizontal().Gap(4)
        | new Badge("Design", icon:Icons.Palette)
        | new Badge("Development", icon:Icons.Code))
```
