# Cohort View

Odoo's cohort view for analyzing data changes over time, particularly churn and retention rates (Enterprise feature). Displays a matrix where rows represent cohorts (records grouped by start date) and columns show time periods, with each cell indicating the retention or churn percentage.

## Odoo

```python
class SaleSubscription(models.Model):
    _name = 'sale.subscription'

    name = fields.Char(string='Subscription', required=True)
    partner_id = fields.Many2one('res.partner', string='Customer')
    date_start = fields.Date(string='Start Date', required=True)
    date = fields.Date(string='End Date')
    recurring_monthly = fields.Monetary(string='Monthly Recurring Revenue')
    stage_id = fields.Many2one('sale.subscription.stage', string='Stage')
    close_reason_id = fields.Many2one('sale.subscription.close.reason', string='Close Reason')
```

```xml
<record id="view_subscription_cohort" model="ir.ui.view">
    <field name="name">sale.subscription.cohort</field>
    <field name="model">sale.subscription</field>
    <field name="arch" type="xml">
        <cohort string="Subscription Retention"
                date_start="date_start"
                date_stop="date"
                interval="month"
                mode="retention"
                measure="recurring_monthly"
                sample="1"/>
    </field>
</record>

<!-- Churn analysis -->
<cohort string="Customer Churn"
        date_start="date_start"
        date_stop="date"
        interval="week"
        mode="churn"
        timeline="forward"/>

<!-- With additional field measures -->
<cohort string="Revenue Retention"
        date_start="date_start"
        date_stop="date"
        interval="month"
        mode="retention">
    <field name="recurring_monthly" string="MRR" invisible="0"/>
</cohort>
```

## Ivy

```csharp
// Cohort views map to a custom retention/churn analysis table
// Pre-compute cohort data and display in a grid

var subscriptions = UseQuery(() => db.Subscriptions
    .Select(s => new { s.DateStart, s.DateEnd, s.RecurringMonthly })
    .ToList());

// Compute cohort data
var cohorts = subscriptions.Value
    .GroupBy(s => new { s.DateStart.Year, s.DateStart.Month })
    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
    .Select(g => {
        var cohortStart = new DateOnly(g.Key.Year, g.Key.Month, 1);
        var total = g.Count();
        var periods = Enumerable.Range(0, 12).Select(i => {
            var periodEnd = cohortStart.AddMonths(i + 1);
            var retained = g.Count(s => s.DateEnd == null || s.DateEnd >= periodEnd);
            return new { Month = i, Retention = total > 0 ? (double)retained / total * 100 : 0 };
        }).ToList();
        return new { CohortMonth = cohortStart.ToString("MMM yyyy"), Total = total, Periods = periods };
    })
    .ToList();

// Display as a Table
var table = new Table();
var headerRow = new TableRow();
headerRow.Add(new TableCell("Cohort"));
headerRow.Add(new TableCell("Count"));
for (int i = 0; i < 12; i++)
    headerRow.Add(new TableCell($"Month {i + 1}"));
table.Add(headerRow);

foreach (var cohort in cohorts)
{
    var row = new TableRow();
    row.Add(new TableCell(cohort.CohortMonth));
    row.Add(new TableCell(cohort.Total.ToString()));
    foreach (var period in cohort.Periods)
    {
        var badge = new Badge($"{period.Retention:F0}%")
            .Variant(period.Retention >= 80 ? BadgeVariants.Success
                : period.Retention >= 50 ? BadgeVariants.Warning
                : BadgeVariants.Destructive);
        row.Add(new TableCell(badge));
    }
    table.Add(row);
}

// Alternative: Heatmap-style using BarChart per cohort
new BarChart(cohorts.SelectMany(c =>
    c.Periods.Select(p => new { c.CohortMonth, p.Month, p.Retention })))
    .XAxis(d => $"Month {d.Month}")
    .Bar(d => d.Retention, "Retention %")
    .Tooltip()
    .Legend();
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<cohort>` | Root cohort view element | Custom Table with computed retention/churn data |
| `date_start` | Cohort start date field | GroupBy key for cohort bucketing |
| `date_stop` | End/churn date field | Used to compute retention per period |
| `interval="day/week/month/year"` | Time period granularity | Period computation interval |
| `mode="retention"` | Retention mode (starts 100%, decreases) | `retained / total * 100` per period |
| `mode="churn"` | Churn mode (starts 0%, increases) | `(total - retained) / total * 100` per period |
| `timeline="forward/backward"` | Direction of timeline | Period iteration direction |
| `measure` | Field to aggregate (default: count) | `.Sum()` of measure field per period |
| `disable_linking="1"` | Prevent click to list | No click handler on cells |
| `<field invisible="0">` | Additional visible measures | Extra columns in cohort table |
| `widget="..."` | Field formatter | Custom `.ToString()` formatting |
| `sample="1"` | Show sample data when empty | Skeleton placeholder |
| `string="..."` | View title | Page heading |
