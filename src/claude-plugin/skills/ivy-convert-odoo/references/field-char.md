# Field: Char / Text

Odoo's basic string fields. `fields.Char` for single-line text, `fields.Text` for multi-line, and `fields.Html` for rich text.

## Odoo

```python
class ProductTemplate(models.Model):
    _name = 'product.template'

    name = fields.Char(string='Product Name', required=True, translate=True)
    description = fields.Text(string='Description')
    description_sale = fields.Text(string='Sales Description')
    website_description = fields.Html(string='Website Description', sanitize=True)
    default_code = fields.Char(string='Internal Reference', index=True)
    barcode = fields.Char(string='Barcode', copy=False)
```

```xml
<!-- In form view -->
<field name="name" placeholder="Product Name"/>
<field name="default_code"/>
<field name="description" placeholder="Add a description..."/>
<field name="website_description"/>

<!-- With widget variants -->
<field name="barcode" widget="CopyClipboardChar"/>
<field name="name" widget="char_emojis"/>
<field name="url" widget="url"/>
<field name="email" widget="email"/>
<field name="phone" widget="phone"/>
```

## Ivy

```csharp
// Char → TextInput (single line)
var name = UseState("");
name.ToTextInput()
    .Placeholder("Product Name")
    .WithField()
    .Label("Product Name")
    .Required();

// Text → TextInput with Textarea variant
var description = UseState("");
description.ToTextInput()
    .Variant(TextInputVariant.Textarea)
    .Placeholder("Add a description...")
    .WithField()
    .Label("Description");

// Html → TextInput with RichText variant (or Markdown)
var htmlContent = UseState("");
htmlContent.ToTextInput()
    .Variant(TextInputVariant.RichText)
    .WithField()
    .Label("Website Description");

// URL field → TextInput with validation
var url = UseState("");
url.ToTextInput()
    .Placeholder("https://...")
    .WithField()
    .Label("URL");

// Email/Phone → TextInput with appropriate type
var email = UseState("");
email.ToTextInput()
    .WithField()
    .Label("Email");

// In form state class
public class ProductState
{
    [Required]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = "";

    [Display(Name = "Internal Reference")]
    public string DefaultCode { get; set; } = "";

    public string Description { get; set; } = "";

    [EmailAddress]
    public string Email { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Char` | Single-line text | `TextInput` (default variant) |
| `fields.Text` | Multi-line text | `TextInput` with `.Variant(TextInputVariant.Textarea)` |
| `fields.Html` | Rich HTML editor | `TextInput` with `.Variant(TextInputVariant.RichText)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `required=True` | Mandatory field | `.Required()` or `[Required]` annotation |
| `placeholder="..."` | Placeholder text | `.Placeholder("...")` |
| `readonly=True` | Read-only display | `.Disabled(true)` or `.ReadOnly(true)` |
| `translate=True` | Translatable field | Not built-in (handle via localization) |
| `index=True` | Database index | Database-level concern, not UI |
| `copy=False` | Don't copy on duplicate | Business logic, not UI |
| `widget="url"` | URL with link | `TextInput` + link display |
| `widget="email"` | Email with mailto | `TextInput` with email validation |
| `widget="phone"` | Phone with tel: link | `TextInput` with phone format |
| `widget="CopyClipboardChar"` | Copy to clipboard button | `TextInput` + copy button (custom) |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `attrs="{'readonly':...}"` | Conditional read-only | `.Disabled(condition)` |
