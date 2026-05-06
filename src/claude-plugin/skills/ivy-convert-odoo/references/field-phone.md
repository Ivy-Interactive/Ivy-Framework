# Field: Phone

Odoo's phone widget for telephone number fields. Displays as a clickable `tel:` link in read mode and a text input in edit mode. Enables one-tap dialing on mobile devices.

## Odoo

```python
class ResPartner(models.Model):
    _name = 'res.partner'

    phone = fields.Char(string='Phone')
    mobile = fields.Char(string='Mobile')
```

```xml
<!-- Phone widget (creates tel: link) -->
<field name="phone" widget="phone"/>

<!-- Mobile phone -->
<field name="mobile" widget="phone"/>

<!-- With dynamic placeholder -->
<field name="phone" widget="phone" options="{'placeholder_field': 'country_id'}"/>

<!-- In list view -->
<field name="phone" widget="phone" optional="show"/>

<!-- In kanban card -->
<field name="phone" widget="phone"/>
```

## Ivy

```csharp
// Phone → TextInput with phone formatting
var phone = UseState("");
phone.ToTextInput()
    .Variant(TextInputVariant.Tel)
    .Placeholder("+1 (555) 000-0000")
    .WithField()
    .Label("Phone");

// Display as clickable link (read-only)
if (!string.IsNullOrEmpty(partner.Phone))
{
    new Button(partner.Phone, onClick: e => {
        // Open tel: link
    }).Link().Icon(Icons.Phone);
}

// In form state class
public class ContactFormState
{
    [Display(Name = "Phone")]
    [Phone]
    public string Phone { get; set; } = "";

    [Display(Name = "Mobile")]
    [Phone]
    public string Mobile { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="phone"` | Phone with tel: link | `TextInput` with `.Variant(TextInputVariant.Tel)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `placeholder="..."` | Placeholder text | `.Placeholder("...")` |
| `options="{'placeholder_field': '...'}"` | Dynamic placeholder | `.Placeholder()` with dynamic value |
| tel: link (read mode) | Clickable phone link | `Button` with `.Link()` and phone icon |
| `readonly=True` | Display only | `TextBlock` or link display |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
