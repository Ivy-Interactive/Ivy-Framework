# Field: Properties

Odoo's dynamic property definition and value editor. Allows users to define custom fields at runtime without modifying the model. Supports various property types (char, integer, float, boolean, datetime, selection, many2one, tags). Used for flexible custom attributes on records.

## Odoo

```python
class ProjectTask(models.Model):
    _name = 'project.task'

    # Properties field - stores dynamic key-value pairs as JSON
    task_properties = fields.Properties(
        string='Properties',
        definition='project_id.task_properties_definition')

class ProjectProject(models.Model):
    _name = 'project.project'

    # Property definitions (schema for properties)
    task_properties_definition = fields.PropertiesDefinition(
        string='Task Properties Definition')
```

```xml
<!-- Properties field in form -->
<field name="task_properties"/>

<!-- With column layout -->
<field name="task_properties" options="{'columns': 2}"/>

<!-- In edit mode (allows defining new properties) -->
<field name="task_properties_definition"/>

<!-- In list view -->
<field name="task_properties"/>
```

## Ivy

```csharp
// Properties → Dynamic form fields based on schema
// Requires a flexible key-value editing approach

var properties = UseState(new Dictionary<string, object>());
var propertyDefinitions = UseQuery(() => db.PropertyDefinitions
    .Where(p => p.ProjectId == projectId)
    .ToList());

// Render dynamic fields based on definitions
foreach (var def in propertyDefinitions.Value)
{
    switch (def.Type)
    {
        case "char":
            var charVal = UseState(properties.Value.GetValueOrDefault(def.Key, "")?.ToString() ?? "");
            charVal.ToTextInput()
                .WithField().Label(def.Label);
            break;

        case "integer":
            var intVal = UseState(Convert.ToInt32(properties.Value.GetValueOrDefault(def.Key, 0)));
            intVal.ToNumberInput()
                .Step(1)
                .WithField().Label(def.Label);
            break;

        case "float":
            var floatVal = UseState(Convert.ToDouble(properties.Value.GetValueOrDefault(def.Key, 0.0)));
            floatVal.ToNumberInput()
                .Precision(2)
                .WithField().Label(def.Label);
            break;

        case "boolean":
            var boolVal = UseState(Convert.ToBoolean(properties.Value.GetValueOrDefault(def.Key, false)));
            boolVal.ToBoolInput()
                .Label(def.Label);
            break;

        case "datetime":
            var dateVal = UseState<DateTime?>(null);
            dateVal.ToDateTimeInput()
                .WithField().Label(def.Label);
            break;

        case "selection":
            var selVal = UseState<string?>(null);
            selVal.ToSelectInput(def.Options.ToOptions())
                .WithField().Label(def.Label);
            break;

        case "tags":
            var tagVals = UseState<string[]>([]);
            tagVals.ToSelectInput(def.Options.ToOptions())
                .WithField().Label(def.Label);
            break;
    }
}

// Property definition editor (admin)
new Button("Add Property", onClick: async e => {
    // Open dialog to define new property
    var newProp = await ShowDialog<PropertyDefinitionState>("Add Custom Property");
    if (newProp != null)
        await api.AddPropertyDefinition(projectId, newProp);
}).Ghost().Icon(Icons.Plus);

// Note: Full property management with drag-to-reorder, inline editing,
// and type-specific value inputs requires significant custom implementation.
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Properties` | Dynamic property values | `Dictionary<string, object>` with dynamic form fields |
| `fields.PropertiesDefinition` | Property schema definition | Custom property definition table/model |
| `definition='field'` | Link to definition source | Query property definitions from parent |
| `options="{'columns': N}"` | Layout columns (1 or 2) | `GridLayout(columns: N)` |
| Property types | char, int, float, bool, etc. | Switch statement rendering appropriate input widget |
| Drag-to-reorder | Reorder properties | Custom reorder controls |
| Property groups (separators) | Group properties visually | `Separator` widgets between groups |
| Add/remove properties | Runtime field management | Button + Dialog for property definition |
| `editMode` | Enable definition editing | Admin-only property management UI |
