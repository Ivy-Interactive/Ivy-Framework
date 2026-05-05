# Field: Priority

Odoo's star rating widget for Selection fields. Displays clickable stars to set priority levels. Commonly used for urgent/normal priority or star ratings on leads, tasks, and tickets.

## Odoo

```python
class CrmLead(models.Model):
    _name = 'crm.lead'

    priority = fields.Selection([
        ('0', 'Normal'),
        ('1', 'Important'),
        ('2', 'Very Important'),
        ('3', 'Urgent'),
    ], string='Priority', default='0', index=True)

class ProjectTask(models.Model):
    _name = 'project.task'

    priority = fields.Selection([
        ('0', 'Normal'),
        ('1', 'Urgent'),
    ], string='Priority', default='0')
```

```xml
<!-- Star priority in form -->
<field name="priority" widget="priority"/>

<!-- In kanban card -->
<field name="priority" widget="priority"/>

<!-- In list view -->
<field name="priority" widget="priority" optional="show"/>

<!-- Auto-save on click (default behavior) -->
<field name="priority" widget="priority" options="{'autosave': true}"/>

<!-- Without auto-save -->
<field name="priority" widget="priority" options="{'autosave': false}"/>
```

## Ivy

```csharp
// Priority stars → FeedbackInput or custom star rating
// Option 1: SelectInput with Toggle variant (pill selector)
var priority = UseState("0");
priority.ToSelectInput(new[] {
    new Option("0", "Normal"),
    new Option("1", "Important"),
    new Option("2", "Very Important"),
    new Option("3", "Urgent"),
}).Variant(SelectInputVariant.Toggle)
    .WithField()
    .Label("Priority");

// Option 2: Custom star display with buttons
PlaceHorizontal(() =>
{
    for (int i = 1; i <= 3; i++)
    {
        var starLevel = i;
        new Button("", onClick: async e => {
            priority.Set(priority.Value == starLevel.ToString() ? "0" : starLevel.ToString());
        })
        .Icon(int.Parse(priority.Value) >= starLevel ? Icons.StarFilled : Icons.Star)
        .Ghost();
    }
});

// Option 3: Badge-based priority display (read-only)
new Badge(priority.Value switch {
    "0" => "Normal",
    "1" => "Important",
    "2" => "Very Important",
    "3" => "Urgent",
    _ => "Normal"
}).Variant(priority.Value switch {
    "3" => BadgeVariants.Destructive,
    "2" => BadgeVariants.Warning,
    "1" => BadgeVariants.Info,
    _ => BadgeVariants.Secondary
});

// In DataTable
tasks.ToDataTable()
    .Header(t => t.Name, "Task")
    .Header(t => t.Priority, "Priority", format: v => new Badge(
        v == "1" ? "Urgent" : "Normal")
        .Variant(v == "1" ? BadgeVariants.Destructive : BadgeVariants.Secondary))
    .Header(t => t.StageName, "Stage");
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="priority"` | Star rating widget | `SelectInput` with Toggle variant or custom star buttons |
| Selection values `('0'-'3')` | Priority levels (0=no stars) | Option values mapped to star count |
| `options="{'autosave': true}"` | Save on click (default) | Immediate state update + API call |
| `options="{'autosave': false}"` | Don't save automatically | State update only, save with form |
| Click star to set | Set priority to clicked level | `onClick` handler setting state |
| Click active star | Reset to 0 (toggle off) | Toggle logic in click handler |
| `default='0'` | Default priority | Initial `UseState("0")` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| In kanban/list | Compact display | Badge or icon-based display |
