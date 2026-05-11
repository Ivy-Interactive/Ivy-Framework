# Field: URL

Odoo's URL widget for web address fields. Displays as a clickable hyperlink in read mode (auto-prepends `http://` if missing) and a text input in edit mode.

## Odoo

```python
class ResPartner(models.Model):
    _name = 'res.partner'

    website = fields.Char(string='Website')

class ProductTemplate(models.Model):
    _name = 'product.template'

    website_url = fields.Char(string='Website URL', compute='_compute_website_url')
```

```xml
<!-- URL widget (creates clickable link) -->
<field name="website" widget="url"/>

<!-- With website_path option (no http:// prefix) -->
<field name="website_url" widget="url" options="{'website_path': true}"/>

<!-- With placeholder -->
<field name="website" widget="url" placeholder="https://www.example.com"/>

<!-- In list view -->
<field name="website" widget="url" optional="show"/>
```

## Ivy

```csharp
// URL → TextInput with URL variant
var website = UseState("");
website.ToTextInput()
    .Variant(TextInputVariant.Url)
    .Placeholder("https://www.example.com")
    .WithField()
    .Label("Website");

// Display as clickable link (read-only)
if (!string.IsNullOrEmpty(partner.Website))
{
    new Button(partner.Website, onClick: e => {
        // Open URL in new tab
    }).Link().Icon(Icons.ExternalLink);
}

// In form state class
public class PartnerFormState
{
    [Display(Name = "Website")]
    [Url]
    public string Website { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="url"` | URL with clickable link | `TextInput` with `.Variant(TextInputVariant.Url)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `placeholder="..."` | Placeholder text | `.Placeholder("...")` |
| `options="{'website_path': true}"` | Don't prepend http:// | Use URL as-is |
| Auto http:// prefix | Adds protocol if missing | Server-side URL normalization |
| Link opens new tab | target="_blank" | `.OpenInNewTab()` on Button or link |
| `readonly=True` | Display only | `TextBlock` or link display |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
