# Pivot View

Odoo's pivot table view for multi-dimensional data analysis. Displays aggregated values in rows and columns with drill-down capabilities, similar to Excel pivot tables.

## Odoo

```python
class TimesheetReport(models.Model):
    _name = 'hr.timesheet.report'
    _auto = False

    employee_id = fields.Many2one('hr.employee', string='Employee', readonly=True)
    project_id = fields.Many2one('project.project', string='Project', readonly=True)
    task_id = fields.Many2one('project.task', string='Task', readonly=True)
    date = fields.Date(string='Date', readonly=True)
    unit_amount = fields.Float(string='Hours Spent', readonly=True)
    amount = fields.Monetary(string='Amount', readonly=True)
    department_id = fields.Many2one('hr.department', string='Department', readonly=True)
```

```xml
<!-- Basic pivot with rows, columns, and measures -->
<record id="view_timesheet_pivot" model="ir.ui.view">
    <field name="name">hr.timesheet.pivot</field>
    <field name="model">hr.timesheet.report</field>
    <field name="arch" type="xml">
        <pivot string="Timesheet Analysis" display_quantity="1">
            <field name="employee_id" type="row"/>
            <field name="date" interval="month" type="col"/>
            <field name="unit_amount" type="measure" widget="float_time"/>
        </pivot>
    </field>
</record>

<!-- Multiple row and column dimensions -->
<pivot string="Sales Analysis" default_order="price_total desc">
    <field name="categ_id" type="row"/>
    <field name="partner_id" type="row"/>
    <field name="date" interval="quarter" type="col"/>
    <field name="price_total" type="measure"/>
    <field name="product_uom_qty" type="measure"/>
</pivot>
```

## Ivy

```csharp
// Pivot tables map to DataTable with grouped/aggregated data
// Option 1: Pre-aggregate in query and display as DataTable
var timesheetPivot = UseQuery(() => db.Timesheets
    .GroupBy(t => new { t.EmployeeName, Month = t.Date.ToString("yyyy-MM") })
    .Select(g => new {
        Employee = g.Key.EmployeeName,
        Month = g.Key.Month,
        Hours = g.Sum(t => t.UnitAmount)
    })
    .ToList());

timesheetPivot.ToDataTable()
    .Header(r => r.Employee, "Employee")
    .Header(r => r.Month, "Month")
    .Header(r => r.Hours, "Hours Spent")
    .Config(c => c.Sortable(true).Groupable(true));

// Summary below table
new TextBlock($"Total: {timesheetPivot.Value.Sum(r => r.Hours):F1}h").Bold();

// Option 2: Cross-tab style with dynamic columns
var employees = timesheetData.Select(t => t.Employee).Distinct();
var months = timesheetData.Select(t => t.Month).Distinct().OrderBy(m => m);

var table = new Table();
var headerRow = new TableRow();
headerRow.Add(new TableCell("Employee"));
foreach (var month in months)
    headerRow.Add(new TableCell(month));
table.Add(headerRow);

foreach (var emp in employees)
{
    var row = new TableRow();
    row.Add(new TableCell(emp));
    foreach (var month in months)
    {
        var hours = timesheetData
            .FirstOrDefault(t => t.Employee == emp && t.Month == month)?.Hours ?? 0;
        row.Add(new TableCell($"{hours:F1}"));
    }
    table.Add(row);
}

// Option 3: Use BarChart for visual pivot representation
new BarChart(timesheetPivot.Value)
    .XAxis(d => d.Employee)
    .Bar(d => d.Hours, "Hours")
    .Tooltip()
    .Legend();
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<pivot>` | Root pivot view element | `DataTable` with grouped/aggregated query data |
| `<field type="row">` | Row grouping dimension | Group in LINQ query, display as row field |
| `<field type="col">` | Column grouping dimension | Dynamic columns or cross-tab layout |
| `<field type="measure">` | Aggregated value | `.Sum()`, `.Average()` etc. in LINQ |
| `interval="month/quarter/year"` | Date grouping interval | Date part extraction in LINQ GroupBy |
| `display_quantity="1"` | Show record count column | Add count column to query |
| `default_order="field desc"` | Default sort | `.OrderByDescending()` in LINQ or `.Config(c => c.DefaultSort(...))` |
| `disable_linking="1"` | Prevent click to list view | No click handler |
| `widget="float_time"` | Format as hours:minutes | Custom format string `$"{hours:F1}h"` |
| `widget="monetary"` | Format as currency | `.ToString("C")` or NumberInput currency format |
| `string="..."` | View title | Page heading |
| drill-down (expand/collapse) | Hierarchical exploration | Nested grouping in UI or collapsible rows |
