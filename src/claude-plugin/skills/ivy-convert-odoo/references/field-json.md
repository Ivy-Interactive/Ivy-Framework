# Field: JSON

Odoo's JSON field for storing and displaying structured JSON data. Read-only formatted display by default. Used for configuration data, API responses, and dynamic property storage.

## Odoo

```python
class IrConfigParameter(models.Model):
    _name = 'ir.config_parameter'

    value = fields.Text(string='Value')

class CustomModel(models.Model):
    _name = 'custom.model'

    metadata = fields.Json(string='Metadata')
    config = fields.Json(string='Configuration', default=dict)
```

```xml
<!-- JSON display widget (read-only formatted) -->
<field name="metadata" widget="json"/>

<!-- As text for editing -->
<field name="config"/>

<!-- In list view -->
<field name="metadata" widget="json" optional="hide"/>
```

## Ivy

```csharp
// JSON display → Json widget (formatted read-only)
new Json(metadata);

// JSON editing → CodeInput with JSON language
var config = UseState("{}");
config.ToCodeInput()
    .Language("json")
    .WithField()
    .Label("Configuration");

// Or TextInput with Textarea for simple JSON editing
var jsonData = UseState("{}");
jsonData.ToTextInput()
    .Variant(TextInputVariant.Textarea)
    .Placeholder("{}")
    .WithField()
    .Label("Metadata");

// Structured JSON display
new TextBlock(jsonString).Variant(TextBlockVariants.Json);

// In form state class
public class CustomModelState
{
    [Display(Name = "Metadata")]
    public string Metadata { get; set; } = "{}";

    [Display(Name = "Configuration")]
    public Dictionary<string, object> Config { get; set; } = new();
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Json` | JSON storage field | `string` or `Dictionary<string, object>` |
| `widget="json"` | Formatted JSON display | `Json` widget or `TextBlock` with Json variant |
| Read-only display | Formatted JSON output | `Json` or `CodeBlock` with JSON highlighting |
| Editable JSON | Text-based editing | `CodeInput` with `.Language("json")` |
| `default=dict` | Default empty dict | Initial `UseState("{}")` or `new Dictionary<>()` |
| `string="..."` | Field label | `.WithField().Label("...")` |
