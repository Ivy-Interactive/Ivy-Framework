# Field: Text

Odoo's multi-line text field for longer content like descriptions, notes, and comments. Renders as a `<textarea>` in edit mode.

## Odoo

```python
class ProjectTask(models.Model):
    _name = 'project.task'

    description = fields.Text(string='Description')
    notes = fields.Text(string='Internal Notes')
    kanban_state_label = fields.Text(string='Kanban State Label', compute='_compute_label')
```

```xml
<!-- Basic textarea -->
<field name="description" placeholder="Add a description..."/>

<!-- With specific rows -->
<field name="notes" placeholder="Internal notes..." attrs="{'invisible': [('stage_id', '=', False)]}"/>

<!-- Read-only text display -->
<field name="kanban_state_label" readonly="1"/>

<!-- In list view (truncated) -->
<field name="description" optional="hide"/>
```

## Ivy

```csharp
// Text → TextInput with Textarea variant
var description = UseState("");
description.ToTextInput()
    .Variant(TextInputVariant.Textarea)
    .Placeholder("Add a description...")
    .Rows(5)
    .WithField()
    .Label("Description");

// Notes with conditional visibility
var notes = UseState("");
notes.ToTextInput()
    .Variant(TextInputVariant.Textarea)
    .Placeholder("Internal notes...")
    .Visible(stageId.Value != null)
    .WithField()
    .Label("Internal Notes");

// Read-only text display
new TextBlock(kanbanStateLabel).Variant(TextBlockVariants.Muted);

// In form state class
public class TaskFormState
{
    [Display(Name = "Description")]
    public string Description { get; set; } = "";

    [Display(Name = "Internal Notes")]
    public string Notes { get; set; } = "";
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Text` | Multi-line text field | `TextInput` with `.Variant(TextInputVariant.Textarea)` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `placeholder="..."` | Placeholder text | `.Placeholder("...")` |
| `required=True` | Mandatory field | `.Required()` or `[Required]` annotation |
| `readonly=True` | Read-only display | `.Disabled(true)` or `TextBlock` widget |
| `translate=True` | Translatable content | Not built-in (handle via localization) |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
| `attrs="{'readonly':...}"` | Conditional read-only | `.Disabled(condition)` |
| `optional="hide"` | Hidden by default in list | Column visibility toggle |
