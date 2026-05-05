# Field: PDF Viewer

Odoo's PDF document viewer widget. Displays PDF files inline using PDF.js, with upload capability. Used for viewing invoices, reports, and document attachments directly within the form.

## Odoo

```python
class AccountMove(models.Model):
    _name = 'account.move'

    invoice_pdf = fields.Binary(string='Invoice PDF', attachment=True)
    invoice_pdf_name = fields.Char(string='PDF Filename')

class IrAttachment(models.Model):
    _name = 'ir.attachment'

    datas = fields.Binary(string='File Content')
    mimetype = fields.Char(string='MIME Type')
```

```xml
<!-- PDF viewer widget -->
<field name="invoice_pdf" widget="pdf_viewer"/>

<!-- With preview thumbnail -->
<field name="datas" widget="pdf_viewer"
       options="{'preview_image': 'thumbnail'}"/>

<!-- Read-only PDF display -->
<field name="invoice_pdf" widget="pdf_viewer" readonly="1"/>
```

## Ivy

```csharp
// PDF viewer → Iframe or Embed with PDF URL
// Option 1: Iframe for PDF display
if (invoice.PdfUrl != null)
{
    new Iframe(invoice.PdfUrl)
        .Width("100%")
        .Height(600);
}

// Option 2: Embed widget
new Embed(pdfUrl);

// Option 3: Download link + upload
if (invoice.PdfUrl != null)
{
    new Button("View PDF", onClick: e => {
        // Open in new tab
    }).Ghost().Icon(Icons.FileText);

    new Button("Download", onClick: e => {
        Download(invoice.PdfUrl);
    }).Ghost().Icon(Icons.Download);
}

// Upload new PDF
var pdfFile = UseState<IFile?>(null);
pdfFile.ToFileInput()
    .Accept(".pdf")
    .Placeholder("Upload PDF...")
    .WithField()
    .Label("Invoice PDF");

// In form state class
public class InvoiceFormState
{
    [Display(Name = "Invoice PDF")]
    public IFile? InvoicePdf { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="pdf_viewer"` | Inline PDF viewer | `Iframe` or `Embed` with PDF URL |
| PDF.js renderer | Client-side PDF rendering | Browser native PDF or iframe |
| `options="{'preview_image': '...'}"` | Thumbnail preview field | Thumbnail `Image` widget |
| Upload capability | Replace PDF file | `FileInput` with `.Accept(".pdf")` |
| Download button | Download PDF | `Button` with download action |
| `readonly=True` | View only (no upload) | Display only, no FileInput |
| Page tracking | `{field}_page` field | Custom page state (if needed) |
| Print/download buttons | Hidden by default in Odoo | Custom button controls |
