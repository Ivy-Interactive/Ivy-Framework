# Tags

A content area to display a set of tags. Shows labeled items with optional colors, icons, and images, supporting wrapping and click interactions.

## Retool

```toolscript
tags1.labels = ["Frontend", "Backend", "DevOps"]
tags1.colorByIndex = ["blue", "green", "orange"]
tags1.iconByIndex = ["/icon:bold/code", "/icon:bold/server", "/icon:bold/cloud"]
tags1.allowWrap = true
tags1.itemMode = "dynamic"
```

## Ivy

Ivy does not have a dedicated Tags widget. The closest equivalent is composing multiple `Badge` widgets inside a `WrapLayout` or `Layout.Horizontal()`.

```csharp
// Using Badge widgets in a horizontal wrap layout
Layout.Horizontal().Wrap(true)
    | new Badge("Frontend", BadgeVariant.Info, Icons.Code)
    | new Badge("Backend", BadgeVariant.Success, Icons.Server)
    | new Badge("DevOps", BadgeVariant.Warning, Icons.Cloud);

// Dynamic tags from data
var tags = new[] { "C#", "React", "Azure", "SQL" };
Layout.Horizontal().Wrap(true)
    | tags.Select(tag => new Badge(tag, BadgeVariant.Secondary));
```

## Parameters

| Parameter          | Documentation                                     | Ivy                                                    |
|--------------------|----------------------------------------------------|---------------------------------------------------------|
| `labels`           | Display labels for items                           | `Badge.Title` on each instance                         |
| `colorByIndex`     | Color assigned to each item by position            | `BadgeVariant` per Badge                               |
| `textColorByIndex` | Text color values by index                         | Not supported                                          |
| `iconByIndex`      | Icon for each item by position                     | `Badge.Icon` / `Icons` enum                            |
| `imageByIndex`     | Image for each item                                | Not supported                                          |
| `allowWrap`        | Whether content wraps across multiple lines        | `Layout.Horizontal().Wrap(true)` or `WrapLayout`       |
| `itemMode`         | Config mode: dynamic or static                     | Not supported (always dynamic via code)                |
| `data`             | Underlying dataset                                 | Pass data collection, use LINQ `.Select()`             |
| `value`            | Current selected values                            | Not supported                                          |
| `horizontalAlign`  | Alignment: left, center, right                     | Not supported (use layout)                             |
| `tooltipByIndex`   | Tooltip text for each item                         | Not supported (wrap with `Tooltip` widget)             |
| `hiddenByIndex`    | Per-item visibility status                         | `Badge.Visible` per instance                           |
| `hidden`           | Component visibility                               | Layout container `Visible` property                    |
| `margin`           | External spacing                                   | Not supported (use layout spacing)                     |
| `isHiddenOnMobile` | Hide on mobile layout                              | Not supported                                          |
| `isHiddenOnDesktop`| Hide on desktop layout                             | Not supported                                          |
| `clickable`        | Enable click event (implicit)                      | Not supported on Badge                                 |
| Events: Click      | Triggered when an item is clicked                  | Not supported                                          |
| `clearValue()`     | Removes all current values                         | Not supported                                          |
| `resetValue()`     | Restores default values                            | Not supported                                          |
| `setValue(value)`   | Sets the current value                            | Not supported                                          |
