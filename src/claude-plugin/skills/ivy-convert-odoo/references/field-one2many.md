# Field: One2many

Odoo's one-to-many relational field for displaying and editing child records inline. The reverse side of a Many2one relationship. Typically renders as an embedded list/tree view within a form, supporting inline editing, row addition, and deletion.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    order_line = fields.One2many('sale.order.line', 'order_id', string='Order Lines',
                                  copy=True)

class SaleOrderLine(models.Model):
    _name = 'sale.order.line'

    order_id = fields.Many2one('sale.order', string='Order', required=True, ondelete='cascade')
    sequence = fields.Integer(string='Sequence', default=10)
    product_id = fields.Many2one('product.product', string='Product', required=True)
    name = fields.Text(string='Description')
    product_uom_qty = fields.Float(string='Quantity', default=1.0)
    price_unit = fields.Float(string='Unit Price')
    discount = fields.Float(string='Discount (%)')
    tax_ids = fields.Many2many('account.tax', string='Taxes')
    price_subtotal = fields.Monetary(string='Subtotal', compute='_compute_amount')
```

```xml
<!-- Editable inline tree -->
<field name="order_line">
    <tree editable="bottom" decoration-info="state == 'draft'">
        <control>
            <create name="add_product" string="Add a product"/>
            <create name="add_section" string="Add a section"
                    context="{'default_display_type': 'line_section'}"/>
            <create name="add_note" string="Add a note"
                    context="{'default_display_type': 'line_note'}"/>
        </control>
        <field name="sequence" widget="handle"/>
        <field name="product_id"/>
        <field name="name"/>
        <field name="product_uom_qty"/>
        <field name="price_unit"/>
        <field name="discount" optional="hide"/>
        <field name="tax_ids" widget="many2many_tags"/>
        <field name="price_subtotal" sum="Total"/>
    </tree>
</field>

<!-- Read-only list -->
<field name="invoice_ids" readonly="1">
    <tree>
        <field name="name"/>
        <field name="date"/>
        <field name="amount_total"/>
        <field name="state" widget="badge"/>
    </tree>
</field>

<!-- With form view for inline editing -->
<field name="order_line">
    <tree><field name="product_id"/><field name="product_uom_qty"/></tree>
    <form>
        <group>
            <field name="product_id"/>
            <field name="product_uom_qty"/>
            <field name="price_unit"/>
            <field name="discount"/>
        </group>
    </form>
</field>
```

## Ivy

```csharp
// One2many → editable DataTable
var orderLines = UseState(new List<OrderLineState>());

orderLines.ToDataTable()
    .Header(l => l.ProductName, "Product")
    .Header(l => l.Description, "Description")
    .Header(l => l.Quantity, "Qty")
    .Header(l => l.UnitPrice, "Unit Price", format: v => $"{v:F2}")
    .Header(l => l.Discount, "Discount %", format: v => $"{v:F1}%")
    .Header(l => l.Subtotal, "Subtotal", format: v => $"{v:C}")
    .RowActions(new MenuItem("Edit", Icons.Pencil), new MenuItem("Delete", Icons.Trash))
    .OnRowAction(async e => {
        // Handle edit via dialog or delete
    });

// Add button above table
new Button("Add Line", onClick: async e => {
    orderLines.Value.Add(new OrderLineState());
}).Ghost().Icon(Icons.Plus);

// Summary below table
new TextBlock($"Total: {orderLines.Value.Sum(l => l.Subtotal):C}").Bold();

// Read-only list of related records
var invoices = UseQuery(() => db.Invoices
    .Where(i => i.OrderId == orderId)
    .ToList());

invoices.ToDataTable()
    .Header(i => i.Name, "Invoice")
    .Header(i => i.Date, "Date", format: v => v.ToString("MMM dd, yyyy"))
    .Header(i => i.AmountTotal, "Total", format: v => $"{v:C}")
    .Header(i => i.State, "Status", format: v => new Badge(v))
    .OnCellClick(async e => Navigate($"/invoice/{e.Value.RowId}"));

// With custom add buttons (sections, notes)
PlaceHorizontal(() =>
{
    new Button("Add a product", onClick: async e => {
        orderLines.Value.Add(new OrderLineState { DisplayType = "product" });
    }).Ghost();
    new Button("Add a section", onClick: async e => {
        orderLines.Value.Add(new OrderLineState { DisplayType = "section" });
    }).Ghost();
    new Button("Add a note", onClick: async e => {
        orderLines.Value.Add(new OrderLineState { DisplayType = "note" });
    }).Ghost();
});

// State class for order line
public class OrderLineState
{
    public int Sequence { get; set; } = 10;
    public string DisplayType { get; set; } = "product";

    [Required]
    [Display(Name = "Product")]
    public int? ProductId { get; set; }

    public string ProductName { get; set; } = "";
    public string Description { get; set; } = "";

    [Range(0, double.MaxValue)]
    public double Quantity { get; set; } = 1.0;

    public double UnitPrice { get; set; }
    public double Discount { get; set; }
    public double Subtotal => Quantity * UnitPrice * (1 - Discount / 100);
}
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `fields.One2many` | One-to-many relation | `UseState<List<T>>()` with editable DataTable |
| Inline `<tree>` | Column definitions for list | `.Header()` calls on DataTable |
| `editable="bottom/top"` | Enable inline editing | Use Dialog or Form for row editing |
| `<control><create>` | Custom add buttons | `Button` widgets above/below table |
| `context="{'default_*': ...}"` | Defaults for new records | Initial values in add button handler |
| `<field widget="handle">` | Drag reorder handle | Drag-to-reorder support |
| `sum="..."` | Column footer aggregate | Computed summary `TextBlock` below table |
| `decoration-*` | Row conditional styling | Row styling via format callbacks |
| `optional="hide/show"` | Column visibility toggle | DataTable column configuration |
| `readonly="1"` | Read-only child list | Display-only DataTable (no row actions) |
| Inline `<form>` | Edit form for child records | Dialog or Sheet for editing rows |
| `ondelete='cascade'` | Delete behavior | Business logic, not UI |
| `copy=True` | Copy children on duplicate | Business logic, not UI |
