# Computed Fields

Odoo's computed field system for deriving values from other fields. Uses `@api.depends` to declare dependencies and `_compute_*` methods to calculate values. Supports stored (persisted) and non-stored (on-the-fly) computation, with `@api.onchange` for real-time form updates.

## Odoo

```python
class SaleOrderLine(models.Model):
    _name = 'sale.order.line'

    product_uom_qty = fields.Float(string='Quantity', default=1.0)
    price_unit = fields.Float(string='Unit Price')
    discount = fields.Float(string='Discount (%)')
    tax_ids = fields.Many2many('account.tax', string='Taxes')
    price_subtotal = fields.Monetary(string='Subtotal',
        compute='_compute_amount', store=True)
    price_tax = fields.Float(string='Tax',
        compute='_compute_amount', store=True)
    price_total = fields.Monetary(string='Total',
        compute='_compute_amount', store=True)

    @api.depends('product_uom_qty', 'discount', 'price_unit', 'tax_ids')
    def _compute_amount(self):
        for line in self:
            price = line.price_unit * (1 - (line.discount or 0.0) / 100.0)
            taxes = line.tax_ids.compute_all(price, quantity=line.product_uom_qty)
            line.update({
                'price_tax': taxes['total_included'] - taxes['total_excluded'],
                'price_total': taxes['total_included'],
                'price_subtotal': taxes['total_excluded'],
            })

class SaleOrder(models.Model):
    _name = 'sale.order'

    order_line = fields.One2many('sale.order.line', 'order_id')
    amount_untaxed = fields.Monetary(string='Untaxed Amount',
        compute='_compute_amounts', store=True)
    amount_tax = fields.Monetary(string='Taxes',
        compute='_compute_amounts', store=True)
    amount_total = fields.Monetary(string='Total',
        compute='_compute_amounts', store=True)

    @api.depends('order_line.price_subtotal', 'order_line.price_tax')
    def _compute_amounts(self):
        for order in self:
            order.amount_untaxed = sum(order.order_line.mapped('price_subtotal'))
            order.amount_tax = sum(order.order_line.mapped('price_tax'))
            order.amount_total = order.amount_untaxed + order.amount_tax
```

## Ivy

```csharp
// Computed fields → C# computed properties, UseQuery, or derived state

// Option 1: Computed properties on state class (non-stored, real-time)
public class OrderLineState
{
    public double Quantity { get; set; } = 1.0;
    public double UnitPrice { get; set; }
    public double Discount { get; set; }
    public double TaxRate { get; set; }

    // Computed: recalculates automatically when accessed
    public double Subtotal => Quantity * UnitPrice * (1 - Discount / 100.0);
    public double TaxAmount => Subtotal * TaxRate / 100.0;
    public double Total => Subtotal + TaxAmount;
}

// Display computed values
var line = UseState(new OrderLineState());
line.Value.Quantity.ToNumberInput().WithField().Label("Quantity");
line.Value.UnitPrice.ToNumberInput().WithField().Label("Unit Price");
line.Value.Discount.ToNumberInput().WithField().Label("Discount %");

// Computed values display automatically
new TextBlock($"Subtotal: {line.Value.Subtotal:C}");
new TextBlock($"Total: {line.Value.Total:C}");

// Option 2: Aggregated computed values from child records
var orderLines = UseState(new List<OrderLineState>());

// Derived totals (equivalent to @api.depends on child fields)
var amountUntaxed = orderLines.Value.Sum(l => l.Subtotal);
var amountTax = orderLines.Value.Sum(l => l.TaxAmount);
var amountTotal = amountUntaxed + amountTax;

PlaceHorizontal(() =>
{
    new TextBlock($"Untaxed: {amountUntaxed:C}");
    new TextBlock($"Taxes: {amountTax:C}");
    new TextBlock($"Total: {amountTotal:C}").Bold();
});

// Option 3: Server-computed via UseQuery (stored computed fields)
var orderTotals = UseQuery(() => db.SaleOrders
    .Where(o => o.Id == orderId)
    .Select(o => new {
        Untaxed = o.OrderLines.Sum(l => l.PriceSubtotal),
        Tax = o.OrderLines.Sum(l => l.PriceTax),
        Total = o.OrderLines.Sum(l => l.PriceTotal),
    })
    .First());
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `compute='_compute_*'` | Computed field declaration | C# computed property (`=>`) or derived variable |
| `@api.depends('field1', 'field2')` | Dependency declaration | Properties referenced in computed expression |
| `store=True` | Persist computed value to DB | Server-side computation via `UseQuery` |
| `store=False` (default) | Calculate on access | C# computed property or inline calculation |
| `_compute_*` method | Computation logic | Property getter or LINQ expression |
| `line.mapped('field')` | Extract field values from recordset | `.Select(l => l.Field)` in LINQ |
| `sum(recordset.mapped(...))` | Aggregate child values | `.Sum()` in LINQ |
| `@api.depends('child.field')` | Depend on child record fields | Aggregate over child collection |
| Chained computation | Computed depends on other computed | Nested computed properties |
| `readonly=True` (implicit) | Computed fields are read-only | Display as `TextBlock` or disabled input |
