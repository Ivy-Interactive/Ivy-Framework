# Server Actions & Automated Actions

Odoo's server actions (`ir.actions.server`) execute server-side logic triggered by buttons, menu items, or automated rules. Automated actions (`base.automation`) run on record events (create, write, timed). Used for workflow automation, scheduled tasks, and bulk operations.

## Odoo

```python
# Server action defined in Python
class SaleOrder(models.Model):
    _name = 'sale.order'

    def action_send_reminder(self):
        """Server action: send reminder emails"""
        for order in self:
            template = self.env.ref('sale.email_template_reminder')
            template.send_mail(order.id)

    def action_mass_confirm(self):
        """Bulk server action from list view"""
        for order in self.filtered(lambda o: o.state == 'draft'):
            order.action_confirm()
```

```xml
<!-- Server action definition -->
<record id="action_send_reminder" model="ir.actions.server">
    <field name="name">Send Reminder</field>
    <field name="model_id" ref="model_sale_order"/>
    <field name="binding_model_id" ref="model_sale_order"/>
    <field name="binding_view_types">list,form</field>
    <field name="state">code</field>
    <field name="code">
        for record in records:
            record.action_send_reminder()
    </field>
</record>

<!-- Automated action (trigger on events) -->
<record id="auto_action_assign_salesperson" model="base.automation">
    <field name="name">Auto-assign Salesperson</field>
    <field name="model_id" ref="model_sale_order"/>
    <field name="trigger">on_create</field>
    <field name="filter_domain">[('user_id', '=', False)]</field>
    <field name="state">code</field>
    <field name="code">
        for record in records:
            record.user_id = record.partner_id.user_id or record.env.user
    </field>
</record>

<!-- Timed automated action -->
<record id="auto_action_escalate_overdue" model="base.automation">
    <field name="name">Escalate Overdue Orders</field>
    <field name="model_id" ref="model_sale_order"/>
    <field name="trigger">on_time</field>
    <field name="trigger_field_id" ref="field_sale_order__commitment_date"/>
    <field name="trg_date_range">-2</field>
    <field name="trg_date_range_type">day</field>
    <field name="filter_domain">[('state', '=', 'sale')]</field>
    <field name="state">code</field>
    <field name="code">
        for record in records:
            record.priority = '1'
            record.message_post(body="Order escalated: approaching commitment date")
    </field>
</record>
```

## Ivy

```csharp
// Server actions → Button onClick handlers calling API endpoints
// Automated actions → scheduled background tasks or event hooks

// Server action: button-triggered
new Button("Send Reminder", onClick: async e => {
    await api.SendReminder(orderId);
}).Ghost().Icon(Icons.Mail);

// Bulk action from DataTable selection
var selectedOrders = UseState(new List<int>());
orders.ToDataTable()
    .Header(o => o.Name, "Order")
    .Header(o => o.State, "Status")
    .Selectable(selectedOrders)
    .BulkActions(new[] {
        new BulkAction("Confirm Selected", async ids => {
            await api.BulkConfirmOrders(ids);
        }),
        new BulkAction("Send Reminders", async ids => {
            await api.BulkSendReminders(ids);
        }),
    });

// Automated action on create → server-side event hook
// In API/service layer:
// public async Task OnOrderCreated(SaleOrder order)
// {
//     if (order.UserId == null)
//         order.UserId = order.Partner?.SalespersonId ?? currentUserId;
// }

// Timed/scheduled action → background job
// Use a scheduled task or cron job on the server:
// [Scheduled("0 8 * * *")] // Daily at 8 AM
// public async Task EscalateOverdueOrders()
// {
//     var overdue = await db.SaleOrders
//         .Where(o => o.State == "sale"
//             && o.CommitmentDate <= DateTime.Today.AddDays(2))
//         .ToListAsync();
//     foreach (var order in overdue)
//     {
//         order.Priority = "1";
//         await PostMessage(order.Id, "Order escalated: approaching commitment date");
//     }
// }

// Display scheduled tasks status
var pendingTasks = UseQuery(() => db.ScheduledTasks
    .Where(t => t.Status == "pending")
    .OrderBy(t => t.NextRun)
    .ToList());

pendingTasks.ToDataTable()
    .Header(t => t.Name, "Task")
    .Header(t => t.NextRun, "Next Run")
    .Header(t => t.Status, "Status", format: v => new Badge(v));
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `ir.actions.server` | Server action definition | API endpoint called from `onClick` handler |
| `state="code"` | Python code execution | C# server-side method |
| `binding_model_id` | Attach to model's action menu | Button or bulk action on DataTable |
| `binding_view_types` | Available in list/form | Button placement in appropriate view |
| `base.automation` | Automated action | Server-side event hook or scheduled task |
| `trigger="on_create"` | Trigger on record creation | `OnCreated` event handler in service layer |
| `trigger="on_write"` | Trigger on record update | `OnUpdated` event handler in service layer |
| `trigger="on_time"` | Time-based trigger | Scheduled/cron job |
| `trigger_field_id` | Date field for time trigger | Date field referenced in scheduled query |
| `trg_date_range` | Time offset from trigger date | Date arithmetic in scheduled query filter |
| `filter_domain` | Filter records for action | `.Where()` clause in automation query |
| Bulk action in list | Action on selected records | `.Selectable()` + `.BulkActions()` on DataTable |
