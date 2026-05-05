# Activity View

Odoo's activity view displays scheduled activities for records in a matrix format, with records as rows and activity types as columns. Requires the `mail` module and model must inherit from `mail.activity.mixin`.

## Odoo

```python
class CrmLead(models.Model):
    _name = 'crm.lead'
    _inherit = ['mail.thread', 'mail.activity.mixin']

    name = fields.Char(string='Opportunity', required=True)
    partner_id = fields.Many2one('res.partner', string='Customer')
    user_id = fields.Many2one('res.users', string='Salesperson')
    stage_id = fields.Many2one('crm.stage', string='Stage')
    expected_revenue = fields.Monetary(string='Expected Revenue')
    date_deadline = fields.Date(string='Expected Closing')
    activity_ids = fields.One2many('mail.activity', 'res_id', string='Activities')
    activity_state = fields.Selection([
        ('overdue', 'Overdue'),
        ('today', 'Today'),
        ('planned', 'Planned'),
    ], string='Activity State', compute='_compute_activity_state')
```

```xml
<record id="crm_lead_view_activity" model="ir.ui.view">
    <field name="name">crm.lead.activity</field>
    <field name="model">crm.lead</field>
    <field name="arch" type="xml">
        <activity string="Leads Activity">
            <field name="partner_id"/>
            <field name="user_id"/>
            <field name="stage_id"/>
            <templates>
                <div t-name="activity-box">
                    <div>
                        <field name="name" display="full"/>
                        <field name="expected_revenue" widget="monetary"/>
                    </div>
                    <div class="text-muted">
                        <field name="partner_id" muted="1"/>
                    </div>
                </div>
            </templates>
        </activity>
    </field>
</record>
```

## Ivy

```csharp
// Activity views map to a custom task/activity tracking layout
// Show records with their scheduled activities as a table or card list

var leads = UseQuery(() => db.Leads
    .Include(l => l.Activities)
    .OrderBy(l => l.Activities.Min(a => a.DateDeadline))
    .ToList());

var activityTypes = UseQuery(() => db.ActivityTypes.ToList());

// Option 1: DataTable with activity status columns
leads.ToDataTable()
    .Header(l => l.Name, "Opportunity")
    .Header(l => l.PartnerName, "Customer")
    .Header(l => l.UserName, "Salesperson")
    .Header(l => l.ExpectedRevenue, "Revenue", format: v => $"{v:C}")
    .Header(l => l.ActivityState, "Activity Status", format: v => new Badge(v)
        .Variant(v switch {
            "overdue" => BadgeVariants.Destructive,
            "today" => BadgeVariants.Warning,
            "planned" => BadgeVariants.Info,
            _ => BadgeVariants.Secondary
        }))
    .Header(l => l.NextActivityDate, "Next Activity")
    .OnCellClick(async e => Navigate($"/lead/{e.Value.RowId}"));

// Option 2: Activity feed per record
foreach (var lead in leads.Value)
{
    Card(() =>
    {
        PlaceHorizontal(() =>
        {
            new TextBlock(lead.Name).Bold();
            new Badge(lead.StageName);
            new TextBlock($"{lead.ExpectedRevenue:C}").Muted();
        });

        foreach (var activity in lead.Activities.OrderBy(a => a.DateDeadline))
        {
            PlaceHorizontal(() =>
            {
                new Badge(activity.TypeName)
                    .Variant(activity.DateDeadline < DateOnly.FromDateTime(DateTime.Today)
                        ? BadgeVariants.Destructive : BadgeVariants.Info);
                new TextBlock(activity.Summary);
                new TextBlock(activity.DateDeadline.ToString("MMM dd")).Muted();
            });
        }
    });
}
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<activity>` | Root activity view element | Custom activity feed layout with DataTable or cards |
| `string="..."` | View title | Page heading |
| `<field name="...">` | Fields to fetch for template | Properties to include in query |
| `<templates>` | QWeb template definitions | Custom C# layout code |
| `<div t-name="activity-box">` | Record display template | `Card` or row layout per record |
| Activity types as columns | Matrix of activities per type | Columns in DataTable or grouped display |
| `activity_state` | Overdue/today/planned status | `Badge` with color variant based on date comparison |
| `mail.activity.mixin` | Activity mixin requirement | Custom activity tracking entity/table |
| Schedule activity button | Create new activity | `Button` with dialog for activity creation |
| Mark as done | Complete activity | Action button calling API endpoint |
| Activity icons/colors | Type-specific styling | `Badge` variant or `Icon` per activity type |
