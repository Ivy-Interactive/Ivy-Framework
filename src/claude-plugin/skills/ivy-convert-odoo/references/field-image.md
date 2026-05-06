# Field: Image

Odoo's image field for uploading, storing, and displaying images. Based on Binary field with automatic resizing and format conversion. Used for product images, user avatars, and company logos.

## Odoo

```python
class ProductTemplate(models.Model):
    _name = 'product.template'

    image_1920 = fields.Image(string='Image', max_width=1920, max_height=1920)
    image_512 = fields.Image(string='Image 512', related='image_1920',
                              max_width=512, max_height=512, store=True)
    image_256 = fields.Image(string='Image 256', related='image_1920',
                              max_width=256, max_height=256, store=True)
    image_128 = fields.Image(string='Image 128', related='image_1920',
                              max_width=128, max_height=128, store=True)

class ResPartner(models.Model):
    _name = 'res.partner'

    avatar_128 = fields.Image(string='Avatar 128', max_width=128, max_height=128)
    image_1920 = fields.Image(string='Image')
```

```xml
<!-- Image upload with preview -->
<field name="image_1920" widget="image" class="oe_avatar"
       options="{'preview_image': 'image_128', 'size': [90, 90]}"/>

<!-- Image with zoom -->
<field name="image_1920" widget="image" options="{'zoom': true, 'zoom_delay': 300}"/>

<!-- Convert to WebP on upload -->
<field name="image_1920" widget="image" options="{'convert_to_webp': true}"/>

<!-- In kanban card -->
<field name="image_128" widget="image" class="o_kanban_image"/>

<!-- Avatar in list view -->
<field name="avatar_128" widget="image" class="rounded-circle" options="{'size': [32, 32]}"/>

<!-- Accepted file types -->
<field name="image_1920" widget="image"
       options="{'accepted_file_extensions': '.png,.jpg,.jpeg,.gif,.webp'}"/>
```

## Ivy

```csharp
// Image upload → FileInput with image accept filter
var imageFile = UseState<IFile?>(null);
imageFile.ToFileInput()
    .Accept(".png,.jpg,.jpeg,.gif,.webp")
    .Placeholder("Upload image...")
    .WithField()
    .Label("Product Image");

// Display uploaded/existing image
if (product.Value.ImageUrl != null)
    new Image(product.Value.ImageUrl);

// Avatar display
new Avatar(partner.Value.Name)
    .Image(partner.Value.AvatarUrl);

// Image in card/kanban
Card(() =>
{
    if (product.Value.ImageUrl != null)
        new Image(product.Value.ImageUrl).Width(128).Height(128);
    new TextBlock(product.Value.Name).Bold();
});

// In DataTable with image column
products.ToDataTable()
    .Header(p => p.ImageUrl, "Image", format: url =>
        url != null ? new Image(url).Width(32).Height(32) : new Empty())
    .Header(p => p.Name, "Product")
    .Header(p => p.Price, "Price");

// In form state class
public class ProductFormState
{
    [Display(Name = "Product Image")]
    public IFile? Image { get; set; }

    [Display(Name = "Product Name")]
    public string Name { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Image` | Image field (auto-resize) | `FileInput` with `.Accept(".png,.jpg,.jpeg,.gif,.webp")` |
| `widget="image"` | Image upload/display widget | `FileInput` for upload, `Image` for display |
| `max_width/max_height` | Auto-resize dimensions | Server-side image processing |
| `class="oe_avatar"` | Avatar-style display | `Avatar` widget with `.Image()` |
| `options="{'preview_image': '...'}"` | Thumbnail field for preview | Use smaller image URL for preview |
| `options="{'size': [W, H]}"` | Display dimensions | `.Width(W).Height(H)` on Image |
| `options="{'zoom': true}"` | Enable image zoom | Custom zoom overlay |
| `options="{'zoom_delay': 300}"` | Delay before zoom appears | Zoom timing configuration |
| `options="{'convert_to_webp': true}"` | Convert upload to WebP | Server-side image conversion |
| `options="{'accepted_file_extensions': '...'}"` | Allowed file types | `.Accept("...")` on FileInput |
| `class="o_kanban_image"` | Kanban card image style | `Image` with appropriate sizing |
| `related='image_1920'` | Derived from larger image | Server-side image derivation |
| `readonly=True` | Display only | `Image` widget (no upload) |
