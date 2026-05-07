# Gantt View

Odoo's Gantt chart view for scheduling and project management (Enterprise feature). Displays records as horizontal bars on a timeline, grouped by resource. Supports drag-and-drop rescheduling, dependencies, and progress indicators.

## Odoo

```python
class ProjectTask(models.Model):
    _name = 'project.task'

    name = fields.Char(string='Task Title', required=True)
    project_id = fields.Many2one('project.project', string='Project')
    user_ids = fields.Many2many('res.users', string='Assignees')
    date_start = fields.Datetime(string='Start Date')
    date_end = fields.Datetime(string='End Date')
    progress = fields.Float(string='Progress', compute='_compute_progress')
    stage_id = fields.Many2one('project.task.type', string='Stage')
    depend_on_ids = fields.Many2many('project.task', 'task_dependencies_rel',
                                      'task_id', 'depends_on_id', string='Dependencies')
    dependent_ids = fields.Many2many('project.task', 'task_dependencies_rel',
                                      'depends_on_id', 'task_id', string='Dependents')
    planned_hours = fields.Float(string='Planned Hours')
```

```xml
<record id="view_task_gantt" model="ir.ui.view">
    <field name="name">project.task.gantt</field>
    <field name="model">project.task</field>
    <field name="arch" type="xml">
        <gantt string="Planning"
               date_start="date_start"
               date_stop="date_end"
               default_group_by="user_ids"
               default_scale="week"
               scales="day,week,month,year"
               color="project_id"
               progress="progress"
               dependency_field="depend_on_ids"
               dependency_inverted_field="dependent_ids"
               precision="{'day': 'hour:half', 'week': 'day:half', 'month': 'day'}"
               total_row="1"
               thumbnails="{'user_ids': 'avatar_128'}"
               decoration-danger="date_end &lt; today and progress &lt; 100"
               decoration-warning="progress == 0"
               decoration-success="progress == 100"
               pill_label="1"
               sample="1">
            <templates>
                <div t-name="gantt-popover">
                    <div><strong><t t-esc="name"/></strong></div>
                    <div>Project: <t t-esc="project_id[1]"/></div>
                    <div>Progress: <t t-esc="progress"/>%</div>
                    <div>Planned: <t t-esc="planned_hours"/>h</div>
                </div>
            </templates>
        </gantt>
    </field>
</record>
```

## Ivy

```csharp
// Gantt charts map to a timeline/scheduling layout
// Option 1: Use DataTable with date-based visualization
var tasks = UseQuery(() => db.Tasks
    .Include(t => t.Project)
    .Include(t => t.Users)
    .OrderBy(t => t.DateStart)
    .ToList());

// Display as a structured task list with progress
tasks.ToDataTable()
    .Header(t => t.Name, "Task")
    .Header(t => t.ProjectName, "Project")
    .Header(t => t.UserNames, "Assignees")
    .Header(t => t.DateStart, "Start", format: v => v?.ToString("MMM dd"))
    .Header(t => t.DateEnd, "End", format: v => v?.ToString("MMM dd"))
    .Header(t => t.Progress, "Progress", format: v =>
        new Progress((int)v).Color(v >= 100 ? "green" : v > 0 ? "blue" : "gray"))
    .Config(c => c.Sortable(true).Groupable(true));

// Option 2: Custom timeline visualization with cards
var groupedByUser = tasks.Value.GroupBy(t => t.UserName);
foreach (var group in groupedByUser)
{
    new TextBlock(group.Key).Variant(TextBlockVariants.H4);
    foreach (var task in group.OrderBy(t => t.DateStart))
    {
        Card(() =>
        {
            PlaceHorizontal(() =>
            {
                new TextBlock(task.Name).Bold();
                new Badge(task.ProjectName);
                new Progress((int)task.Progress);
            });
            new TextBlock($"{task.DateStart:MMM dd} → {task.DateEnd:MMM dd}").Muted();
        });
    }
}

// Option 3: BarChart as horizontal timeline approximation
new BarChart(tasks.Value)
    .Horizontal()
    .XAxis(t => t.Name)
    .Bar(t => (t.DateEnd - t.DateStart).TotalDays, "Duration (days)")
    .Tooltip();
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<gantt>` | Root Gantt view element | DataTable with date columns or custom timeline layout |
| `date_start` | Task start date field | Date column or timeline start |
| `date_stop` | Task end date field | Date column or timeline end |
| `default_group_by` | Group tasks by field | `.GroupBy()` in LINQ or DataTable grouping |
| `default_scale` | Default time scale (day/week/month/year) | Layout granularity |
| `scales` | Available time scales | View toggle options |
| `color` | Color pills by field | Badge or card color based on field |
| `progress` | Completion percentage field | `Progress` widget (0-100) |
| `dependency_field` | Task dependency links | Visual dependency lines (custom implementation) |
| `precision` | Snap precision per scale | Not applicable (server-side scheduling) |
| `total_row="1"` | Show total count row | Footer row in DataTable |
| `decoration-danger/warning/success` | Conditional pill colors | Conditional styling via Badge or row class |
| `thumbnails` | User avatar thumbnails | `Avatar` widget per user |
| `pill_label="1"` | Show time in pill | Time display in card/cell |
| `<templates><gantt-popover>` | Hover popover template | `Tooltip` or Card details on hover |
| `cell_create` | Allow creating from empty cells | Add button or click handler |
| `disable_drag_drop` | Prevent rescheduling by drag | Read-only layout |
| `sample="1"` | Show sample data when empty | Skeleton or placeholder content |
