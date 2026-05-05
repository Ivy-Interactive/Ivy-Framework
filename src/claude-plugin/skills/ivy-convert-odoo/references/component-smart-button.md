# Component: Smart Button (Stat Button)

Odoo's stat/smart buttons displayed at the top of form views inside the sheet. Show key metrics with icons and navigate to related records on click. Commonly used to display counts of related records (invoices, deliveries, tasks, etc.).

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    invoice_count = fields.Integer(string='Invoice Count', compute='_compute_invoice_count')
    delivery_count = fields.Integer(string='Delivery Count', compute='_compute_delivery_count')
    task_count = fields.Integer(string='Task Count', compute='_compute_task_count')

    def _compute_invoice_count(self):
        for order in self:
            order.invoice_count = len(order.invoice_ids)

    def action_view_invoice(self):
        return {
            'type': 'ir.actions.act_window',
            'name': 'Invoices',
            'res_model': 'account.move',
            'view_mode': 'tree,form',
            'domain': [('id', 'in', self.invoice_ids.ids)],
        }
```

```xml
<sheet>
    <div class="oe_button_box" name="button_box">
        <!-- Invoice stat button -->
        <button class="oe_stat_button" icon="fa-pencil-square-o"
                type="object" name="action_view_invoice"
                attrs="{'invisible': [('invoice_count', '=', 0)]}">
            <field name="invoice_count" widget="statinfo" string="Invoices"/>
        </button>

        <!-- Delivery stat button -->
        <button class="oe_stat_button" icon="fa-truck"
                type="object" name="action_view_delivery">
            <field name="delivery_count" widget="statinfo" string="Deliveries"/>
        </button>

        <!-- Task stat button with percentage -->
        <button class="oe_stat_button" icon="fa-tasks"
                type="object" name="action_view_tasks">
            <div class="o_stat_info">
                <span class="o_stat_value"><field name="task_count"/></span>
                <span class="o_stat_text">Tasks</span>
            </div>
        </button>

        <!-- Boolean stat button (toggle) -->
        <button class="oe_stat_button" icon="fa-star"
                type="object" name="toggle_active">
            <field name="active" widget="boolean_button"
                   options="{'terminology': 'archive'}"/>
        </button>
    </div>

    <!-- Rest of form -->
</sheet>
```

## Ivy

```csharp
// Smart buttons → Row of clickable Card widgets with metrics

PlaceHorizontal(() =>
{
    // Invoice stat button
    if (invoiceCount.Value > 0)
    {
        new Card(() =>
        {
            PlaceHorizontal(() =>
            {
                new Icon(Icons.FileText);
                Column(() =>
                {
                    new TextBlock(invoiceCount.Value.ToString())
                        .Variant(TextBlockVariants.H3);
                    new TextBlock("Invoices").Variant(TextBlockVariants.Muted);
                });
            });
        }).OnClick(e => Navigate($"/invoices?orderId={orderId}"));
    }

    // Delivery stat button
    new Card(() =>
    {
        PlaceHorizontal(() =>
        {
            new Icon(Icons.Truck);
            Column(() =>
            {
                new TextBlock(deliveryCount.Value.ToString())
                    .Variant(TextBlockVariants.H3);
                new TextBlock("Deliveries").Variant(TextBlockVariants.Muted);
            });
        });
    }).OnClick(e => Navigate($"/deliveries?orderId={orderId}"));

    // Task stat button
    new Card(() =>
    {
        PlaceHorizontal(() =>
        {
            new Icon(Icons.ListChecks);
            Column(() =>
            {
                new TextBlock(taskCount.Value.ToString())
                    .Variant(TextBlockVariants.H3);
                new TextBlock("Tasks").Variant(TextBlockVariants.Muted);
            });
        });
    }).OnClick(e => Navigate($"/tasks?orderId={orderId}"));

    // Active toggle button
    new Card(() =>
    {
        PlaceHorizontal(() =>
        {
            new Icon(isActive.Value ? Icons.Star : Icons.StarOff);
            new TextBlock(isActive.Value ? "Active" : "Archived");
        });
    }).OnClick(async e => {
        await api.ToggleActive(orderId);
    });
});
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<div class="oe_button_box">` | Stat button container | `PlaceHorizontal` with Card widgets |
| `class="oe_stat_button"` | Individual stat button | `Card` with `OnClick` handler |
| `icon="fa-*"` | FontAwesome icon | `Icon` widget with Ivy icon |
| `widget="statinfo"` | Number + label display | `TextBlock` for value + muted label |
| `string="..."` | Stat label text | Text in stat Card |
| `type="object" name="..."` | Click action (Python method) | `OnClick` handler with Navigate or API call |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` or `if` statement |
| `widget="boolean_button"` | Toggle button (active/archive) | Card with toggle icon and click handler |
| Count display | Computed field value | Query result count |
| Navigation on click | Opens related records list | `Navigate()` to filtered list page |
