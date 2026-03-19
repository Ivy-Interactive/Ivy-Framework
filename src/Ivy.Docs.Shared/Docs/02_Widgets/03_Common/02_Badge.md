---
prepare: |
  var client = UseService<IClientProvider>();
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
Display small pieces of information like counts, statuses, or labels in compact, styled [badges](../../01_Onboarding/02_Concepts/03_Widgets.md) with various colors and variants.
</Ingress>

The `Badge` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a versatile component used to display small pieces of information, such as counts or statuses, in a compact form. It is commonly used within [Views](../../01_Onboarding/02_Concepts/02_Views.md).

## Basic Usage

Here's a simple example of a badge:

```csharp demo-below
new Badge("Primary")
```

## Variants

Badges come in several variants to suit different use cases and [visual hierarchies](../../01_Onboarding/02_Concepts/12_Theming.md).

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

`Badge`s can include icons to enhance their visual appearance and meaning. See [Icon](../01_Primitives/02_Icon.md) for more details. Use [Align](../../04_ApiReference/Ivy/Align.md) for icon position (e.g. `Align.Right`).

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.P("Icons on the Left").Large()
    | (Layout.Horizontal().Gap(4)
        | new Badge("Notification", icon:Icons.Bell)
        | new Badge("Success", icon:Icons.Check).Secondary()
        | new Badge("Error", icon:Icons.X).Destructive())
    | Text.P("Icons on the Right").Large()
    | (Layout.Horizontal().Gap(4)
        | new Badge("Download").Icon(Icons.Download, Align.Right)
        | new Badge("Next").Icon(Icons.ChevronRight, Align.Right).Secondary())
    | Text.P("Icon-Only").Large()
    | (Layout.Horizontal().Gap(4)
        | new Badge(null, icon:Icons.Bell)
        | new Badge(null, icon:Icons.X, variant:BadgeVariant.Destructive))
```

## Click Listener

Badges can be made clickable using the `OnClick` extension method. This is useful for filter chips, tag management, and toggle states.

```csharp demo-below
new Badge("Click Me", icon:Icons.MousePointer)
    .OnClick(_ => client.Toast("Badge clicked!"))
```

<WidgetDocs Type="Ivy.Badge" ExtensionTypes="Ivy.BadgeExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Badge.cs"/>

## Faq

<Details>
<Summary>
Badge usage examples
</Summary>
<Body>

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.P("Status").Large()
    | (Layout.Horizontal().Gap(4)
        | new Badge("Online", icon:Icons.Circle, variant:BadgeVariant.Secondary)
        | new Badge("Offline", icon:Icons.Circle, variant:BadgeVariant.Destructive))
    | Text.P("Counters").Large()
    | (Layout.Horizontal().Gap(4)
        | new Badge("4").Large()
        | new Badge("12", icon:Icons.Mail).Large())
    | Text.P("Tags").Large()
    | (Layout.Horizontal().Gap(4)
        | new Badge("Design", icon:Icons.Palette)
        | new Badge("Development", icon:Icons.Code))
```

</Body>
</Details>

<Details>
<Summary>
What are the available BadgeVariant values in Ivy?
</Summary>
<Body>

The `BadgeVariant` enum has these values: `Primary`, `Destructive`, `Outline`, `Secondary`, `Success`, `Warning`, `Info`.

Usage:
```csharp
// Via constructor:
new Badge("Status", BadgeVariant.Success)

// Via fluent Variant() method:
new Badge("Status").Variant(BadgeVariant.Warning)

// Via shortcut extension methods:
new Badge("Status").Success()
new Badge("Status").Destructive()
new Badge("Status").Info()
```

There is no `BadgeVariant.Default`. Use `BadgeVariant.Primary` or omit the variant for the default appearance.

</Body>
</Details>
