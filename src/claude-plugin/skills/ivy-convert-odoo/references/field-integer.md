# Field: Integer

Odoo's integer field for whole number values. Used for quantities, counts, sequences, and IDs.

## Odoo

```python
class ProductTemplate(models.Model):
    _name = 'product.template'

    sequence = fields.Integer(string='Sequence', default=1)
    qty_available = fields.Float(string='Quantity On Hand')
    sale_count = fields.Integer(string='Sales Count', compute='_compute_sale_count')
    color = fields.Integer(string='Color Index')
    nbr_reordering_rules = fields.Integer(string='Reordering Rules', compute='_compute_nbr')
```

```xml
<!-- Basic integer input -->
<field name="sequence"/>

<!-- Read-only computed integer -->
<field name="sale_count" readonly="1"/>

<!-- With human-readable formatting -->
<field name="qty_available" options="{'human_readable': true}"/>

<!-- As stat button -->
<button class="oe_stat_button" icon="fa-list" type="object" name="action_view_reordering">
    <field name="nbr_reordering_rules" widget="statinfo" string="Reordering"/>
</button>

<!-- In list view with sum -->
<field name="qty_available" sum="Total Qty"/>
```

## Ivy

```csharp
// Integer → NumberInput
var sequence = UseState(1);
sequence.ToNumberInput()
    .Min(0)
    .Step(1)
    .WithField()
    .Label("Sequence");

// Read-only display
new TextBlock(saleCount.ToString());

// Or as Metric in a stat card
new Card(() => {
    new TextBlock(nbrReorderingRules.ToString()).Variant(TextBlockVariants.H2);
    new TextBlock("Reordering Rules").Variant(TextBlockVariants.Muted);
}).OnClick(e => Navigate("/reordering-rules"));

// Quantity input with min/max
var quantity = UseState(0);
quantity.ToNumberInput()
    .Min(0)
    .Max(1000)
    .Step(1)
    .WithField()
    .Label("Quantity");

// In DataTable with footer sum
items.ToDataTable()
    .Header(i => i.Name, "Product")
    .Header(i => i.QtyAvailable, "Qty Available")
    .Config(c => c.Sortable(true));

// Summary below table
new TextBlock($"Total: {items.Value.Sum(i => i.QtyAvailable)}").Bold();

// In form state class
public class ProductState
{
    [Display(Name = "Sequence")]
    [Range(0, int.MaxValue)]
    public int Sequence { get; set; } = 1;

    [Display(Name = "Quantity")]
    public int QtyAvailable { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Integer` | Integer field | `NumberInput` with `.Step(1)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `default=N` | Default value | Initial `UseState(N)` |
| `required=True` | Mandatory field | `.Required()` or `[Required]` annotation |
| `readonly=True` | Read-only display | `.Disabled(true)` or `TextBlock` |
| `widget="statinfo"` | Statistic display | `TextBlock` or Metric in Card |
| `options="{'human_readable': true}"` | Format large numbers | Custom formatting (e.g., "1.5K") |
| `options="{'type': 'number'}"` | HTML input type | Default NumberInput behavior |
| `options="{'step': N}"` | Increment step | `.Step(N)` |
| `sum="..."` | Column aggregate in list | Computed summary `TextBlock` below table |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `attrs="{'readonly':...}"` | Conditional read-only | `.Disabled(condition)` |
| `group_operator="avg"` | Group aggregation method | `.Average()` in LINQ |
