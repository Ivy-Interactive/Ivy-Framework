# JSON Schema Form

A JSON-defined form component that generates input fields from a JSON schema and UI schema. Built on react-jsonschema-form, it allows programmatic form definition, dynamic schema changes, default data population, and input validation -- all configured through JSON rather than a visual editor.

## Retool

```toolscript
// JSON Schema — defines the fields
{
  "title": "{{ table1.selectedRow ? 'Edit' : 'Create new' }} deal",
  "type": "object",
  "properties": {
    "deal_name": { "type": "string", "title": "Deal Name" },
    "email":     { "type": "string", "title": "Email", "format": "email" },
    "amount":    { "type": "number", "title": "Amount" },
    "stage":     { "type": "string", "title": "Stage", "enum": ["Lead", "Opportunity", "Customer"] }
  },
  "required": ["deal_name", "email"]
}

// UI Schema — controls rendering
{
  "amount":    { "ui:widget": "updown" },
  "stage":     { "ui:widget": "radio" },
  "ui:order":  ["deal_name", "email", "amount", "stage"]
}

// Default Form Data — populate fields from a table row
{
  "email":     {{ table1.selectedRow ? table1.selectedRow.email : "email@example.com" }},
  "amount":    {{ table1.selectedRow ? table1.selectedRow.amount : 1000 }},
  "stage":     {{ table1.selectedRow ? table1.selectedRow.stage : "Lead" }},
  "deal_name": {{ table1.selectedRow ? table1.selectedRow.deal_name : "Company Name" }}
}

// Access submitted data
{{ jsonSchemaForm1.data.amount }}
```

## Ivy

Ivy does not have a JSON-schema-driven form. Instead, forms are generated from C# model types using `.ToForm()`, which automatically maps properties to input widgets based on their CLR type. Layout, validation, and rendering are configured via a fluent API and .NET `DataAnnotations`.

```csharp
// Define a model
public record Deal(
    [property: Required] string DealName,
    [property: Required, EmailAddress] string Email,
    [property: Range(0, 1_000_000)] decimal Amount,
    DealStage Stage
);

public enum DealStage { Lead, Opportunity, Customer }

// Generate and configure the form
var deal = UseState(() => new Deal("", "", 1000m, DealStage.Lead));

var form = deal.ToForm("Save deal")
    .Label(m => m.DealName, "Deal Name")
    .Description(m => m.Email, "We'll use this for verification")
    .Builder(m => m.Stage, s => s.ToSelectInput())
    .PlaceHorizontal(m => m.Amount, m => m.Stage)
    .Required(m => m.DealName, m => m.Email);

// React to submission
UseEffect(() =>
{
    if (!string.IsNullOrEmpty(deal.Value.DealName))
        client.Toast($"Deal '{deal.Value.DealName}' saved!");
}, deal);

return form;
```

## Parameters

| Parameter | Retool Documentation | Ivy |
|---|---|---|
| JSON Schema | JSON object defining field types, titles, enums, required fields, and validation rules using react-jsonschema-form spec | C# model types with `DataAnnotations` — types are auto-mapped to widgets (`string` -> TextInput, `enum` -> SelectInput, etc.) |
| UI Schema | JSON object controlling widget type, order, and rendering hints (e.g., `ui:widget`, `ui:order`) | `.Builder()` to override widget per field; `.Place()` / `.PlaceHorizontal()` for layout; `.Group()` for sections |
| Default Form Data | JSON object with initial field values; supports dynamic expressions from table rows | `UseState(() => new Model(...))` — initial values come from the model constructor |
| Title | Set via `"title"` key in the JSON schema; supports dynamic expressions | Submit button text via `ToForm("button text")`; no built-in form title property |
| Description | Set via `"description"` key in the JSON schema | `.Description(m => m.Field, "text")` per field; `[Display(Description = "...")]` via attribute |
| Submit Button Text | Configurable in the Inspector | First argument of `.ToForm("Submit")` |
| Validation | JSON schema validation rules (`required`, `minLength`, `pattern`, `minimum`, `maximum`, etc.) | .NET `DataAnnotations` (`[Required]`, `[Range]`, `[RegularExpression]`, `[EmailAddress]`, etc.) and `.Validate()` for custom rules |
| Required fields | `"required": ["field1", "field2"]` array in JSON schema | `.Required(m => m.Field1, m => m.Field2)` or non-nullable types are auto-required |
| Event: onSubmit | Event handler that triggers a query on form submission | `UseEffect(() => { ... }, state)` watches state changes after successful submission |
| Event: onChange | Event handler that fires when any field value changes | Not supported as a direct callback; reactive state updates via `UseState` |
| Data access | `{{ jsonSchemaForm1.data.fieldName }}` | `state.Value.FieldName` — strongly typed access |
| Generate from DB | Auto-generates JSON & UI schema from a PostgreSQL, MySQL, or MS SQL table | Not supported — models are defined in C# code |
| Single-column layout | Only supports single-column layout | Supports single-column (`.Place()`) and multi-column (`.PlaceHorizontal()`) layouts |
| Field grouping | Not supported natively | `.Group("Section Name", m => m.Field1, m => m.Field2)` or `[Display(GroupName = "...")]` |
| Conditional visibility | Not supported natively | `.Visible(m => m.Field, m => m.SomeBool)` — show/hide fields based on other values |
| Field help tooltips | Not supported (no Markdown in labels) | `.Help(m => m.Field, "tooltip text")` or `[Display(Prompt = "...")]` |
| Modal/dialog forms | Not supported | `.ToSheet(isOpen, ...)` / `.ToDialog(isOpen, ...)` |
| Custom submit handling | Event handler + query trigger | `UseForm()` hook returns `(onSubmit, formView, validationView, loading)` for full control |
| Programmatic data set | Methods like `setFormData`, `resetFormData`, `clearFormData` | `state.Set(new Model(...))` to update; re-renders the form automatically |
| Hidden / Disabled | Configurable in the Inspector | `.Visible` property; not directly a form-level toggle |
