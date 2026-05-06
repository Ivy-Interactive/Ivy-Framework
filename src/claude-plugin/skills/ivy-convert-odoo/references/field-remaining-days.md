# Field: Remaining Days

Odoo's remaining days widget for date fields. Displays the number of days until or since a date using relative language ("In 3 days", "Today", "2 days ago"). Automatically applies color coding: red for overdue, yellow for today, green for future.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    validity_date = fields.Date(string='Expiration Date')

class ProjectTask(models.Model):
    _name = 'project.task'

    date_deadline = fields.Date(string='Deadline')
    date_last_stage_update = fields.Datetime(string='Last Stage Update')
```

```xml
<!-- Remaining days widget -->
<field name="validity_date" widget="remaining_days"/>

<!-- In list view -->
<field name="date_deadline" widget="remaining_days" optional="show"/>

<!-- In kanban card -->
<field name="date_deadline" widget="remaining_days"/>

<!-- With custom decoration classes -->
<field name="date_deadline" widget="remaining_days"
       options="{'classes': {'danger': 'date_deadline < today', 'warning': 'date_deadline == today'}}"/>
```

## Ivy

```csharp
// Remaining days → computed display with color-coded Badge
Badge RemainingDaysBadge(DateOnly? targetDate)
{
    if (targetDate == null) return new Badge("No date").Variant(BadgeVariants.Secondary);

    var today = DateOnly.FromDateTime(DateTime.Today);
    var diff = targetDate.Value.DayNumber - today.DayNumber;

    var text = diff switch {
        0 => "Today",
        1 => "Tomorrow",
        -1 => "Yesterday",
        > 1 and <= 99 => $"In {diff} days",
        < -1 and >= -99 => $"{-diff} days ago",
        _ => targetDate.Value.ToString("MMM dd, yyyy")
    };

    var variant = diff switch {
        < 0 => BadgeVariants.Destructive,
        0 => BadgeVariants.Warning,
        _ => BadgeVariants.Success
    };

    return new Badge(text).Variant(variant);
}

// Usage in form
RemainingDaysBadge(task.DateDeadline);

// In DataTable
tasks.ToDataTable()
    .Header(t => t.Name, "Task")
    .Header(t => t.DateDeadline, "Deadline", format: v => RemainingDaysBadge(v))
    .Header(t => t.StageName, "Stage");

// With additional context
PlaceHorizontal(() =>
{
    new TextBlock("Deadline:").Bold();
    RemainingDaysBadge(task.DateDeadline);
    new TextBlock(task.DateDeadline?.ToString("MMM dd, yyyy") ?? "").Muted();
});
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="remaining_days"` | Relative date display | Custom `Badge` with computed text and color |
| "In X days" | Future date display | `$"In {diff} days"` string |
| "X days ago" | Past date display | `$"{-diff} days ago"` string |
| "Today" | Current date display | `"Today"` string |
| Red color (danger) | Overdue dates | `BadgeVariants.Destructive` |
| Yellow color (warning) | Due today | `BadgeVariants.Warning` |
| Green/default | Future dates | `BadgeVariants.Success` |
| >99 days | Falls back to formatted date | `targetDate.ToString("MMM dd, yyyy")` |
| `options="{'classes': {...}}"` | Custom decoration rules | Custom badge variant logic |
| Date/Datetime support | Works with both types | `DateOnly` or `DateTime` comparison |
