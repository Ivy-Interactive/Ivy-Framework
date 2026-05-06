# Field: Selection

Odoo's enumeration field for predefined choices. Renders as dropdown by default, with widget variants for radio buttons, badges, and status bars.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    state = fields.Selection([
        ('draft', 'Quotation'),
        ('sent', 'Quotation Sent'),
        ('sale', 'Sales Order'),
        ('done', 'Locked'),
        ('cancel', 'Cancelled'),
    ], string='Status', readonly=True, copy=False, index=True, default='draft')

    priority = fields.Selection([
        ('0', 'Normal'),
        ('1', 'Urgent'),
    ], string='Priority', default='0')

    invoice_status = fields.Selection([
        ('upselling', 'Upselling Opportunity'),
        ('invoiced', 'Fully Invoiced'),
        ('to invoice', 'To Invoice'),
        ('no', 'Nothing to Invoice'),
    ], string='Invoice Status', compute='_compute_invoice_status', store=True)
```

```xml
<!-- Default dropdown -->
<field name="priority"/>

<!-- Status bar widget -->
<field name="state" widget="statusbar"
       statusbar_visible="draft,sent,sale,done"/>

<!-- Badge widget -->
<field name="state" widget="badge"
       decoration-success="state == 'sale'"
       decoration-info="state == 'draft'"
       decoration-danger="state == 'cancel'"/>

<!-- Radio buttons -->
<field name="priority" widget="radio"/>

<!-- Selection badge (pills) -->
<field name="priority" widget="selection_badge"/>
```

## Ivy

```csharp
// Selection → SelectInput
var priority = UseState("0");
priority.ToSelectInput(new[] {
    new Option("0", "Normal"),
    new Option("1", "Urgent"),
}).WithField().Label("Priority");

// Status bar → SelectInput with Toggle variant or stepper
var state = UseState("draft");
state.ToSelectInput(new[] {
    new Option("draft", "Quotation"),
    new Option("sent", "Quotation Sent"),
    new Option("sale", "Sales Order"),
    new Option("done", "Locked"),
}.ToOptions()).Variant(SelectInputVariant.Toggle);

// Badge display (read-only)
new Badge(state.Value switch {
    "draft" => "Quotation",
    "sale" => "Sales Order",
    "cancel" => "Cancelled",
    _ => state.Value
}).Variant(state.Value switch {
    "sale" => BadgeVariants.Success,
    "draft" => BadgeVariants.Info,
    "cancel" => BadgeVariants.Destructive,
    _ => BadgeVariants.Default
});

// Radio buttons → List variant
priority.ToSelectInput(new[] {
    new Option("0", "Normal"),
    new Option("1", "Urgent"),
}).Variant(SelectInputVariants.List).WithField().Label("Priority");

// Selection badge (pills) → Toggle variant
priority.ToSelectInput(new[] {
    new Option("0", "Normal"),
    new Option("1", "Urgent"),
}).Variant(SelectInputVariants.Toggle).WithField().Label("Priority");

// In form state class
public class OrderState
{
    [Display(Name = "Status")]
    public string State { get; set; } = "draft";

    [Display(Name = "Priority")]
    public string Priority { get; set; } = "0";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Selection([...])` | Enum field with choices | `SelectInput` with `Option` list |
| `widget="statusbar"` | Horizontal step/status indicator | `SelectInput` with `.Variant(SelectInputVariant.Toggle)` |
| `statusbar_visible="..."` | Which steps to show | Filter options list |
| `widget="badge"` | Colored badge display | `Badge` widget with variant |
| `widget="radio"` | Radio button group | `.ToSelectInput().Variant(SelectInputVariants.List)` |
| `widget="selection_badge"` | Pill/segmented selector | `.ToSelectInput().Variant(SelectInputVariants.Toggle)` |
| `decoration-success` | Green color condition | `BadgeVariants.Success` |
| `decoration-info` | Blue color condition | `BadgeVariants.Info` |
| `decoration-danger` | Red color condition | `BadgeVariants.Destructive` |
| `decoration-warning` | Yellow color condition | `BadgeVariants.Warning` |
| `readonly=True` | Read-only display | `.Disabled(true)` or display as `Badge`/`Text` |
| `default='...'` | Default selection value | Initial `UseState` value |
| `copy=False` | Don't copy on duplicate | Business logic, not UI |
| computed field | Auto-calculated selection | `UseQuery` or derived state |
