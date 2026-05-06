# Field: Handle

Odoo's drag handle widget for reordering records in list views. Renders as a drag grip icon (≡) that enables drag-and-drop row reordering. Typically bound to a `sequence` integer field.

## Odoo

```python
class ProductCategory(models.Model):
    _name = 'product.category'
    _order = 'sequence, id'

    name = fields.Char(string='Name', required=True)
    sequence = fields.Integer(string='Sequence', default=10)

class SaleOrderLine(models.Model):
    _name = 'sale.order.line'

    sequence = fields.Integer(string='Sequence', default=10)
```

```xml
<!-- Handle widget in list view -->
<tree>
    <field name="sequence" widget="handle"/>
    <field name="name"/>
    <field name="active"/>
</tree>

<!-- In One2many inline list -->
<field name="order_line">
    <tree editable="bottom">
        <field name="sequence" widget="handle"/>
        <field name="product_id"/>
        <field name="product_uom_qty"/>
        <field name="price_unit"/>
    </tree>
</field>
```

## Ivy

```csharp
// Handle widget → DataTable with reordering support
// The sequence field controls ordering; drag-drop updates sequence values

var categories = UseQuery(() => db.Categories
    .OrderBy(c => c.Sequence)
    .ToList());

categories.ToDataTable()
    .Header(c => c.Name, "Name")
    .Header(c => c.Active, "Active")
    .Config(c => c.Sortable(true));

// For One2many with reordering
var orderLines = UseState(new List<OrderLineState>());

new Button("Add Line", onClick: async e => {
    orderLines.Value.Add(new OrderLineState());
}).Ghost().Icon(Icons.Plus);

orderLines.ToDataTable()
    .Header(l => l.ProductName, "Product")
    .Header(l => l.Quantity, "Qty")
    .Header(l => l.UnitPrice, "Unit Price")
    .RowActions(
        new MenuItem("Up", Icons.ChevronUp),
        new MenuItem("Down", Icons.ChevronDown),
        new MenuItem("Delete", Icons.Trash))
    .OnRowAction(async e => {
        // Handle reorder or delete based on e.Value.Action
    });
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="handle"` | Drag handle for reordering | DataTable row reordering or manual up/down actions |
| `fields.Integer` (sequence) | Ordering field | `Sequence` property on list items |
| Drag-and-drop | Visual drag reordering | DataTable drag support or action buttons |
| `_order = 'sequence, id'` | Default model ordering | `.OrderBy(x => x.Sequence)` in LINQ |
| Column width (20px) | Narrow handle column | Compact action column |
| `default=10` | Default sequence value | Initial sequence in new items |
