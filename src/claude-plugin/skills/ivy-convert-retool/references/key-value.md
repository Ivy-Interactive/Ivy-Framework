# Key-Value

A content area for viewing and editing key-value data. Displays paired data entries in an organized label-value format with support for multiple layout options and built-in editing capabilities.

## Retool

```toolscript
keyValue1.data = {
  Name: "John Doe",
  Email: "john@example.com",
  Role: "Admin",
  Status: true
}
keyValue1.groupLayout = "auto"
keyValue1.itemLabelPosition = "left"
```

## Ivy

```csharp
new { Name = "John Doe", Email = "john@example.com", Role = "Admin", Status = true }
    .ToDetails()
    .RemoveEmpty()
    .Builder(x => x.Email, b => b.CopyToClipboard())
```

## Parameters

| Parameter           | Documentation                                                              | Ivy                                          |
|---------------------|----------------------------------------------------------------------------|----------------------------------------------|
| `data`              | The key-value data to display (string/number/boolean/object/array)         | Passed via `ToDetails()` on any object       |
| `changeset`         | Array of edited values (read-only)                                         | Not supported                                |
| `disableSave`       | Controls whether edits can be saved                                        | Not supported                                |
| `editIcon`          | Icon indicating editable fields                                            | Not supported                                |
| `groupLayout`       | Layout mode: `multiColumn`, `singleColumn`, `wrap`, or `auto`             | Not supported                                |
| `heightType`        | Height behavior: `auto`, `fixed`, or `fill`                               | `Height` (Size)                              |
| `itemLabelPosition` | Label placement: `top` or `left`                                          | Not supported                                |
| `minColumnWidth`    | Minimum column width for multi-column layouts                              | Not supported                                |
| `hidden`            | Visibility toggle                                                          | `Visible` (bool)                             |
| `scrollIntoView()`  | Scrolls the component into view                                            | Not supported                                |
| `setHidden()`       | Toggles component visibility                                               | `Visible`                                    |
| —                   | —                                                                          | `RemoveEmpty()` – filters null/empty values  |
| —                   | —                                                                          | `Remove()` – hides specific fields           |
| —                   | —                                                                          | `Multiline()` – multi-line text wrapping     |
| —                   | —                                                                          | `CopyToClipboard()` – makes values copyable  |
| —                   | —                                                                          | `Link()` – converts values to clickable links|
| —                   | —                                                                          | `Scale` – nullable scale property            |
| —                   | —                                                                          | `Width` (Size)                               |
