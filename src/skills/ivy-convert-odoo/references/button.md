# Button

Odoo buttons trigger server-side actions, client-side actions, or URL navigation. Used in form headers, tree views, and kanban cards.

## Odoo

```xml
<!-- Object button - calls Python method -->
<button name="action_confirm" type="object" string="Confirm"
        class="oe_highlight" icon="fa-check"
        attrs="{'invisible': [('state', '!=', 'draft')]}"
        confirm="Are you sure you want to confirm?"/>

<!-- Action button - opens a window action -->
<button name="%(action_view_invoices)d" type="action" string="View Invoices"
        context="{'default_partner_id': active_id}"/>

<!-- Smart button (stat button) in form sheet -->
<div class="oe_button_box" name="button_box">
    <button name="action_view_orders" type="object" class="oe_stat_button" icon="fa-usd">
        <field name="sale_count" widget="statinfo" string="Sales"/>
    </button>
</div>
```

```python
def action_confirm(self):
    for record in self:
        record.write({'state': 'confirmed'})

def action_view_orders(self):
    return {
        'type': 'ir.actions.act_window',
        'name': 'Sales Orders',
        'res_model': 'sale.order',
        'view_mode': 'tree,form',
        'domain': [('partner_id', '=', self.id)],
    }
```

## Ivy

```csharp
// Basic action button
new Button("Confirm", onClick: async e => {
    await api.ConfirmRecord(recordId);
}).Primary().Icon(Icons.Check);

// Conditional visibility
new Button("Confirm", onClick: async e => {
    await api.ConfirmRecord(recordId);
}).Primary().Visible(state.Value == "draft");

// Confirmation dialog before action
new Button("Confirm", onClick: async e => {
    var confirmed = await Confirm("Are you sure you want to confirm?");
    if (confirmed) await api.ConfirmRecord(recordId);
});

// Navigation button (like action type)
new Button("View Invoices", onClick: e => {
    Navigate($"/invoices?partnerId={partnerId}");
}).Ghost();

// Stat button → Metric + clickable card
new Card(() => {
    new Metric("Sales", saleCount.ToString());
}).OnClick(e => Navigate($"/orders?partnerId={partnerId}"));
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `type="object"` | Calls Python method on model | `onClick` handler calling API/server action |
| `type="action"` | Opens a window action | `Navigate()` to route or open dialog |
| `string="..."` | Button label text | First param: `new Button("Label")` |
| `class="oe_highlight"` | Primary button style | `.Primary()` |
| `class="oe_stat_button"` | Stat/smart button | `Card` with `Metric` widget |
| `icon="fa-*"` | FontAwesome icon | `.Icon(Icons.*)` |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `confirm="..."` | Confirmation dialog | `await Confirm("message")` before action |
| `context="{...}"` | Pass context to action | Query params or state passed to target |
| `widget="statinfo"` | Stat info display | `Metric` widget |
| `groups="..."` | Access group visibility | Role-based `.Visible()` check |
