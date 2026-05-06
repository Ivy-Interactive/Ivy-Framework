# Form

Forms are used to collect user input, group multiple input controls together, validate the data, and submit it as a unit. In Reflex this is `rx.form`; in Ivy the equivalent is the `ToForm()` API which auto-scaffolds a form from a state model.

## Reflex

```python
class FormState(rx.State):
    form_data: dict = {}

    def handle_submit(self, form_data: dict):
        self.form_data = form_data

def form_example():
    return rx.form(
        rx.vstack(
            rx.input(name="name", placeholder="Name"),
            rx.input(name="email", placeholder="Email"),
            rx.checkbox("Subscribe", name="subscribe"),
            rx.button("Submit", type="submit"),
        ),
        on_submit=FormState.handle_submit,
        reset_on_submit=True,
    )
```

## Ivy

```csharp
record ContactModel(string Name, string Email, bool Subscribe);

var contact = UseState(() => new ContactModel("", "", false));

UseEffect(() =>
{
    if (!string.IsNullOrEmpty(contact.Value.Name))
    {
        client.Toast($"Submitted: {contact.Value.Name}");
    }
}, contact);

return contact.ToForm()
    .Required(m => m.Name, m => m.Email)
    .Label(m => m.Name, "Name")
    .Label(m => m.Email, "Email")
    .Label(m => m.Subscribe, "Subscribe");
```

## Key Differences

- **Reflex** requires manually placing each input and assigning `name` props. Form data is received as a flat dictionary in `on_submit`.
- **Ivy** auto-generates inputs from the model's property types (string -> TextInput, bool -> BoolInput, etc.) and updates the state object directly on successful submission. Layout and labels are configured via fluent methods.

## Parameters

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Submit handler | `on_submit` event receives `dict` | `OnSubmit` event / `UseEffect` watches state changes |
| Reset on submit | `reset_on_submit=True` | Not supported (state is updated in place) |
| Field name | `name` prop on each input | Auto-derived from model property names |
| Required fields | HTML `required` attribute on inputs | `.Required(m => m.Prop)` fluent method |
| Validation (client) | `rx.form.message` with `match` prop (e.g. `"valueMissing"`, `"typeMismatch"`) | DataAnnotations (`[Required]`, `[EmailAddress]`, `[Range]`, etc.) |
| Validation (server) | `server_invalid` prop on `rx.form.field` | `.Validate<T>(m => m.Prop, val => (bool, string))` |
| Validation message | `rx.form.message` component | Automatic display from annotation `ErrorMessage` |
| Force match | `force_match` prop | Not supported |
| Clear server errors | `on_clear_server_errors` event | Not supported (automatic) |
| Field label | `rx.form.label` component | `.Label(m => m.Prop, "text")` |
| Field description | Manual text/components | `.Description(m => m.Prop, "text")` |
| Help tooltip | Manual implementation | `.Help(m => m.Prop, "text")` |
| Custom input type | Direct placement of any `rx.*` input | `.Builder(m => m.Prop, s => s.ToTextareaInput())` |
| Layout control | Manual with `rx.hstack` / `rx.vstack` | `.Place()`, `.PlaceHorizontal()`, `.Group()` |
| Conditional fields | Python conditionals / `rx.cond` | `.Visible(m => m.Prop, m => m.Condition)` |
| Dynamic fields | `rx.foreach` over state list | Not supported (model is statically typed) |
| Render as child | `as_child` prop | Not supported |
| Dialog/Sheet form | Manual modal implementation | `.ToDialog()` / `.ToSheet()` |
| Custom submit text | Button child text | `.ToForm("Submit text")` |
| Manual form control | Full manual layout | `UseForm()` returns `(onSubmit, formView, validationView, loading)` |
