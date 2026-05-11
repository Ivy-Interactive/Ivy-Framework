# Field: Monetary

Odoo's currency-aware field for financial amounts. Automatically displays the appropriate currency symbol and formatting based on a linked currency field. Used for prices, totals, balances, and financial calculations.

## Odoo

```python
class AccountMove(models.Model):
    _name = 'account.move'

    currency_id = fields.Many2one('res.currency', string='Currency',
                                    default=lambda self: self.env.company.currency_id)
    amount_total = fields.Monetary(string='Total', currency_field='currency_id',
                                    compute='_compute_amount', store=True)
    amount_untaxed = fields.Monetary(string='Untaxed Amount', currency_field='currency_id')
    amount_tax = fields.Monetary(string='Tax Amount')
    amount_residual = fields.Monetary(string='Amount Due', currency_field='currency_id')
```

```xml
<!-- Basic monetary display -->
<field name="amount_total"/>

<!-- With explicit currency field -->
<field name="amount_total" options="{'currency_field': 'currency_id'}"/>

<!-- Hide currency symbol -->
<field name="amount_total" options="{'no_symbol': true}"/>

<!-- Hide trailing zeros -->
<field name="amount_total" options="{'hide_trailing_zeros': true}"/>

<!-- In list view with sum -->
<field name="amount_total" sum="Total" widget="monetary"/>

<!-- Read-only in form -->
<field name="amount_residual" readonly="1"/>
```

## Ivy

```csharp
// Monetary → NumberInput with currency formatting
var amountTotal = UseState(0.0m);
amountTotal.ToNumberInput()
    .FormatStyle(NumberFormatStyles.Currency)
    .Currency("USD")
    .Precision(2)
    .WithField()
    .Label("Total");

// Or with .ToMoneyInput() shorthand
var amountUntaxed = UseState(0.0m);
amountUntaxed.ToMoneyInput()
    .WithField()
    .Label("Untaxed Amount");

// Without currency symbol
var amount = UseState(0.0m);
amount.ToNumberInput()
    .Precision(2)
    .WithField()
    .Label("Amount");

// Read-only monetary display
new TextBlock($"{amountResidual:C}");

// In DataTable with sum
invoices.ToDataTable()
    .Header(i => i.Name, "Invoice")
    .Header(i => i.PartnerName, "Customer")
    .Header(i => i.AmountTotal, "Total", format: v => $"{v:C}")
    .Header(i => i.AmountResidual, "Amount Due", format: v => $"{v:C}")
    .Config(c => c.Sortable(true));

// Summary below table
new TextBlock($"Total: {invoices.Value.Sum(i => i.AmountTotal):C}  |  Due: {invoices.Value.Sum(i => i.AmountResidual):C}").Bold();

// In form state class
public class InvoiceFormState
{
    [Display(Name = "Currency")]
    public int CurrencyId { get; set; }

    [Display(Name = "Total")]
    [DataType(DataType.Currency)]
    public decimal AmountTotal { get; set; }

    [Display(Name = "Untaxed Amount")]
    [DataType(DataType.Currency)]
    public decimal AmountUntaxed { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Monetary` | Currency-aware field | `NumberInput` with `.FormatStyle(NumberFormatStyles.Currency)` or `.ToMoneyInput()` |
| `currency_field='currency_id'` | Linked currency field | `.Currency("USD")` or dynamic currency |
| `widget="monetary"` | Explicit monetary widget | `.FormatStyle(NumberFormatStyles.Currency)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `options="{'no_symbol': true}"` | Hide currency symbol | `.FormatStyle(NumberFormatStyles.Decimal)` with `.Precision(2)` |
| `options="{'hide_trailing_zeros': true}"` | Remove trailing zeros | Custom format |
| `readonly=True` | Read-only display | `.Disabled(true)` or `TextBlock` with `$"{v:C}"` |
| `compute='...'` | Computed field | Derived value or `UseQuery` |
| `sum="..."` | Column aggregate in list | Computed summary `TextBlock` below table |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `default=lambda...` | Default from company currency | Initial state value |
