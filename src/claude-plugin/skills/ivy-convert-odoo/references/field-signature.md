# Field: Signature

Odoo's digital signature capture widget. Opens a signature pad dialog for drawing signatures, stores the result as a base64-encoded PNG image. Used for approvals, contracts, and delivery confirmations.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    signature = fields.Binary(string='Signature', copy=False, attachment=True)
    signed_by = fields.Char(string='Signed By')
    signed_on = fields.Datetime(string='Signed On', copy=False)

class StockPicking(models.Model):
    _name = 'stock.picking'

    signature = fields.Binary(string='Signature', help='Signature of the customer')
```

```xml
<!-- Signature widget -->
<field name="signature" widget="signature"
       options="{'full_name': 'signed_by'}"/>

<!-- With size preset -->
<field name="signature" widget="signature"
       options="{'full_name': 'partner_name', 'size': [256, 85]}"/>

<!-- Signature type (signature vs initial) -->
<field name="signature" widget="signature"
       options="{'type': 'signature'}"/>

<!-- Read-only display -->
<field name="signature" widget="signature" readonly="1"/>
```

## Ivy

```csharp
// Signature → FileInput for capture, Image for display
// Option 1: File upload for signature image
var signatureFile = UseState<IFile?>(null);
signatureFile.ToFileInput()
    .Accept(".png,.jpg")
    .Placeholder("Upload signature...")
    .WithField()
    .Label("Signature");

// Option 2: Display existing signature
if (order.Signature != null)
{
    new Image(order.SignatureUrl)
        .Width(256)
        .Height(85);
    new TextBlock($"Signed by {order.SignedBy} on {order.SignedOn:MMM dd, yyyy}").Muted();
}

// Option 3: Custom signature dialog with canvas
new Button("Sign", onClick: async e => {
    // Open signature capture dialog
    var result = await ShowDialog<SignatureState>("Capture Signature");
    if (result != null)
    {
        await api.SaveSignature(orderId, result.SignatureData, result.Name);
    }
}).Primary().Icon(Icons.Pen);

// Signature state
public class SignatureState
{
    [Display(Name = "Full Name")]
    [Required]
    public string Name { get; set; } = "";

    public byte[] SignatureData { get; set; } = [];
}

// Note: Native signature pad widget is not built-in to Ivy.
// Use a custom HTML canvas component or third-party library for
// actual signature drawing capability.
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="signature"` | Signature capture pad | Custom signature dialog or FileInput |
| `options="{'full_name': 'field'}"` | Pre-fill signer name | Text input in signature dialog |
| `options="{'size': [W, H]}"` | Signature dimensions | `Image` width/height for display |
| `options="{'type': 'signature'}"` | Signature mode | Full signature capture |
| `options="{'type': 'initial'}"` | Initials mode | Compact initial capture |
| `options="{'default_font': '...'}"` | Font for typed signature | Font selection in dialog |
| Binary storage (base64 PNG) | Stored signature image | File/byte array storage |
| `readonly=True` | Display only | `Image` widget |
| `copy=False` | Don't copy on duplicate | Business logic |
| Click to sign | Opens signature dialog | Button opening Dialog with canvas |
