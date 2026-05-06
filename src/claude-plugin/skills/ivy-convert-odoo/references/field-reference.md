# Field: Reference

Odoo's polymorphic reference field that can point to any model. Stores both the model name and record ID as a string (e.g., `sale.order,42`). Renders as a model selector dropdown plus a record selector.

## Odoo

```python
class MailFollowers(models.Model):
    _name = 'mail.followers'

    res_model = fields.Char(string='Related Document Model')
    res_id = fields.Many2oneReference(string='Related Document ID',
                                       model_field='res_model')

class IrAttachment(models.Model):
    _name = 'ir.attachment'

    res_model = fields.Char(string='Resource Model')
    res_id = fields.Many2oneReference(string='Resource ID', model_field='res_model')

class CustomLink(models.Model):
    _name = 'custom.link'

    reference = fields.Reference(string='Reference',
                                  selection=[
                                      ('sale.order', 'Sale Order'),
                                      ('purchase.order', 'Purchase Order'),
                                      ('account.move', 'Invoice'),
                                  ])
```

```xml
<!-- Reference field (model + record selector) -->
<field name="reference"/>

<!-- Hide model selector -->
<field name="res_id" options="{'hide_model': true}"/>

<!-- With model from another field -->
<field name="res_id" options="{'model_field': 'res_model'}"/>

<!-- In list view -->
<field name="reference" optional="show"/>
```

## Ivy

```csharp
// Reference → Two-step selection: model type + record
var modelType = UseState<string?>(null);
var recordId = UseState<int?>(null);

// Step 1: Model selector
modelType.ToSelectInput(new[] {
    new Option("sale_order", "Sale Order"),
    new Option("purchase_order", "Purchase Order"),
    new Option("invoice", "Invoice"),
}).Placeholder("Select type...")
    .WithField()
    .Label("Reference Type");

// Step 2: Record selector (changes based on model)
if (modelType.Value != null)
{
    var records = UseQuery(() => modelType.Value switch {
        "sale_order" => db.SaleOrders.Select(o => new { o.Id, o.Name }).ToList(),
        "purchase_order" => db.PurchaseOrders.Select(o => new { o.Id, o.Name }).ToList(),
        "invoice" => db.Invoices.Select(i => new { i.Id, i.Name }).ToList(),
        _ => new List<dynamic>()
    });

    recordId.ToSelectInput(records.Value.ToOptions(r => r.Id, r => r.Name))
        .Searchable()
        .Placeholder("Select record...")
        .WithField()
        .Label("Record");
}

// Display reference as link
if (modelType.Value != null && recordId.Value != null)
{
    new Button(referenceName, onClick: e => {
        Navigate($"/{modelType.Value}/{recordId.Value}");
    }).Link();
}

// In form state class
public class LinkFormState
{
    [Display(Name = "Reference Type")]
    public string? ModelType { get; set; }

    [Display(Name = "Record")]
    public int? RecordId { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Reference` | Polymorphic reference (model,id) | Two `SelectInput` widgets (model + record) |
| `fields.Many2oneReference` | Reference with external model field | `SelectInput` for record, model from another field |
| `selection=[...]` | Available model choices | Options list for model `SelectInput` |
| `model_field='res_model'` | Field holding model name | Dynamic record query based on model selection |
| `options="{'hide_model': true}"` | Hide model selector | Omit model SelectInput, use fixed model |
| `"model,id"` storage format | String format | Separate model and ID fields |
| Record display name | Shows linked record name | Fetch display name from selected record |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `readonly=True` | Display as link | Button or TextBlock with navigation |
