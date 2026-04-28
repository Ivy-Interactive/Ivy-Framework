# Field: Boolean

Odoo's boolean field for true/false values. Renders as a checkbox by default, with toggle/switch widget variants.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    is_expired = fields.Boolean(string='Is Expired', compute='_compute_is_expired')
    require_signature = fields.Boolean(string='Online Signature', default=True)
    require_payment = fields.Boolean(string='Online Payment', default=True)
    show_update_pricelist = fields.Boolean(string='Update Pricelist', compute='_compute_show_update')
```

```xml
<!-- Default checkbox -->
<field name="require_signature"/>

<!-- With label -->
<field name="require_payment" string="Require Payment"/>

<!-- Boolean toggle widget -->
<field name="require_signature" widget="boolean_toggle"/>

<!-- Boolean favorite (star) -->
<field name="is_favorite" widget="boolean_favorite" nolabel="1"/>

<!-- Conditional visibility based on boolean -->
<field name="show_update_pricelist" invisible="1"/>
<button name="update_prices" string="Update Prices"
        attrs="{'invisible': [('show_update_pricelist', '=', False)]}"/>

<!-- In list view -->
<field name="require_signature" widget="boolean_toggle" optional="show"/>
```

## Ivy

```csharp
// Boolean → BoolInput (checkbox by default)
var requireSignature = UseState(true);
requireSignature.ToBoolInput()
    .Label("Online Signature")
    .WithField()
    .Label("Signature");

// Switch variant
var requirePayment = UseState(true);
requirePayment.ToBoolInput()
    .Variant(BoolInputVariants.Switch)
    .Label("Online Payment");

// Toggle variant
requireSignature.ToBoolInput()
    .Variant(BoolInputVariants.Toggle)
    .Label("Require Signature");

// Read-only boolean display
new Badge(isExpired ? "Expired" : "Active")
    .Variant(isExpired ? BadgeVariants.Destructive : BadgeVariants.Success);

// Conditional visibility using boolean state
new Button("Update Prices", onClick: async e => {
    await api.UpdatePrices(orderId);
}).Visible(showUpdatePricelist.Value);

// In form state class
public class OrderFormState
{
    [Display(Name = "Online Signature")]
    public bool RequireSignature { get; set; } = true;

    [Display(Name = "Online Payment")]
    public bool RequirePayment { get; set; } = true;
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Boolean` | Boolean field | `BoolInput` (checkbox default) |
| `widget="boolean_toggle"` | Toggle switch | `.Variant(BoolInputVariants.Switch)` or `.Variant(BoolInputVariants.Toggle)` |
| `widget="boolean_favorite"` | Star/favorite toggle | Custom icon toggle or `BoolInput` with star icon |
| `string="..."` | Field label | `.Label("...")` on BoolInput or `.WithField().Label("...")` |
| `default=True/False` | Default value | Initial `UseState` value |
| `readonly=True` | Read-only display | `.Disabled(true)` or display as Badge/text |
| `nolabel="1"` | Hide field label | Omit `.WithField().Label()` |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| computed field | Auto-calculated boolean | `UseQuery` or derived state |
| `optional="show/hide"` | Column toggle in list | DataTable column visibility |
