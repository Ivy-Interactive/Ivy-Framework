# List View (Tree View)

Odoo's primary view for displaying multiple records in a tabular format. Supports inline editing, row selection, grouping, and column sorting.

## Odoo

```xml
<record id="view_partner_tree" model="ir.ui.view">
    <field name="name">res.partner.tree</field>
    <field name="model">res.partner</field>
    <field name="arch" type="xml">
        <tree string="Partners" editable="bottom" decoration-danger="state == 'cancelled'"
              decoration-success="state == 'done'" default_order="name asc">
            <field name="name"/>
            <field name="email"/>
            <field name="phone"/>
            <field name="company_id"/>
            <field name="state" widget="badge"
                   decoration-success="state == 'done'"
                   decoration-info="state == 'draft'"/>
            <field name="amount_total" sum="Total Amount"/>
            <button name="action_confirm" type="object" string="Confirm"
                    icon="fa-check" attrs="{'invisible': [('state', '!=', 'draft')]}"/>
        </tree>
    </field>
</record>
```

```python
class ResPartner(models.Model):
    _name = 'res.partner'

    name = fields.Char(string='Name', required=True)
    email = fields.Char(string='Email')
    phone = fields.Char(string='Phone')
    company_id = fields.Many2one('res.company', string='Company')
    state = fields.Selection([
        ('draft', 'Draft'),
        ('confirmed', 'Confirmed'),
        ('done', 'Done'),
        ('cancelled', 'Cancelled'),
    ], default='draft')
    amount_total = fields.Float(string='Total Amount')
```

## Ivy

```csharp
var partners = UseQuery(() => db.Partners.OrderBy(p => p.Name).ToList());

partners.ToDataTable()
    .Header(p => p.Name, "Name")
    .Header(p => p.Email, "Email")
    .Header(p => p.Phone, "Phone")
    .Header(p => p.CompanyName, "Company")
    .Header(p => p.State, "Status", format: v => new Badge(v.ToString()))
    .Header(p => p.AmountTotal, "Total Amount")
    .OnCellClick(async e => Navigate($"/partner/{e.Value.RowId}"))
    .Config(c => c.Sortable(true).Filterable(true));

// Summary below table
new TextBlock($"Total: {partners.Value.Sum(p => p.AmountTotal):C}").Bold();

// Per-row action menu
partners.ToDataTable()
    .RowActions(new MenuItem("Confirm", Icons.Check))
    .OnRowAction(async e => {
        await api.ConfirmPartner(e.Value.RowId);
    });
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<tree>` | Root list view element | `.ToDataTable()` on `IEnumerable<T>` |
| `<field name="...">` | Column definition | `.Header(x => x.Prop, "Label")` |
| `string="..."` | Column/view label | Second param of `.Header()` |
| `editable="bottom/top"` | Inline row editing | Use Dialog or Form for row editing |
| `decoration-*` | Row conditional styling | `.RowClass(row => condition)` or Badge formatting |
| `default_order` | Default sort column | `.OrderBy()` in LINQ query or `.Config(c => c.DefaultSort(...))` |
| `sum="..."` | Column aggregate in footer | Computed summary `TextBlock` below table |
| `widget="badge"` | Badge display for field | Format with `new Badge(...)` in `.Header()` |
| `<button>` in tree | Per-row action button | `.RowActions(new MenuItem(...))` + `.OnRowAction()` |
| `attrs="{'invisible':...}"` | Conditional visibility | Conditional logic in `.OnRowAction()` handler |
| `limit` attribute | Records per page | `.Config(c => c.PageSize(n))` |
| groupby support | Group rows by field | `.Config(c => c.Groupable(true))` |
