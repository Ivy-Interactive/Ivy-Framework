# Field: Many2one / Many2many / One2many

Odoo's relational fields for linking records between models. `Many2one` is a foreign key, `Many2many` is a join table, and `One2many` is the reverse of Many2one.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    partner_id = fields.Many2one('res.partner', string='Customer', required=True,
                                  domain="[('customer_rank', '>', 0)]")
    company_id = fields.Many2one('res.company', string='Company', default=lambda self: self.env.company)
    tag_ids = fields.Many2many('crm.tag', string='Tags')
    order_line = fields.One2many('sale.order.line', 'order_id', string='Order Lines')
    user_id = fields.Many2one('res.users', string='Salesperson',
                               default=lambda self: self.env.user)
```

```xml
<!-- Many2one - dropdown with search -->
<field name="partner_id" options="{'no_create': True, 'no_open': True}"/>

<!-- Many2many - tag chips -->
<field name="tag_ids" widget="many2many_tags" options="{'color_field': 'color'}"/>

<!-- Many2many - checkboxes -->
<field name="tag_ids" widget="many2many_checkboxes"/>

<!-- One2many - inline list -->
<field name="order_line">
    <tree editable="bottom">
        <field name="product_id"/>
        <field name="product_uom_qty"/>
        <field name="price_unit"/>
        <field name="price_subtotal"/>
    </tree>
</field>

<!-- Many2one avatar widgets -->
<field name="user_id" widget="many2one_avatar_user"/>
```

## Ivy

```csharp
// Many2one → SelectInput with options from query
var partners = UseQuery(() => db.Partners.Where(p => p.CustomerRank > 0)
    .Select(p => new { p.Id, p.Name }).ToList());
var partnerId = UseState<int?>(null);

partnerId.ToSelectInput(partners.Value.ToOptions(p => p.Id, p => p.Name))
    .Placeholder("Select customer...")
    .Searchable()
    .WithField()
    .Label("Customer")
    .Required();

// Many2many → SelectInput with collection state (auto multi-select)
var allTags = UseQuery(() => db.Tags.ToList());
var selectedTagIds = UseState<int[]>([]);

selectedTagIds.ToSelectInput(allTags.Value.ToOptions(t => t.Id, t => t.Name))
    .WithField()
    .Label("Tags");

// Many2many as list variant
selectedTagIds.ToSelectInput(allTags.Value.ToOptions(t => t.Id, t => t.Name))
    .Variant(SelectInputVariants.List)
    .WithField()
    .Label("Tags");

// One2many → DataTable with add/edit via buttons and dialogs
var orderLines = UseState(new List<OrderLineState>());

new Button("Add Line", onClick: async e => {
    orderLines.Value.Add(new OrderLineState());
}).Ghost().Icon(Icons.Plus);

orderLines.ToDataTable()
    .Header(l => l.ProductName, "Product")
    .Header(l => l.Quantity, "Qty")
    .Header(l => l.UnitPrice, "Unit Price")
    .Header(l => l.Subtotal, "Subtotal")
    .RowActions(new MenuItem("Edit", Icons.Pencil), new MenuItem("Delete", Icons.Trash))
    .OnRowAction(async e => { /* handle edit/delete */ });

// Many2one avatar → SelectInput or Avatar display
var userId = UseState<int?>(currentUser.Id);
userId.ToSelectInput(users.Value.ToOptions(u => u.Id, u => u.Name))
    .WithField()
    .Label("Salesperson");
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Many2one` | Foreign key dropdown | `SelectInput` with options from query |
| `fields.Many2many` | Multi-select relation | `SelectInput` with collection state (auto multi-select) |
| `fields.One2many` | Reverse relation (inline list) | `DataTable` with add/edit via buttons |
| `domain="[...]"` | Filter available options | Filter in `UseQuery` LINQ |
| `widget="many2many_tags"` | Tag chip display | `SelectInput` with collection state (default tag display) |
| `widget="many2many_checkboxes"` | Checkbox group | `SelectInput` with `.Variant(SelectInputVariants.List)` |
| `widget="many2one_avatar_user"` | User avatar + name | `SelectInput` or custom `Avatar` display |
| `options="{'no_create': True}"` | Disable create from dropdown | Default behavior (no inline create) |
| `options="{'no_open': True}"` | Disable link to record | Default behavior (no navigation) |
| `options="{'color_field': 'color'}"` | Colored tags | Badge/tag with color mapping |
| `required=True` | Mandatory selection | `.Required()` or `[Required]` annotation |
| `default=lambda...` | Default value | Initial `UseState` value |
| Inline `<tree editable>` | Editable sub-table | DataTable with row actions + Dialog for editing |
| `context="{'default_*': ...}"` | Default values for new records | Pass defaults in add handler |
