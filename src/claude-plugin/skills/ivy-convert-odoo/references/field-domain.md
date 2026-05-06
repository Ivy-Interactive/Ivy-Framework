# Field: Domain

Odoo's domain builder widget for constructing record filter expressions. Provides a visual query builder interface for creating Odoo domain tuples (e.g., `[('state', '=', 'draft')]`). Used in automation rules, email templates, and dynamic filters.

## Odoo

```python
class IrFilters(models.Model):
    _name = 'ir.filters'

    domain = fields.Char(string='Domain', required=True, default='[]')
    model_id = fields.Selection(string='Model', selection='_list_all_models')

class BaseAutomation(models.Model):
    _name = 'base.automation'

    filter_domain = fields.Char(string='Filter', default='[]')
    model_id = fields.Many2one('ir.model', string='Model')
```

```xml
<!-- Domain builder widget -->
<field name="domain" widget="domain" options="{'model': 'sale.order'}"/>

<!-- With model from field -->
<field name="filter_domain" widget="domain"
       options="{'in_dialog': true, 'foldable': true}"/>

<!-- With record count -->
<field name="domain" widget="domain"
       options="{'model': 'res.partner', 'count_limit': 10000}"/>

<!-- Allow expressions -->
<field name="domain" widget="domain"
       options="{'allow_expressions': true}"/>
```

## Ivy

```csharp
// Domain builder → Custom filter builder UI
// Map Odoo domain expressions to Ivy filter controls

// Option 1: Simple filter builder with field/operator/value rows
var filters = UseState(new List<FilterRule>());

foreach (var filter in filters.Value)
{
    PlaceHorizontal(() =>
    {
        filter.Field.ToSelectInput(availableFields.ToOptions())
            .Placeholder("Field");
        filter.Operator.ToSelectInput(new[] {
            new Option("=", "equals"),
            new Option("!=", "not equals"),
            new Option("ilike", "contains"),
            new Option(">", "greater than"),
            new Option("<", "less than"),
            new Option("in", "in list"),
        }).Placeholder("Operator");
        filter.Value.ToTextInput()
            .Placeholder("Value");
        new Button("Remove", onClick: async e => {
            filters.Value.Remove(filter);
        }).Destructive().Ghost();
    });
}
new Button("Add Filter", onClick: async e => {
    filters.Value.Add(new FilterRule());
}).Ghost();

// Option 2: Use TextInput for raw domain expression (advanced users)
var domainExpr = UseState("[]");
domainExpr.ToTextInput()
    .Variant(TextInputVariant.Textarea)
    .Placeholder("[('field', 'operator', 'value')]")
    .WithField()
    .Label("Domain Filter");

// Filter rule state
public class FilterRule
{
    public string Field { get; set; } = "";
    public string Operator { get; set; } = "=";
    public string Value { get; set; } = "";
}

// Note: A full visual query builder with drag-drop, nested conditions,
// and field-type-aware value inputs is a complex component.
// Consider building a custom QueryBuilder widget for full parity.
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="domain"` | Visual domain/query builder | Custom filter builder UI with field/operator/value rows |
| `options="{'model': '...'}"` | Target model for field list | Available fields list for SelectInput |
| `options="{'in_dialog': true}"` | Open builder in dialog | Dialog with filter builder content |
| `options="{'foldable': true}"` | Collapsible display | Expandable/Details widget wrapping builder |
| `options="{'count_limit': N}"` | Max records to count | Query count with limit |
| `options="{'allow_expressions': true}"` | Allow Python expressions | Advanced text input mode |
| Domain string `[(...)]` | Odoo domain tuple format | Custom filter rule list |
| Record count display | Shows matching record count | Display query result count |
| AND/OR operators | Logical grouping | Nested filter groups |
| `default='[]'` | Empty domain default | Initial empty filter list |
