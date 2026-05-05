# Field: Float

Odoo's decimal number field for values requiring fractional precision. Used for quantities, weights, percentages, and measurements. Supports configurable precision via digits parameter.

## Odoo

```python
class SaleOrderLine(models.Model):
    _name = 'sale.order.line'

    product_uom_qty = fields.Float(string='Quantity', digits='Product Unit of Measure',
                                    default=1.0, required=True)
    price_unit = fields.Float(string='Unit Price', digits='Product Price')
    discount = fields.Float(string='Discount (%)', digits=(16, 2))
    price_subtotal = fields.Float(string='Subtotal', compute='_compute_amount',
                                   digits='Account', store=True)
    weight = fields.Float(string='Weight', digits='Stock Weight')
```

```xml
<!-- Basic float input -->
<field name="product_uom_qty"/>

<!-- With explicit precision display -->
<field name="price_unit" widget="float" options="{'digits': [16, 4]}"/>

<!-- Float as time (hours:minutes) -->
<field name="planned_hours" widget="float_time"/>

<!-- Float factor (convert units) -->
<field name="timesheet_hours" widget="float_factor" options="{'factor': 24}"/>

<!-- Read-only with formatting -->
<field name="price_subtotal" readonly="1"/>

<!-- In list view with sum/avg -->
<field name="discount" avg="Average Discount"/>
<field name="price_subtotal" sum="Total"/>

<!-- Hide trailing zeros -->
<field name="weight" options="{'hide_trailing_zeros': true}"/>
```

## Ivy

```csharp
// Float → NumberInput with precision
var quantity = UseState(1.0);
quantity.ToNumberInput()
    .Min(0)
    .Step(0.01)
    .Precision(2)
    .WithField()
    .Label("Quantity")
    .Required();

// Price with 4 decimal precision
var priceUnit = UseState(0.0);
priceUnit.ToNumberInput()
    .Precision(4)
    .Step(0.0001)
    .Prefix("$")
    .WithField()
    .Label("Unit Price");

// Discount percentage
var discount = UseState(0.0);
discount.ToNumberInput()
    .Min(0)
    .Max(100)
    .Precision(2)
    .Suffix("%")
    .WithField()
    .Label("Discount (%)");

// Float time (hours) → display as hours:minutes
var plannedHours = UseState(0.0);
plannedHours.ToNumberInput()
    .Precision(2)
    .Suffix("h")
    .WithField()
    .Label("Planned Hours");

// Read-only computed value
new TextBlock($"{priceSubtotal:F2}");

// In DataTable with aggregates
orderLines.ToDataTable()
    .Header(l => l.ProductName, "Product")
    .Header(l => l.Quantity, "Qty")
    .Header(l => l.PriceUnit, "Unit Price", format: v => $"{v:F2}")
    .Header(l => l.Discount, "Discount", format: v => $"{v:F1}%")
    .Header(l => l.PriceSubtotal, "Subtotal", format: v => $"{v:C}")
    .Config(c => c.Sortable(true));

// Summary below table
new TextBlock($"Total: {orderLines.Value.Sum(l => l.PriceSubtotal):C}").Bold();

// In form state class
public class OrderLineState
{
    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Quantity")]
    public double Quantity { get; set; } = 1.0;

    [Display(Name = "Unit Price")]
    public double PriceUnit { get; set; }

    [Range(0, 100)]
    [Display(Name = "Discount (%)")]
    public double Discount { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Float` | Decimal number field | `NumberInput` |
| `digits=(16, 2)` | Precision (total, decimal) | `.Precision(2)` |
| `digits='Product Price'` | Named precision reference | `.Precision(N)` with appropriate value |
| `widget="float_time"` | Display as hours:minutes | `.Suffix("h")` or custom time format |
| `widget="float_factor"` | Multiply by factor for display | Custom display formatting |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `required=True` | Mandatory field | `.Required()` or `[Required]` |
| `readonly=True` | Read-only display | `.Disabled(true)` or `TextBlock` |
| `default=1.0` | Default value | Initial `UseState(1.0)` |
| `options="{'digits': [16, 4]}"` | Override precision | `.Precision(4)` |
| `options="{'hide_trailing_zeros': true}"` | Remove trailing zeros | Custom format string |
| `sum="..."` / `avg="..."` | Column aggregate in list | Computed summary `TextBlock` below table |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `group_operator` | Aggregation method | LINQ `.Sum()`, `.Average()`, etc. |
