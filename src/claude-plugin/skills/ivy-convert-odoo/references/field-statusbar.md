# Field: Statusbar

Odoo's status bar widget for displaying workflow state progression. Shows stages as clickable pills in a horizontal bar, typically placed in the form header. Works with both Selection and Many2one fields.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    state = fields.Selection([
        ('draft', 'Quotation'),
        ('sent', 'Quotation Sent'),
        ('sale', 'Sales Order'),
        ('done', 'Locked'),
        ('cancel', 'Cancelled'),
    ], string='Status', readonly=True, default='draft', copy=False)

class ProjectTask(models.Model):
    _name = 'project.task'

    stage_id = fields.Many2one('project.task.type', string='Stage',
                                 group_expand='_read_group_stage_ids',
                                 domain="[('project_ids', '=', project_id)]",
                                 tracking=True)
```

```xml
<!-- Selection-based statusbar (most common) -->
<header>
    <field name="state" widget="statusbar"
           statusbar_visible="draft,sent,sale,done"/>
</header>

<!-- Many2one statusbar (dynamic stages) -->
<header>
    <field name="stage_id" widget="statusbar"
           options="{'clickable': '1', 'fold_field': 'fold'}"/>
</header>

<!-- Non-clickable statusbar (read-only progression) -->
<field name="state" widget="statusbar"
       statusbar_visible="draft,confirmed,done"
       options="{'clickable': false}"/>

<!-- With folded stages -->
<field name="stage_id" widget="statusbar"
       options="{'fold_field': 'fold'}"/>
```

## Ivy

```csharp
// Statusbar → SelectInput with Toggle variant
var state = UseState("draft");
state.ToSelectInput(new[] {
    new Option("draft", "Quotation"),
    new Option("sent", "Quotation Sent"),
    new Option("sale", "Sales Order"),
    new Option("done", "Locked"),
}.ToOptions()).Variant(SelectInputVariant.Toggle);

// Or as Stepper for step-by-step progression
new Stepper(new[] {
    new Step("Quotation", state.Value == "draft" ? StepStatus.Current
        : state.Value == "sent" || state.Value == "sale" || state.Value == "done"
        ? StepStatus.Completed : StepStatus.Pending),
    new Step("Sent", state.Value == "sent" ? StepStatus.Current
        : state.Value == "sale" || state.Value == "done"
        ? StepStatus.Completed : StepStatus.Pending),
    new Step("Sales Order", state.Value == "sale" ? StepStatus.Current
        : state.Value == "done" ? StepStatus.Completed : StepStatus.Pending),
    new Step("Locked", state.Value == "done" ? StepStatus.Current : StepStatus.Pending),
});

// Many2one statusbar with dynamic stages
var stages = UseQuery(() => db.TaskStages
    .Where(s => s.ProjectId == projectId && !s.Fold)
    .OrderBy(s => s.Sequence)
    .ToList());
var stageId = UseState<int?>(null);

stageId.ToSelectInput(stages.Value.ToOptions(s => s.Id, s => s.Name))
    .Variant(SelectInputVariant.Toggle);

// Read-only status display as badges
new Badge(currentStateName)
    .Variant(state.Value switch {
        "draft" => BadgeVariants.Secondary,
        "sale" => BadgeVariants.Success,
        "cancel" => BadgeVariants.Destructive,
        _ => BadgeVariants.Info
    });
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="statusbar"` | Status bar widget | `SelectInput` with `.Variant(SelectInputVariant.Toggle)` or `Stepper` |
| `statusbar_visible="a,b,c"` | Visible stages (hide others) | Filter options list to include only visible stages |
| `options="{'clickable': '1'}"` | Allow clicking to change status | Default SelectInput behavior (clickable) |
| `options="{'clickable': false}"` | Read-only progression display | `.Disabled(true)` or `Stepper` (non-interactive) |
| `options="{'fold_field': 'fold'}"` | Fold/collapse inactive stages | Filter out folded stages from options |
| Selection field source | Static choices | Hard-coded `Option[]` array |
| Many2one field source | Dynamic stages from table | `UseQuery` to fetch stage options |
| `domain="[...]"` | Filter available stages | `.Where()` clause in stage query |
| `tracking=True` | Log state changes | Server-side audit logging |
| `readonly=True` | Non-editable status | `.Disabled(true)` or display as `Badge` |
| Cancelled state (excluded) | Usually hidden from statusbar | Omit from options list, show separately if needed |
