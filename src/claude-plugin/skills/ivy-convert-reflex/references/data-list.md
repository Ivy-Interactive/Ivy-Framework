# DataList

Displays label-value pairs, typically used for showing metadata or key-value information such as user profiles, order details, or configuration summaries.

## Reflex

```python
rx.data_list.root(
    rx.data_list.item(
        rx.data_list.label("Status"),
        rx.data_list.value("Authorized"),
    ),
    rx.data_list.item(
        rx.data_list.label("ID"),
        rx.data_list.value("U-474747"),
    ),
    rx.data_list.item(
        rx.data_list.label("Name"),
        rx.data_list.value("Developer Success"),
    ),
    rx.data_list.item(
        rx.data_list.label("Email"),
        rx.data_list.value("success@reflex.dev"),
    ),
)
```

## Ivy

```csharp
new { Status = "Authorized", Id = "U-474747", Name = "Developer Success", Email = "success@reflex.dev" }
    .ToDetails()
    .Builder(x => x.Id, b => b.CopyToClipboard())
    .Builder(x => x.Email, b => b.Link())
```

## Parameters

| Parameter       | Documentation                                                                 | Ivy                                                                                      |
|-----------------|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `orientation`   | `"horizontal" \| "vertical"` — layout direction of the data list             | Not supported (always vertical)                                                          |
| `size`          | `"1" \| "2" \| ...` — controls the text size of the data list                | `Scale` property on Details                                                              |
| `trim`          | `"normal" \| "start" \| ...` — trims whitespace around the data list         | Not supported                                                                            |
| `align`         | `"start" \| "center" \| ...` — alignment of items within the list            | Not supported                                                                            |
| `width`         | Label width constraint                                                        | `Width` property on Details                                                              |
| `min_width`     | Minimum label width                                                           | Not supported                                                                            |
| `max_width`     | Maximum label width                                                           | Not supported                                                                            |
| `color_scheme`  | Color theme for labels (e.g. `"tomato"`, `"red"`)                             | Not supported                                                                            |
| N/A             | N/A                                                                           | `RemoveEmpty()` — automatically hides null/empty/false fields                            |
| N/A             | N/A                                                                           | `Remove(x => x.Field)` — selectively hide specific fields                               |
| N/A             | N/A                                                                           | `Multiline()` — format specific fields across multiple lines                             |
| N/A             | N/A                                                                           | `Builder(x => x.Field, b => b.CopyToClipboard())` — make values clickable to copy       |
| N/A             | N/A                                                                           | `Builder(x => x.Field, b => b.Link())` — render URLs/emails as clickable links           |
| N/A             | N/A                                                                           | Nested object support — automatically renders child objects as hierarchical detail views  |
