# Field: Html

Odoo's rich text HTML editor field. Provides a WYSIWYG editor for formatted content including bold, italic, lists, images, tables, and embedded media. Used for website content, email templates, and formatted descriptions.

## Odoo

```python
class ProductTemplate(models.Model):
    _name = 'product.template'

    website_description = fields.Html(string='Website Description', sanitize=True,
                                       sanitize_attributes=True, sanitize_form=True)
    description_sale = fields.Html(string='Sales Description', translate=True)
    notes = fields.Html(string='Internal Notes', sanitize=False)
```

```xml
<!-- Rich text editor -->
<field name="website_description"/>

<!-- With sanitization options -->
<field name="description_sale" options="{'collaborative': true}"/>

<!-- Read-only HTML display -->
<field name="notes" readonly="1"/>

<!-- In kanban card (limited display) -->
<field name="website_description" class="oe_kanban_text_ellipsis"/>
```

## Ivy

```csharp
// Html → TextInput with RichText variant
var websiteDescription = UseState("");
websiteDescription.ToTextInput()
    .Variant(TextInputVariant.RichText)
    .Placeholder("Add website description...")
    .WithField()
    .Label("Website Description");

// Read-only HTML display
new Html(notes.Value);

// Or as Markdown (if content can be converted)
new Markdown(htmlContent);

// In form state class
public class ProductState
{
    [Display(Name = "Website Description")]
    public string WebsiteDescription { get; set; } = "";

    [Display(Name = "Sales Description")]
    public string DescriptionSale { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Html` | Rich HTML editor field | `TextInput` with `.Variant(TextInputVariant.RichText)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `sanitize=True` | Clean HTML on save | Server-side HTML sanitization |
| `sanitize_attributes=True` | Strip dangerous attributes | Server-side sanitization |
| `sanitize_form=True` | Remove form elements | Server-side sanitization |
| `translate=True` | Translatable content | Not built-in (handle via localization) |
| `readonly=True` | Read-only display | `Html` widget for display or `.Disabled(true)` |
| `options="{'collaborative': true}"` | Collaborative editing | Not built-in |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `class="oe_kanban_text_ellipsis"` | Truncated display | CSS text overflow or `.MaxHeight()` |
