# Field: Badge

Odoo's badge widget for displaying field values as colored pill/label indicators. Works with Selection, Many2one, and Char fields. Supports conditional coloring via decoration attributes.

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
    ], string='Status', default='draft')

    invoice_status = fields.Selection([
        ('no', 'Nothing to Invoice'),
        ('to invoice', 'To Invoice'),
        ('invoiced', 'Fully Invoiced'),
    ], string='Invoice Status')
```

```xml
<!-- Basic badge -->
<field name="state" widget="badge"/>

<!-- Badge with conditional colors -->
<field name="state" widget="badge"
       decoration-success="state in ('sale', 'done')"
       decoration-info="state == 'draft'"
       decoration-warning="state == 'sent'"
       decoration-danger="state == 'cancel'"/>

<!-- Badge with color from field -->
<field name="stage_id" widget="badge" options="{'color_field': 'color'}"/>

<!-- In list view -->
<field name="invoice_status" widget="badge"
       decoration-success="invoice_status == 'invoiced'"
       decoration-info="invoice_status == 'to invoice'"
       decoration-warning="invoice_status == 'no'"/>

<!-- In kanban card -->
<field name="state" widget="badge"
       decoration-success="state == 'done'"
       decoration-danger="state == 'cancel'"/>
```

## Ivy

```csharp
// Badge display with conditional variants
new Badge(state.Value switch {
    "draft" => "Quotation",
    "sent" => "Quotation Sent",
    "sale" => "Sales Order",
    "done" => "Locked",
    "cancel" => "Cancelled",
    _ => state.Value
}).Variant(state.Value switch {
    "sale" or "done" => BadgeVariants.Success,
    "draft" => BadgeVariants.Info,
    "sent" => BadgeVariants.Warning,
    "cancel" => BadgeVariants.Destructive,
    _ => BadgeVariants.Secondary
});

// Badge with icon
new Badge("Active")
    .Variant(BadgeVariants.Success)
    .Icon(Icons.Check);

// Invoice status badge
new Badge(invoiceStatus switch {
    "invoiced" => "Fully Invoiced",
    "to invoice" => "To Invoice",
    "no" => "Nothing to Invoice",
    _ => invoiceStatus
}).Variant(invoiceStatus switch {
    "invoiced" => BadgeVariants.Success,
    "to invoice" => BadgeVariants.Info,
    _ => BadgeVariants.Warning
});

// In DataTable columns
orders.ToDataTable()
    .Header(o => o.Name, "Order")
    .Header(o => o.State, "Status", format: v => new Badge(v)
        .Variant(v switch {
            "sale" => BadgeVariants.Success,
            "draft" => BadgeVariants.Info,
            "cancel" => BadgeVariants.Destructive,
            _ => BadgeVariants.Secondary
        }))
    .Header(o => o.InvoiceStatus, "Invoice Status", format: v => new Badge(v)
        .Variant(v == "invoiced" ? BadgeVariants.Success : BadgeVariants.Warning));

// Clickable badge
new Badge("Draft")
    .Variant(BadgeVariants.Info)
    .OnClick(e => Navigate($"/order/{orderId}"));
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="badge"` | Badge/pill display | `new Badge(text)` |
| `decoration-success="expr"` | Green when expression true | `.Variant(BadgeVariants.Success)` |
| `decoration-info="expr"` | Blue when expression true | `.Variant(BadgeVariants.Info)` |
| `decoration-warning="expr"` | Yellow when expression true | `.Variant(BadgeVariants.Warning)` |
| `decoration-danger="expr"` | Red when expression true | `.Variant(BadgeVariants.Destructive)` |
| `decoration-secondary="expr"` | Gray when expression true | `.Variant(BadgeVariants.Secondary)` |
| `options="{'color_field': '...'}"` | Color from integer field | Map color index to `BadgeVariants` |
| Selection field | Display selected label | Map value to display string |
| Many2one field | Display record name | Use related record's display name |
| Char field | Display raw text | Direct text content |
