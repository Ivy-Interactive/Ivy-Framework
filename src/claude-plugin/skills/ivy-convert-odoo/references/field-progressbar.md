# Field: Progressbar

Odoo's progress bar widget for displaying completion percentages. Shows a horizontal bar with optional text showing current/max values. Used for task completion, project progress, and capacity indicators.

## Odoo

```python
class ProjectTask(models.Model):
    _name = 'project.task'

    progress = fields.Float(string='Progress', compute='_compute_progress',
                             store=True, group_operator="avg")
    planned_hours = fields.Float(string='Planned Hours')
    effective_hours = fields.Float(string='Hours Spent', compute='_compute_effective_hours')
    remaining_hours = fields.Float(string='Remaining Hours', compute='_compute_remaining')
```

```xml
<!-- Basic progress bar -->
<field name="progress" widget="progressbar"/>

<!-- With max value from another field -->
<field name="effective_hours" widget="progressbar"
       options="{'max_value': 'planned_hours', 'editable': true}"/>

<!-- Editable progress bar -->
<field name="progress" widget="progressbar"
       options="{'editable': true, 'edit_max_value': true}"/>

<!-- With current value override -->
<field name="progress" widget="progressbar"
       options="{'current_value': 'effective_hours', 'max_value': 'planned_hours'}"/>

<!-- In kanban card -->
<field name="progress" widget="progressbar"/>

<!-- In list view -->
<field name="progress" widget="progressbar" optional="show"/>

<!-- With overflow styling -->
<field name="effective_hours" widget="progressbar"
       options="{'max_value': 'planned_hours', 'overflow_class': 'bg-danger'}"/>
```

## Ivy

```csharp
// Progress bar → Progress widget
new Progress((int)task.Progress)
    .Color("blue");

// With goal/max value
new Progress((int)effectiveHours)
    .Goal((int)plannedHours)
    .Color(effectiveHours > plannedHours ? "red" : "blue");

// Editable progress
var progress = UseState(0);
progress.ToNumberInput()
    .Min(0)
    .Max(100)
    .Suffix("%")
    .Variant(NumberInputVariants.Slider)
    .WithField()
    .Label("Progress");
new Progress(progress.Value);

// Progress with label
PlaceHorizontal(() =>
{
    new Progress((int)(effectiveHours / plannedHours * 100))
        .Color(effectiveHours > plannedHours ? "red" : "green");
    new TextBlock($"{effectiveHours:F1} / {plannedHours:F1}h").Muted();
});

// Indeterminate progress (loading)
new Progress(0).Indeterminate();

// In DataTable
tasks.ToDataTable()
    .Header(t => t.Name, "Task")
    .Header(t => t.Progress, "Progress", format: v =>
        new Progress((int)v).Color(v >= 100 ? "green" : v > 50 ? "blue" : "orange"))
    .Header(t => t.EffectiveHours, "Hours", format: v => $"{v:F1}h");

// Multiple progress bars for dashboard
foreach (var project in projects.Value)
{
    Card(() =>
    {
        new TextBlock(project.Name).Bold();
        new Progress((int)project.CompletionRate)
            .Color(project.CompletionRate >= 100 ? "green" : "blue");
        new TextBlock($"{project.CompletionRate:F0}% complete").Muted();
    });
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="progressbar"` | Progress bar widget | `Progress` widget |
| Value (0-100) | Completion percentage | `new Progress(percentValue)` |
| `options="{'max_value': 'field'}"` | Field for max value | `.Goal(maxValue)` on Progress |
| `options="{'max_value': 100}"` | Static max value | `.Goal(100)` |
| `options="{'current_value': 'field'}"` | Override display value | Compute percentage from current/max |
| `options="{'editable': true}"` | Allow editing progress | `NumberInput` with Slider variant |
| `options="{'edit_max_value': true}"` | Allow editing max value | Separate NumberInput for max |
| `options="{'overflow_class': '...'}"` | Style when exceeded | `.Color("red")` when over 100% |
| `group_operator="avg"` | Average in grouped views | `.Average()` in LINQ aggregation |
| `string="..."` | Field label | `.WithField().Label("...")` |
| Bar color (auto) | Blue default, red on overflow | `.Color()` based on value |
