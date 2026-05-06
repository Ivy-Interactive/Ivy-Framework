# Form

A form to group and submit input fields. Can automatically generate input fields from a database or Table component, and populate nested input field values using default data. Supports validation, reset after submit, and event handling.

## Retool

```toolscript
// Form groups input fields with submit/reset functionality.
// Configure via Inspector: add input fields, set validation, event handlers.

// Submit the form programmatically:
form1.submit();

// Reset all child inputs to defaults:
form1.reset();

// Clear all values:
form1.clear();

// Set child input values:
form1.setData({ name: "John", email: "john@example.com" });

// Validate the form:
form1.validate();

// Clear validation messages:
form1.clearValidation();

// Toggle visibility and interaction:
form1.setHidden(false);
form1.setDisabled(true);

// Toggle header/footer/body:
form1.setShowHeader(true);
form1.setShowFooter(true);
```

## Ivy

```csharp
// Ivy equivalent: .ToForm() on state objects
public class UserFormState
{
    [Required]
    public string Name { get; set; } = "";

    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

// In your view:
var (formState, setFormState) = UseState(new UserFormState());

// Automatic form generation with validation
formState.ToForm();

// With custom layout using UseForm:
var form = UseForm(formState);
form.Place("Name", "Email");
form.PlaceHorizontal("Password");

// Forms integrate with sheets and dialogs:
formState.ToSheet();
formState.ToDialog();
```

## Parameters

| Parameter            | Documentation                                              | Ivy                                                   |
|----------------------|------------------------------------------------------------|-------------------------------------------------------|
| data                 | The form data                                              | State object passed to `.ToForm()`                    |
| initialData          | Data to populate default values                            | Default values on state class properties              |
| disabled             | Whether interaction is disabled                            | Not supported at form level                           |
| disableSubmit        | Whether to disable form submission                         | Not supported                                         |
| invalid              | Whether values have failed validation                      | Built-in via DataAnnotations                          |
| requireValidation    | Whether all fields must be valid to submit                 | Built-in (always validates on submit)                 |
| resetAfterSubmit     | Whether to reset values after submission                   | Not supported (manual reset via state)                |
| submitting           | Whether the Submit handler is running                      | Not supported                                         |
| loading              | Whether to display a loading indicator                     | Not supported                                         |
| hoistFetching        | Show loading when nested objects fetch                     | Not supported                                         |
| showBody             | Whether to show the body                                   | Not supported                                         |
| showHeader           | Whether to show the header area                            | Not supported                                         |
| showFooter           | Whether to show the footer area                            | Not supported                                         |
| showBorder           | Whether to show a border                                   | Not supported                                         |
| padding              | The amount of padding inside                               | Not supported                                         |
| margin               | The amount of margin outside                               | Not supported                                         |
| tooltipText          | Tooltip text on hover                                      | `[Display(Description = "...")]` attribute            |
| style                | Custom style options                                       | Not supported                                         |
| events (Submit)      | Event triggered on form submission                         | `UseEffect` watching state changes                    |
| events (Invalid)     | Event triggered when validation fails                      | Built-in validation error display                     |
| Field layout         | Drag and drop in IDE                                       | `.Place()`, `.PlaceHorizontal()`, `.Group()`          |
| Field labels         | Configured per input component                             | `[Display(Name = "...")]` attribute                   |
| Field ordering       | Drag and drop in IDE                                       | `[Display(Order = N)]` attribute                      |
