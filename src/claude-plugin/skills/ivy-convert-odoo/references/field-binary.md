# Field: Binary

Odoo's binary field for generic file uploads and downloads. Stores file data as base64-encoded content. Used for attachments, documents, templates, and any non-image file storage.

## Odoo

```python
class IrAttachment(models.Model):
    _name = 'ir.attachment'

    name = fields.Char(string='Name', required=True)
    datas = fields.Binary(string='File Content', attachment=True)
    datas_fname = fields.Char(string='File Name')
    mimetype = fields.Char(string='MIME Type')
    file_size = fields.Integer(string='File Size')

class HrEmployee(models.Model):
    _name = 'hr.employee'

    resume = fields.Binary(string='Resume', attachment=True)
    resume_filename = fields.Char(string='Resume Filename')
    id_card = fields.Binary(string='ID Card', attachment=True)
```

```xml
<!-- Basic file upload with filename -->
<field name="resume" filename="resume_filename"/>
<field name="resume_filename" invisible="1"/>

<!-- With file type restrictions -->
<field name="datas" filename="datas_fname"
       options="{'accepted_file_extensions': '.pdf,.doc,.docx'}"/>

<!-- With MIME type filter -->
<field name="datas" options="{'allowed_mime_type': 'application/pdf'}"/>

<!-- Download link display -->
<field name="datas" widget="binary" filename="name"/>

<!-- In list view -->
<field name="datas" widget="binary" string="File"/>
```

## Ivy

```csharp
// Binary → FileInput
var resumeFile = UseState<IFile?>(null);
resumeFile.ToFileInput()
    .Accept(".pdf,.doc,.docx")
    .Placeholder("Upload resume...")
    .WithField()
    .Label("Resume");

// Multiple file upload
var attachments = UseState<IFile[]>([]);
attachments.ToFileInput()
    .MaxFiles(10)
    .MaxFileSize(10 * 1024 * 1024) // 10MB
    .Placeholder("Drop files here...")
    .WithField()
    .Label("Attachments");

// Drop zone variant
var document = UseState<IFile?>(null);
document.ToFileInput()
    .Variant(FileInputVariants.Drop)
    .Accept(".pdf")
    .Placeholder("Drop PDF here or click to browse")
    .WithField()
    .Label("Document");

// Download link for existing file
new Button(attachment.FileName, onClick: e => {
    Download(attachment.Url);
}).Ghost().Icon(Icons.Download);

// File list display
foreach (var file in existingFiles)
{
    PlaceHorizontal(() =>
    {
        new Icon(Icons.File);
        new TextBlock(file.Name);
        new TextBlock($"{file.Size / 1024}KB").Muted();
        new Button("Download", onClick: e => Download(file.Url)).Ghost();
    });
}

// In form state class
public class EmployeeFormState
{
    [Display(Name = "Resume")]
    public IFile? Resume { get; set; }

    [Display(Name = "ID Card")]
    public IFile? IdCard { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Binary` | Binary/file field | `FileInput` |
| `widget="binary"` | File upload/download widget | `FileInput` |
| `filename="field_name"` | Associated filename field | `IFile.FileName` property |
| `attachment=True` | Store as attachment | Server-side storage concern |
| `options="{'accepted_file_extensions': '...'}"` | Allowed file types | `.Accept("...")` |
| `options="{'allowed_mime_type': '...'}"` | MIME type filter | `.Accept()` with MIME patterns |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `readonly=True` | Download only (no upload) | Display as download link/button |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| File download | Download stored file | `Download(url)` or link button |
| File size display | Show file size | Format `IFile.Size` property |
