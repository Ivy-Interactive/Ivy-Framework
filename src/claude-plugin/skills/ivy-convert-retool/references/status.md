# Status

A content area to display a status indicator. Shows labeled status values with colored dots or icons, useful for representing workflow states, service health, or record status.

## Retool

```toolscript
status1.value = "active"
status1.values = ["active", "inactive", "pending"]
status1.labels = ["Active", "Inactive", "Pending"]
status1.colorByIndex = ["green", "red", "yellow"]
status1.iconByIndex = ["/icon:bold/check", "/icon:bold/close", "/icon:bold/clock"]
```

## Ivy

Ivy does not have a dedicated Status widget. The closest equivalent is the `Badge` widget with variant-based coloring, or composing `Icon` + `Text` for a custom status indicator.

```csharp
// Using Badge with variants as status indicators
new Badge("Active", BadgeVariant.Success, Icons.CircleCheck);
new Badge("Inactive", BadgeVariant.Destructive, Icons.CircleX);
new Badge("Pending", BadgeVariant.Warning, Icons.Clock);

// Compose a status indicator with Icon + Text
Layout.Horizontal()
    | new Icon(Icons.Circle, Colors.Green).Small()
    | Text.P("Active");
```

## Parameters

| Parameter          | Documentation                                     | Ivy                                                    |
|--------------------|----------------------------------------------------|---------------------------------------------------------|
| `value`            | Current status value                               | `Badge.Title` or composed text                         |
| `values`           | Available values for selection                     | Not supported (define badges per state)                |
| `labels`           | Label list for each item                           | `Badge.Title` per instance                             |
| `colorByIndex`     | Color list for each item by index                  | `BadgeVariant` (Success, Destructive, Warning, etc.)   |
| `iconByIndex`      | Icon list for each item by index                   | `Badge.Icon` / `Icons` enum per instance               |
| `iconPosition`     | Icon placement (left/right/replace)                | `Badge.IconPosition(Align)` (Left/Right)               |
| `fallbackValue`    | Fallback value to use                              | Not supported                                          |
| `horizontalAlign`  | Alignment: left, center, right                     | Not supported (use layout)                             |
| `heightType`       | Height behavior: auto, fixed, fill                 | `Badge.Height`                                         |
| `hidden`           | Visibility toggle                                  | `Badge.Visible` property                               |
| `margin`           | External spacing                                   | Not supported (use layout spacing)                     |
| `isHiddenOnMobile` | Hide on mobile layout                              | Not supported                                          |
| `isHiddenOnDesktop`| Hide on desktop layout                             | Not supported                                          |
| `tooltipByIndex`   | Tooltip list per item                              | Not supported (wrap with `Tooltip` widget)             |
| `tooltipText`      | Hover helper text                                  | Not supported (wrap with `Tooltip` widget)             |
| `selectedIndex`    | Selected item index                                | Not supported                                          |
| `selectedItem`     | Selected item value                                | Not supported                                          |
| `selectedLabel`    | Selected item label                                | Not supported                                          |
| `setValue(value)`   | Set current value                                 | Not supported (set Badge properties directly)          |
| `resetValue()`     | Reset to default value                             | Not supported                                          |
| `setHidden(hidden)`| Toggle visibility                                  | `Visible` property                                     |
