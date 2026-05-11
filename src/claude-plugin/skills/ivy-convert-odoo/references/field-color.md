# Field: Color

Odoo's color picker widget for selecting colors. Uses the native HTML5 color input. Stores colors as hex strings (e.g., `#ff0000`).

## Odoo

```python
class ProjectProject(models.Model):
    _name = 'project.project'

    color = fields.Integer(string='Color Index')

class EventEvent(models.Model):
    _name = 'event.event'

    color = fields.Char(string='Color', default='#875A7B')
```

```xml
<!-- Color picker widget -->
<field name="color" widget="color"/>

<!-- In list view (auto-saves on change) -->
<field name="color" widget="color"/>

<!-- In kanban card -->
<field name="color" widget="color"/>

<!-- Odoo color index (integer-based, not hex) -->
<field name="color" widget="color_picker"/>
```

## Ivy

```csharp
// Color → ColorInput
var color = UseState("#875A7B");
color.ToColorInput()
    .WithField()
    .Label("Color");

// Color picker with text display
color.ToColorInput()
    .Variant(ColorInputVariants.TextAndPicker)
    .WithField()
    .Label("Theme Color");

// Swatch variant (compact)
color.ToColorInput()
    .Variant(ColorInputVariants.Swatch)
    .WithField()
    .Label("Color");

// With alpha channel
color.ToColorInput()
    .AllowAlpha()
    .WithField()
    .Label("Background Color");

// Display color preview
new Box()
    .Background(color.Value)
    .Width(24)
    .Height(24)
    .BorderRadius(4);

// In form state class
public class ProjectFormState
{
    [Display(Name = "Color")]
    public string Color { get; set; } = "#875A7B";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="color"` | HTML5 color picker | `ColorInput` |
| Hex string storage | `#rrggbb` format | String state with hex value |
| `widget="color_picker"` | Odoo color index picker | `ColorInput` or predefined color `SelectInput` |
| Integer color index | Odoo's 0-11 color palette | Map integer to hex colors |
| Auto-save on change | Saves immediately | State update + API call in handler |
| `default='#875A7B'` | Default color | Initial `UseState("#875A7B")` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `Variant` options | Display style | `.Variant(ColorInputVariants.Picker/Text/Swatch)` |
