# Field: Email

Odoo's email widget for email address fields. Displays as a clickable `mailto:` link in read mode and a text input in edit mode.

## Odoo

```python
class ResPartner(models.Model):
    _name = 'res.partner'

    email = fields.Char(string='Email')
    email_from = fields.Char(string='Email From')
```

```xml
<!-- Email widget (creates mailto: link) -->
<field name="email" widget="email"/>

<!-- With placeholder -->
<field name="email" widget="email" placeholder="user@example.com"/>

<!-- In list view -->
<field name="email" widget="email" optional="show"/>
```

## Ivy

```csharp
// Email → TextInput with Email variant
var email = UseState("");
email.ToTextInput()
    .Variant(TextInputVariant.Email)
    .Placeholder("user@example.com")
    .WithField()
    .Label("Email");

// Display as clickable mailto link (read-only)
if (!string.IsNullOrEmpty(partner.Email))
{
    new Button(partner.Email, onClick: e => {
        // Open mailto: link
    }).Link().Icon(Icons.Mail);
}

// In form state class
public class ContactFormState
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="email"` | Email with mailto: link | `TextInput` with `.Variant(TextInputVariant.Email)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `placeholder="..."` | Placeholder text | `.Placeholder("...")` |
| mailto: link (read mode) | Clickable email link | `Button` with `.Link()` and mail icon |
| `readonly=True` | Display only | `TextBlock` or link display |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `[EmailAddress]` validation | Email format validation | `[EmailAddress]` data annotation |
