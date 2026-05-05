# Icon Text

A content area to display an icon with accompanying text. Retool's IconText is a presentation component that pairs an icon (from 3,000+ built-in icons) with a text label. In Ivy, the closest equivalent is the `Badge` widget, which combines an icon with text in a single component.

## Retool

```toolscript
// Basic Icon Text component configuration
iconText1.icon = "/icon:bold/shopping-gift"
iconText1.text = "Gift Shop"

// Dynamic icon based on condition
iconText1.icon = {{ table1.selectedRow.enabled
  ? "/icon:bold/interface-user-check"
  : "/icon:bold/interface-user-block" }}

// Style configuration via Inspector
iconText1.style = { fontSize: "16px" }
iconText1.hidden = false
iconText1.disabled = false
```

## Ivy

```csharp
// Badge with icon and text (closest equivalent)
new Badge("Gift Shop", icon: Icons.Gift)

// Badge with icon alignment
new Badge("Download").Icon(Icons.Download, Align.Right)

// Badge with variant styling
new Badge("Active", BadgeVariant.Success, Icons.CircleCheck)

// Icon-only badge
new Badge(null, icon: Icons.Bell)

// Alternative: compose Icon + Text separately in a layout
Layout.Horizontal()
    | new Icon(Icons.Gift, Colors.Green)
    | Text.P("Gift Shop")
```

## Parameters

| Parameter            | Documentation                                                                 | Ivy                                                        |
|----------------------|-------------------------------------------------------------------------------|------------------------------------------------------------|
| icon                 | The icon to display. Uses IconKey format (e.g. `/icon:bold/shopping-gift`)    | `Icons` enum (e.g. `Icons.Gift`). Uses Lucide icon library |
| text                 | The text displayed alongside the icon                                         | `title` parameter on `Badge` constructor                   |
| hidden               | Whether the component is hidden from view                                     | `Visible` property on widgets                              |
| disabled             | Whether interaction is disabled                                               | Not supported                                              |
| loading              | Whether to display a loading indicator                                        | Not supported (use `Icon.WithAnimation` for loading state) |
| horizontalAlign      | Horizontal alignment of contents (`left`, `center`, `right`)                  | `Align` parameter on `Badge.Icon()` (`Left`, `Right`)     |
| style                | Custom style options (font size, color, background)                           | `BadgeVariant` enum + `.Color()` modifier                  |
| style.fontSize       | Custom font size (supports dynamic `fx` values)                               | `.Small()`, `.Medium()`, `.Large()` on `Icon`              |
| margin               | Margin around the component (`4px 8px` or `0`)                                | Not supported                                              |
| tooltip              | Tooltip text shown on hover                                                   | Wrap with `Tooltip` widget                                 |
| maintainSpaceWhenHidden | Whether to reserve space when hidden                                       | Not supported                                              |
| isHiddenOnDesktop    | Whether to hide on desktop layout                                             | Not supported                                              |
| isHiddenOnMobile     | Whether to hide on mobile layout                                              | Not supported                                              |
| styleVariant         | Visual style variant (`solid`, `outline`)                                     | `BadgeVariant` (`Primary`, `Outline`, `Secondary`, etc.)   |
| events (Click)       | Event handler triggered on click                                              | Not supported on Badge                                     |

> **Note:** Retool's official IconText documentation is currently incomplete ("No definition found for object: IconText"). Properties above are inferred from the related Icon component and community discussions. Ivy's `Badge` is the closest single-widget match; for more control, compose `Icon` + `Text` in a `Layout.Horizontal()`.
