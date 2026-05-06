# Workflow States

Odoo's `state` field pattern for tracking record lifecycle. Uses a Selection field combined with buttons in the form header to drive transitions between states (e.g., Draft → Confirmed → Done). Buttons call Python methods that update the state.

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
    ], string='Status', readonly=True, copy=False, default='draft', tracking=True)

    def action_confirm(self):
        for order in self:
            order.write({'state': 'sale'})

    def action_cancel(self):
        self.write({'state': 'cancel'})

    def action_draft(self):
        self.write({'state': 'draft'})

    def action_done(self):
        self.write({'state': 'done'})

class RepairOrder(models.Model):
    _name = 'repair.order'

    state = fields.Selection([
        ('draft', 'Quotation'),
        ('confirmed', 'Confirmed'),
        ('under_repair', 'Under Repair'),
        ('ready', 'Ready to Repair'),
        ('2binvoiced', 'To be Invoiced'),
        ('invoice_except', 'Invoice Exception'),
        ('done', 'Repaired'),
        ('cancel', 'Cancelled'),
    ], string='Status', default='draft', tracking=True)
```

```xml
<header>
    <button name="action_confirm" type="object" string="Confirm Order"
            class="oe_highlight"
            attrs="{'invisible': [('state', '!=', 'draft')]}"/>
    <button name="action_done" type="object" string="Lock"
            attrs="{'invisible': [('state', '!=', 'sale')]}"/>
    <button name="action_cancel" type="object" string="Cancel"
            attrs="{'invisible': [('state', 'in', ['done', 'cancel'])]}"/>
    <button name="action_draft" type="object" string="Set to Draft"
            attrs="{'invisible': [('state', '!=', 'cancel')]}"/>
    <field name="state" widget="statusbar"
           statusbar_visible="draft,sent,sale,done"/>
</header>
```

## Ivy

```csharp
// Workflow state → UseState + conditional buttons + status bar
var state = UseState("draft");

// Status bar display
PlaceHorizontal(() =>
{
    // Transition buttons (visible based on current state)
    new Button("Confirm Order", onClick: async e => {
        await api.ConfirmOrder(orderId);
        state.Set("sale");
    }).Primary().Visible(state.Value == "draft");

    new Button("Lock", onClick: async e => {
        await api.LockOrder(orderId);
        state.Set("done");
    }).Visible(state.Value == "sale");

    new Button("Cancel", onClick: async e => {
        var confirmed = await Confirm("Are you sure you want to cancel?");
        if (confirmed)
        {
            await api.CancelOrder(orderId);
            state.Set("cancel");
        }
    }).Destructive()
        .Visible(state.Value != "done" && state.Value != "cancel");

    new Button("Set to Draft", onClick: async e => {
        await api.ResetToDraft(orderId);
        state.Set("draft");
    }).Ghost().Visible(state.Value == "cancel");

    new Spacer();

    // Status bar
    state.ToSelectInput(new[] {
        new Option("draft", "Quotation"),
        new Option("sent", "Quotation Sent"),
        new Option("sale", "Sales Order"),
        new Option("done", "Locked"),
    }).Variant(SelectInputVariant.Toggle).Disabled(true);
});

new Separator();

// Complex multi-step workflow with guards
var repairState = UseState("draft");

async Task TransitionTo(string newState)
{
    // Server validates transition is allowed
    await api.UpdateRepairState(repairId, newState);
    repairState.Set(newState);
}
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `fields.Selection` state field | Workflow state definition | `UseState("initial")` with string values |
| `def action_confirm(self)` | State transition method | `onClick` handler calling API + `state.Set()` |
| `self.write({'state': 'new'})` | State update | `await api.UpdateState()` + `state.Set("new")` |
| `attrs="{'invisible': [...]}"` | Conditional button display | `.Visible(state.Value == "expected")` |
| `class="oe_highlight"` | Primary action button | `.Primary()` on Button |
| `widget="statusbar"` | Visual state progression | `SelectInput` with `.Variant(SelectInputVariant.Toggle)` |
| `statusbar_visible="a,b,c"` | Visible stages subset | Filter options to exclude hidden states (e.g., cancel) |
| `tracking=True` | Log state changes | Server-side audit logging |
| `readonly=True` on state | Prevent direct editing | `.Disabled(true)` on SelectInput |
| `copy=False` | Don't copy state on duplicate | Business logic, not UI |
