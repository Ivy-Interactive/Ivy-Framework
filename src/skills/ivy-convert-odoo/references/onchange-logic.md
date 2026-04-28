# Onchange Logic

Odoo's `@api.onchange` decorator for dynamic form behavior. Triggers client-side RPC calls when a field value changes, allowing other fields to update in real-time without saving. Used for cascading selections, auto-filling fields, showing warnings, and dynamic price calculations.

## Odoo

```python
class SaleOrderLine(models.Model):
    _name = 'sale.order.line'

    product_id = fields.Many2one('product.product', string='Product')
    name = fields.Text(string='Description')
    product_uom_qty = fields.Float(string='Quantity', default=1.0)
    price_unit = fields.Float(string='Unit Price')
    tax_ids = fields.Many2many('account.tax', string='Taxes')
    product_uom = fields.Many2one('uom.uom', string='Unit of Measure')

    @api.onchange('product_id')
    def _onchange_product_id(self):
        """When product changes, auto-fill description, price, taxes, UoM"""
        if self.product_id:
            self.name = self.product_id.get_product_multiline_description_sale()
            self.price_unit = self.product_id.lst_price
            self.tax_ids = self.product_id.taxes_id
            self.product_uom = self.product_id.uom_id
        else:
            self.name = ''
            self.price_unit = 0.0

    @api.onchange('product_uom_qty')
    def _onchange_qty(self):
        """Show warning for large quantities"""
        if self.product_uom_qty > 1000:
            return {
                'warning': {
                    'title': 'Large Quantity',
                    'message': f'You ordered {self.product_uom_qty} units. Please confirm.',
                }
            }

class SaleOrder(models.Model):
    _name = 'sale.order'

    partner_id = fields.Many2one('res.partner', string='Customer')
    pricelist_id = fields.Many2one('product.pricelist', string='Pricelist')
    payment_term_id = fields.Many2one('account.payment.term', string='Payment Terms')
    fiscal_position_id = fields.Many2one('account.fiscal.position', string='Fiscal Position')

    @api.onchange('partner_id')
    def _onchange_partner_id(self):
        """When customer changes, auto-fill pricelist, payment terms, fiscal position"""
        if self.partner_id:
            self.pricelist_id = self.partner_id.property_product_pricelist
            self.payment_term_id = self.partner_id.property_payment_term_id
            self.fiscal_position_id = self.env['account.fiscal.position'].with_company(
                self.company_id).get_fiscal_position(self.partner_id.id)

    @api.onchange('pricelist_id')
    def _onchange_pricelist(self):
        """Recalculate line prices when pricelist changes"""
        for line in self.order_line:
            line._onchange_product_id()
```

## Ivy

```csharp
// Onchange → reactive state updates with onChange handlers or computed properties

// Option 1: onChange handler on input widgets
var productId = UseState<int?>(null);
var description = UseState("");
var unitPrice = UseState(0.0);
var quantity = UseState(1.0);

// Product selection with onchange behavior
productId.ToSelectInput(products.Value.ToOptions(p => p.Id, p => p.Name))
    .WithField().Label("Product")
    .OnChange(async newProductId => {
        if (newProductId != null)
        {
            var product = await api.GetProduct(newProductId.Value);
            description.Set(product.Description);
            unitPrice.Set(product.ListPrice);
        }
        else
        {
            description.Set("");
            unitPrice.Set(0.0);
        }
    });

description.ToTextInput()
    .Variant(TextInputVariant.Textarea)
    .WithField().Label("Description");

unitPrice.ToNumberInput()
    .WithField().Label("Unit Price");

// Quantity with warning onchange
quantity.ToNumberInput()
    .WithField().Label("Quantity")
    .OnChange(async newQty => {
        if (newQty > 1000)
        {
            await Alert("Large Quantity",
                $"You ordered {newQty} units. Please confirm.");
        }
    });

// Option 2: Cascading selection (partner → pricelist, payment terms)
var partnerId = UseState<int?>(null);
var pricelistId = UseState<int?>(null);
var paymentTermId = UseState<int?>(null);

partnerId.ToSelectInput(partners.Value.ToOptions(p => p.Id, p => p.Name))
    .Searchable()
    .WithField().Label("Customer")
    .OnChange(async newPartnerId => {
        if (newPartnerId != null)
        {
            var partner = await api.GetPartner(newPartnerId.Value);
            pricelistId.Set(partner.PricelistId);
            paymentTermId.Set(partner.PaymentTermId);
        }
    });

pricelistId.ToSelectInput(pricelists.Value.ToOptions(p => p.Id, p => p.Name))
    .WithField().Label("Pricelist")
    .OnChange(async newPricelistId => {
        // Recalculate line prices when pricelist changes
        if (newPricelistId != null)
        {
            var prices = await api.GetPricelistPrices(newPricelistId.Value,
                orderLines.Value.Select(l => l.ProductId).ToList());
            foreach (var line in orderLines.Value)
                line.UnitPrice = prices.GetValueOrDefault(line.ProductId, line.UnitPrice);
        }
    });

// Option 3: Computed property (no server call needed)
public class OrderLineState
{
    public int? ProductId { get; set; }
    public double Quantity { get; set; } = 1.0;
    public double UnitPrice { get; set; }
    public double Discount { get; set; }

    // Automatically recalculates (like instant onchange)
    public double Subtotal => Quantity * UnitPrice * (1 - Discount / 100);
}
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `@api.onchange('field')` | Trigger on field change | `.OnChange(async newValue => { ... })` on input widget |
| `self.other_field = value` | Set another field's value | `otherState.Set(value)` in onChange handler |
| `return {'warning': {...}}` | Show warning dialog | `await Alert("title", "message")` in onChange |
| `return {'domain': {...}}` | Update field domain | Update options list for dependent SelectInput |
| Cascading onchange | Field A → fills B → fills C | Chained `.OnChange()` handlers or single handler updating multiple states |
| `@api.depends` + compute | Auto-recompute on dependency change | C# computed property (`=>` expression) |
| `@api.onchange` vs `@api.depends` | Client-side vs stored compute | `OnChange` (explicit) vs computed property (implicit) |
| Multi-field onchange | One handler for multiple fields | Separate `OnChange` per field, or shared update method |
| Onchange returns domain | Filter related field options | Reactive `UseQuery` with dependent filter |
| Onchange on One2many lines | Update parent from child change | Update parent state from line-level onChange |
