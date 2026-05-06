# Field: Percentage

Odoo's percentage widget for displaying numeric values as percentages. The stored value is a decimal (e.g., 0.75 = 75%) and the widget handles the conversion for display and input.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    margin_percent = fields.Float(string='Margin (%)', compute='_compute_margin',
                                    digits=(5, 2))
    tax_rate = fields.Float(string='Tax Rate')
    completion_rate = fields.Float(string='Completion Rate')
```

```xml
<!-- Percentage widget -->
<field name="margin_percent" widget="percentage"/>

<!-- With precision -->
<field name="margin_percent" widget="percentage" options="{'digits': [5, 2]}"/>

<!-- In list view -->
<field name="completion_rate" widget="percentage" optional="show"/>

<!-- Read-only percentage -->
<field name="margin_percent" widget="percentage" readonly="1"/>
```

## Ivy

```csharp
// Percentage → NumberInput with % suffix
var marginPercent = UseState(0.0);
marginPercent.ToNumberInput()
    .Min(0)
    .Max(100)
    .Precision(2)
    .Suffix("%")
    .WithField()
    .Label("Margin (%)");

// Or format as percentage style
var taxRate = UseState(0.0);
taxRate.ToNumberInput()
    .FormatStyle(NumberFormatStyles.Percent)
    .Precision(2)
    .WithField()
    .Label("Tax Rate");

// Read-only percentage display
new TextBlock($"{marginPercent.Value:P1}");

// In DataTable
orders.ToDataTable()
    .Header(o => o.Name, "Order")
    .Header(o => o.MarginPercent, "Margin", format: v => $"{v:F1}%")
    .Header(o => o.CompletionRate, "Completion", format: v =>
        new Progress((int)(v * 100)));

// In form state class
public class OrderFormState
{
    [Display(Name = "Margin (%)")]
    [Range(0, 100)]
    public double MarginPercent { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="percentage"` | Percentage display/input | `NumberInput` with `.Suffix("%")` or `.FormatStyle(NumberFormatStyles.Percent)` |
| `options="{'digits': [5, 2]}"` | Precision control | `.Precision(2)` |
| Stored as decimal (0.75) | Value × 100 for display | Depends on storage format |
| Display with % symbol | Percentage formatting | `.Suffix("%")` or `$"{v:P1}"` |
| `readonly=True` | Read-only display | `.Disabled(true)` or `TextBlock` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
