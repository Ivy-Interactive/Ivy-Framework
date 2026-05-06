# Component: Group

Odoo's field grouping container within form views. A single `<group>` creates a two-column label-field layout. Nested `<group>` elements create side-by-side column sections. Used to organize form fields into logical visual groups.

## Odoo

```xml
<form>
    <sheet>
        <!-- Single group: fields in two columns (label | field) -->
        <group>
            <field name="name"/>
            <field name="email"/>
            <field name="phone"/>
        </group>

        <!-- Two groups side by side (4-column layout) -->
        <group>
            <group string="Personal Information">
                <field name="name"/>
                <field name="email"/>
                <field name="phone"/>
            </group>
            <group string="Work Information">
                <field name="company_id"/>
                <field name="department_id"/>
                <field name="job_id"/>
            </group>
        </group>

        <!-- Group with colspan and col attributes -->
        <group col="4">
            <field name="street" colspan="4"/>
            <field name="city"/>
            <field name="state_id"/>
            <field name="zip"/>
            <field name="country_id"/>
        </group>

        <!-- Conditional group -->
        <group attrs="{'invisible': [('type', '!=', 'company')]}">
            <field name="company_registry"/>
            <field name="vat"/>
        </group>
    </sheet>
</form>
```

## Ivy

```csharp
// Single group → fields with labels (default form layout)
var form = UseForm(partnerState);
form.Place("Name");
form.Place("Email");
form.Place("Phone");

// Two groups side by side → PlaceHorizontal
form.PlaceHorizontal(() =>
{
    Column(() =>
    {
        new TextBlock("Personal Information").Variant(TextBlockVariants.H4);
        form.Place("Name");
        form.Place("Email");
        form.Place("Phone");
    });
    Column(() =>
    {
        new TextBlock("Work Information").Variant(TextBlockVariants.H4);
        form.Place("CompanyId");
        form.Place("DepartmentId");
        form.Place("JobId");
    });
});

// Multi-column address layout → GridLayout
new GridLayout(columns: 2, () =>
{
    streetInput.ToTextInput()
        .Placeholder("Street")
        .WithField().Label("Street")
        .GridColumnSpan(2);
    cityInput.ToTextInput().WithField().Label("City");
    stateInput.ToSelectInput(states).WithField().Label("State");
    zipInput.ToTextInput().WithField().Label("ZIP");
    countryInput.ToSelectInput(countries).WithField().Label("Country");
});

// Conditional group
if (type.Value == "company")
{
    form.Place("CompanyRegistry");
    form.Place("Vat");
}
// Or with .Visible()
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<group>` | Field grouping container | Form layout, `PlaceHorizontal`, or `GridLayout` |
| `string="..."` | Group heading | `TextBlock` with H4 variant |
| `col="N"` | Number of columns | `GridLayout(columns: N)` |
| `colspan="N"` | Field spanning N columns | `.GridColumnSpan(N)` |
| Nested `<group>` | Side-by-side columns | `PlaceHorizontal` with `Column` blocks |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` or `if` statement |
| Label-field pairing | Auto label + input layout | `.WithField().Label("...")` |
| `<separator string="..."/>` | Section divider with label | `Separator` widget + `TextBlock` heading |
