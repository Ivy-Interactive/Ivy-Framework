# Key Value (Legacy)

Displays key-value information as a two-column layout with labeled rows. This is a legacy Retool component for presenting structured data as property/value pairs. Retool recommends using the newer [Key Value](https://docs.retool.com/apps/reference/components/key-value) component instead.

## Retool

```toolscript
legacyKeyValue1.data = {
  "Name": "John Doe",
  "Email": "john@example.com",
  "Age": 30
};
legacyKeyValue1.keyTitle = "Property";
legacyKeyValue1.valueTitle = "Value";
```

## Ivy

The closest equivalent in Ivy is the **Details** widget, which displays structured label-value pairs from models using the `ToDetails()` extension method.

```csharp
new { Name = "John Doe", Email = "john@example.com", Age = 30 }
    .ToDetails()
```

## Parameters

| Parameter              | Documentation                                                                 | Ivy                                                                                      |
|------------------------|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `data`                 | The data to display (string, number, boolean, object, or array).              | Passed as the object that `ToDetails()` is called on.                                    |
| `keyTitle`             | The title of the Key column.                                                  | Not supported (labels are derived from property names).                                  |
| `valueTitle`           | The title of the Value column.                                                | Not supported.                                                                           |
| `rows`                 | A list of row keys.                                                           | Derived automatically from the object's properties.                                      |
| `rowVisibility`        | A list of visible rows.                                                       | `.Remove(x => x.FieldName)` or `.RemoveEmpty()` to hide fields.                         |
| `hidden`               | Whether the component is hidden from view.                                    | `Visible` property on the widget.                                                        |
| `isHiddenOnDesktop`    | Whether to hide in the desktop layout.                                        | Not supported (no separate desktop/mobile layouts).                                      |
| `isHiddenOnMobile`     | Whether to hide in the mobile layout.                                         | Not supported (no separate desktop/mobile layouts).                                      |
| `maintainSpaceWhenHidden` | Whether to take up space when hidden.                                      | Not supported.                                                                           |
| `margin`               | The amount of margin to render outside (`4px 8px` or `0`).                    | Not supported (layout handled differently).                                              |
| `style`                | Custom style options.                                                         | Not supported (styling handled via Ivy theming).                                         |
| `id`                   | The unique identifier (name).                                                 | Not applicable (widgets are referenced by variable binding).                             |
| `showInEditor`         | Whether the component remains visible in the editor when hidden.              | Not supported.                                                                           |
| `scrollIntoView()`     | Method to scroll the component into the visible area.                         | Not supported.                                                                           |
