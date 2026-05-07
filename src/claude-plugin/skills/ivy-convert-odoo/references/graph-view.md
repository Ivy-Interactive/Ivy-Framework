# Graph View

Odoo's visualization view for displaying aggregated data as bar charts, line charts, or pie charts. Used for reporting and analytics dashboards.

## Odoo

```python
class SaleReport(models.Model):
    _name = 'sale.report'
    _auto = False

    date = fields.Date(string='Order Date', readonly=True)
    product_id = fields.Many2one('product.product', string='Product', readonly=True)
    partner_id = fields.Many2one('res.partner', string='Customer', readonly=True)
    categ_id = fields.Many2one('product.category', string='Product Category', readonly=True)
    price_total = fields.Float(string='Total', readonly=True)
    product_uom_qty = fields.Float(string='Qty Ordered', readonly=True)
    state = fields.Selection([
        ('draft', 'Draft'),
        ('sale', 'Sales Order'),
        ('done', 'Done'),
        ('cancel', 'Cancelled'),
    ], string='Status', readonly=True)
```

```xml
<!-- Bar chart (default) grouped by product category -->
<record id="view_sale_report_graph" model="ir.ui.view">
    <field name="name">sale.report.graph</field>
    <field name="model">sale.report</field>
    <field name="arch" type="xml">
        <graph string="Sales Analysis" type="bar" stacked="0">
            <field name="categ_id"/>
            <field name="price_total" type="measure"/>
        </graph>
    </field>
</record>

<!-- Line chart with date interval -->
<graph string="Sales Trend" type="line">
    <field name="date" interval="month"/>
    <field name="price_total" type="measure"/>
</graph>

<!-- Pie chart -->
<graph string="Sales by Status" type="pie">
    <field name="state"/>
    <field name="product_uom_qty" type="measure"/>
</graph>

<!-- Multiple grouping dimensions -->
<graph string="Sales by Category and Month" type="bar">
    <field name="categ_id"/>
    <field name="date" interval="month"/>
    <field name="price_total" type="measure"/>
</graph>
```

## Ivy

```csharp
// Bar chart → BarChart widget
var salesByCategory = UseQuery(() => db.SaleReports
    .GroupBy(s => s.CategoryName)
    .Select(g => new { Category = g.Key, Total = g.Sum(s => s.PriceTotal) })
    .ToList());

new BarChart(salesByCategory.Value)
    .XAxis(d => d.Category)
    .Bar(d => d.Total, "Total Sales")
    .Tooltip()
    .Legend();

// Line chart → LineChart widget
var salesTrend = UseQuery(() => db.SaleReports
    .GroupBy(s => new { s.Date.Year, s.Date.Month })
    .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:D2}", Total = g.Sum(s => s.PriceTotal) })
    .OrderBy(g => g.Month)
    .ToList());

new LineChart(salesTrend.Value)
    .XAxis(d => d.Month)
    .Line(d => d.Total, "Revenue")
    .CartesianGrid()
    .Tooltip()
    .Legend();

// Pie chart → PieChart widget
var salesByStatus = UseQuery(() => db.SaleReports
    .GroupBy(s => s.State)
    .Select(g => new { Status = g.Key, Qty = g.Sum(s => s.ProductUomQty) })
    .ToList());

new PieChart(salesByStatus.Value)
    .Pie(d => d.Qty, d => d.Status)
    .Tooltip()
    .Legend();

// Stacked bar chart
new BarChart(salesData.Value)
    .XAxis(d => d.Category)
    .Bar(d => d.Q1, "Q1")
    .Bar(d => d.Q2, "Q2")
    .Bar(d => d.Q3, "Q3")
    .StackOffset()
    .Tooltip()
    .Legend();
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<graph>` | Root graph view element | `BarChart`, `LineChart`, or `PieChart` widget |
| `type="bar"` | Bar chart (default) | `new BarChart(data)` |
| `type="line"` | Line chart | `new LineChart(data)` |
| `type="pie"` | Pie chart | `new PieChart(data)` |
| `stacked="0"` | Disable bar stacking | Omit `.StackOffset()` |
| `<field name="..." type="measure">` | Aggregation measure | `.Bar()`, `.Line()`, or `.Pie()` with value selector |
| `<field name="...">` (no type) | Grouping dimension | `.XAxis()` selector or `.Pie()` label |
| `interval="day/week/month/quarter/year"` | Date grouping interval | Group in LINQ query by date part |
| `order="asc/desc"` | X-axis sorting | `.OrderBy()` / `.OrderByDescending()` in LINQ |
| `disable_linking="1"` | Prevent click-through to list | No click handler needed |
| `string="..."` | View title / breadcrumb | Page heading or chart title |
| `sample="1"` | Show sample data when empty | Handle empty state in UI |
