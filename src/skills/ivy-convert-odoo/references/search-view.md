# Search View

Odoo's search view defines filters, search fields, and group-by options that control how records are filtered in other views (list, kanban, graph, etc.). It appears as a search bar with filter dropdowns and grouping menus.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    name = fields.Char(string='Order Reference', required=True)
    partner_id = fields.Many2one('res.partner', string='Customer')
    user_id = fields.Many2one('res.users', string='Salesperson')
    date_order = fields.Datetime(string='Order Date', default=fields.Datetime.now)
    state = fields.Selection([
        ('draft', 'Quotation'),
        ('sent', 'Quotation Sent'),
        ('sale', 'Sales Order'),
        ('done', 'Locked'),
        ('cancel', 'Cancelled'),
    ], string='Status', default='draft')
    amount_total = fields.Monetary(string='Total')
    tag_ids = fields.Many2many('crm.tag', string='Tags')
```

```xml
<record id="view_sale_order_search" model="ir.ui.view">
    <field name="name">sale.order.search</field>
    <field name="model">sale.order</field>
    <field name="arch" type="xml">
        <search string="Search Sales Orders">
            <!-- Search fields -->
            <field name="name" string="Order"
                   filter_domain="['|', ('name', 'ilike', self), ('client_order_ref', 'ilike', self)]"/>
            <field name="partner_id" operator="child_of"/>
            <field name="user_id"/>
            <field name="tag_ids"/>

            <!-- Pre-defined filters -->
            <filter name="my_orders" string="My Orders"
                    domain="[('user_id', '=', uid)]"/>
            <filter name="draft" string="Quotations"
                    domain="[('state', 'in', ['draft', 'sent'])]"/>
            <separator/>
            <filter name="this_month" string="This Month"
                    date="date_order" default_period="this_month"/>
            <filter name="last_quarter" string="Last Quarter"
                    date="date_order" default_period="last_quarter"/>

            <!-- Group by -->
            <group expand="0" string="Group By">
                <filter name="group_customer" string="Customer"
                        context="{'group_by': 'partner_id'}"/>
                <filter name="group_salesperson" string="Salesperson"
                        context="{'group_by': 'user_id'}"/>
                <filter name="group_month" string="Order Month"
                        context="{'group_by': 'date_order:month'}"/>
            </group>

            <!-- Search panel (left sidebar) -->
            <searchpanel>
                <field name="state" icon="fa-tasks" enable_counters="1"/>
                <field name="user_id" select="multi" icon="fa-user" enable_counters="1"/>
            </searchpanel>
        </search>
    </field>
</record>
```

## Ivy

```csharp
// Search views map to filter/search UI above a data view
// Combine TextInput search, SelectInput filters, and DateRangeInput

var searchTerm = UseState("");
var statusFilter = UseState<string?>(null);
var salespersonFilter = UseState<int?>(null);
var dateRange = UseState<(DateOnly?, DateOnly?)>((null, null));

// Search bar
searchTerm.ToTextInput()
    .Variant(TextInputVariant.Search)
    .Placeholder("Search orders...");

// Filter controls in horizontal layout
PlaceHorizontal(() =>
{
    statusFilter.ToSelectInput(new[] {
        new Option("draft", "Quotations"),
        new Option("sale", "Sales Orders"),
        new Option("done", "Locked"),
        new Option("cancel", "Cancelled"),
    }).Placeholder("All Statuses").Nullable();

    salespersonFilter.ToSelectInput(
        users.Value.ToOptions(u => u.Id, u => u.Name)
    ).Placeholder("All Salespersons").Nullable().Searchable();

    dateRange.ToDateRangeInput()
        .StartPlaceholder("From")
        .EndPlaceholder("To");
});

// Apply filters to query
var orders = UseQuery(() => db.SaleOrders
    .Where(o => string.IsNullOrEmpty(searchTerm.Value)
        || o.Name.Contains(searchTerm.Value)
        || o.ClientOrderRef.Contains(searchTerm.Value))
    .Where(o => statusFilter.Value == null || o.State == statusFilter.Value)
    .Where(o => salespersonFilter.Value == null || o.UserId == salespersonFilter.Value)
    .Where(o => dateRange.Value.Item1 == null || o.DateOrder >= dateRange.Value.Item1)
    .Where(o => dateRange.Value.Item2 == null || o.DateOrder <= dateRange.Value.Item2)
    .ToList());

// Display results in DataTable
orders.ToDataTable()
    .Header(o => o.Name, "Order")
    .Header(o => o.PartnerName, "Customer")
    .Header(o => o.UserName, "Salesperson")
    .Header(o => o.DateOrder, "Date")
    .Header(o => o.AmountTotal, "Total")
    .Config(c => c.Sortable(true).Filterable(true).Groupable(true));

// Search panel (sidebar filters) → sidebar layout with filter controls
// Use SidebarLayout for a left-side filter panel
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<search>` | Root search view element | Combination of search/filter widgets above data view |
| `<field name="...">` | Searchable field | `TextInput` with `.Variant(TextInputVariant.Search)` |
| `filter_domain` | Custom search domain | Custom `.Where()` logic in LINQ query |
| `operator="child_of"` | Hierarchical search | Include children in `.Where()` query |
| `<filter domain="...">` | Pre-defined filter | `SelectInput` option or `BoolInput` toggle |
| `<filter date="...">` | Date-based filter | `DateRangeInput` with presets |
| `default_period` | Default date period | Initial `UseState` value for date range |
| `<separator/>` | Visual separator between filters | `Separator` widget or layout spacing |
| `<group string="Group By">` | Grouping options | `.Config(c => c.Groupable(true))` on DataTable |
| `context="{'group_by': '...'}` | Group by field | `.GroupBy()` in LINQ or DataTable grouping |
| `<searchpanel>` | Left sidebar filter panel | `SidebarLayout` with filter widgets |
| `enable_counters="1"` | Show record counts on filters | Badge counts next to filter options |
| `select="multi"` | Multi-select filter panel | `SelectInput` with collection state |
| `icon="fa-*"` | Filter icon | `.Icon()` on filter widget |
