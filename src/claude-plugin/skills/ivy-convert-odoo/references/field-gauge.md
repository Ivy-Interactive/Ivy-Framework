# Field: Gauge

Odoo's gauge/doughnut chart widget for displaying a value relative to a maximum. Renders as a semi-circular gauge chart using Chart.js. Used in dashboards and reports for KPI visualization.

## Odoo

```python
class FleetVehicle(models.Model):
    _name = 'fleet.vehicle'

    odometer = fields.Float(string='Last Odometer')
    fuel_level = fields.Float(string='Fuel Level')
    next_service_km = fields.Float(string='Next Service (km)')

class ProjectProject(models.Model):
    _name = 'project.project'

    progress = fields.Float(string='Progress', compute='_compute_progress')
    task_count = fields.Integer(string='Tasks')
    task_count_done = fields.Integer(string='Completed Tasks')
```

```xml
<!-- Basic gauge -->
<field name="fuel_level" widget="gauge"
       options="{'max_value': 100, 'title': 'Fuel Level'}"/>

<!-- Gauge with max from field -->
<field name="task_count_done" widget="gauge"
       options="{'max_field': 'task_count', 'title': 'Task Completion'}"/>

<!-- Gauge with static max -->
<field name="odometer" widget="gauge"
       options="{'max_value': 200000, 'title': 'Odometer'}"/>
```

## Ivy

```csharp
// Gauge → PieChart (doughnut) or Progress widget
// Option 1: Progress bar for simple gauge
new Progress((int)fuelLevel)
    .Goal(100)
    .Color(fuelLevel > 50 ? "green" : fuelLevel > 25 ? "orange" : "red");
new TextBlock($"Fuel Level: {fuelLevel:F0}%").Muted();

// Option 2: PieChart as doughnut gauge
new PieChart(new[] {
    new { Label = "Completed", Value = taskCountDone },
    new { Label = "Remaining", Value = taskCount - taskCountDone },
})
    .Pie(d => d.Value, d => d.Label)
    .Total($"{taskCountDone}/{taskCount}")
    .ColorScheme(new[] { "green", "lightgray" })
    .Tooltip();

// Option 3: Card with metric display
Card(() =>
{
    new TextBlock("Task Completion").Variant(TextBlockVariants.H4);
    new Progress((int)((double)taskCountDone / taskCount * 100))
        .Color("green");
    new TextBlock($"{taskCountDone} of {taskCount} tasks").Muted();
});

// Dashboard with multiple gauges
PlaceHorizontal(() =>
{
    Card(() =>
    {
        new TextBlock("Fuel Level").Variant(TextBlockVariants.Muted);
        new Progress((int)fuelLevel).Goal(100).Color("blue");
        new TextBlock($"{fuelLevel:F0}%").Variant(TextBlockVariants.H3);
    });

    Card(() =>
    {
        new TextBlock("Odometer").Variant(TextBlockVariants.Muted);
        new Progress((int)(odometer / 200000 * 100)).Goal(100);
        new TextBlock($"{odometer:N0} km").Variant(TextBlockVariants.H3);
    });
});
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="gauge"` | Gauge/doughnut chart | `Progress` widget or `PieChart` (doughnut) |
| `options="{'max_value': N}"` | Static max value | `.Goal(N)` on Progress |
| `options="{'max_field': '...'}"` | Max value from field | `.Goal(fieldValue)` |
| `options="{'title': '...'}"` | Gauge title | `TextBlock` heading above widget |
| Value display | Current value shown | `TextBlock` with formatted value |
| Ratio visualization | Fill based on value/max | Progress percentage computation |
| Chart.js doughnut | Semi-circular chart | `PieChart` or `Progress` widget |
| Human-readable values | Formatted large numbers | Custom number formatting |
