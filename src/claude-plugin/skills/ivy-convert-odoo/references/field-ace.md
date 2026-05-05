# Field: Ace (Code Editor)

Odoo's code editor widget using the Ace Editor library. Provides syntax highlighting, line numbers, and theme support for editing code, XML templates, and Python expressions.

## Odoo

```python
class IrUiView(models.Model):
    _name = 'ir.ui.view'

    arch = fields.Text(string='View Architecture')

class IrServerAction(models.Model):
    _name = 'ir.actions.server'

    code = fields.Text(string='Python Code')
```

```xml
<!-- Code editor with XML mode -->
<field name="arch" widget="ace" options="{'mode': 'xml'}"/>

<!-- Python code editor -->
<field name="code" widget="ace" options="{'mode': 'python'}"/>

<!-- QWeb template editor (default mode) -->
<field name="arch" widget="ace"/>

<!-- HTML editor -->
<field name="body_html" widget="ace" options="{'mode': 'html'}"/>
```

## Ivy

```csharp
// Ace → CodeInput widget
var code = UseState("");
code.ToCodeInput()
    .Language("python")
    .WithField()
    .Label("Python Code");

// XML/HTML editor
var xmlContent = UseState("");
xmlContent.ToCodeInput()
    .Language("xml")
    .ShowCopyButton()
    .WithField()
    .Label("View Architecture");

// JavaScript editor
var jsCode = UseState("");
jsCode.ToCodeInput()
    .Language("javascript")
    .WithField()
    .Label("Script");

// Read-only code display
new CodeBlock(existingCode, "python");

// In form state class
public class ActionFormState
{
    [Display(Name = "Python Code")]
    public string Code { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="ace"` | Ace code editor | `CodeInput` |
| `options="{'mode': 'python'}"` | Language/syntax mode | `.Language("python")` |
| `options="{'mode': 'xml'}"` | XML syntax highlighting | `.Language("xml")` |
| `options="{'mode': 'qweb'}"` | QWeb (maps to XML) | `.Language("xml")` |
| `options="{'mode': 'html'}"` | HTML syntax highlighting | `.Language("html")` |
| Dark/light theme | Editor theme | Follows Ivy theme |
| Line numbers | Code line numbers | Built-in CodeInput feature |
| `readonly=True` | Read-only display | `CodeBlock` widget or `.Disabled(true)` |
| Save on form submit | Saves with form | State binding handles save |
