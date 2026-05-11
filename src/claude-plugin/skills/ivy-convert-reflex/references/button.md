# Button

Triggers an action or event when clicked, such as submitting a form or calling an event handler. Supports loading and disabled states for user feedback.

## Reflex

```python
class CountState(rx.State):
    count: int = 0

    @rx.event
    def increment(self):
        self.count += 1

    @rx.event
    def decrement(self):
        self.count -= 1

def counter():
    return rx.flex(
        rx.button(
            "Decrement",
            color_scheme="red",
            on_click=CountState.decrement,
        ),
        rx.heading(CountState.count),
        rx.button(
            "Increment",
            color_scheme="grass",
            on_click=CountState.increment,
        ),
        spacing="3",
    )
```

## Ivy

```csharp
new Button("Click Me", onClick: _ => client.Toast("Hello!"))

// With variant and icon
new Button("Delete").Icon(Icons.Trash).Destructive()

// Loading and disabled states
new Button("Loading", loading: true)
new Button("Disabled", disabled: true)
```

## Parameters

| Parameter        | Documentation                                                        | Ivy                                          |
|------------------|----------------------------------------------------------------------|----------------------------------------------|
| on_click         | Event trigger called when the button is clicked                      | `OnClick: Func<Event<Button>, ValueTask>`    |
| disabled         | `bool` - Prevents interaction without displaying a spinner           | `Disabled: bool`                             |
| loading          | `bool` - Shows a loading spinner and prevents multiple clicks        | `Loading: bool`                              |
| variant          | `"classic" \| "solid" \| "soft" \| "surface" \| "outline" \| "ghost"` | `Variant: ButtonVariant` (Primary, Secondary, Destructive, Outline, Ghost, Link, Success, Warning, Info) |
| color_scheme     | `"tomato" \| "red" \| "grass"` etc. - Sets the button color         | Not supported (use `Variant` for semantics)  |
| size             | `"1" \| "2" \| "3" \| "4"` - Controls button size                   | Not supported                                |
| radius           | `"none" \| "small" \| "medium" \| "large" \| "full"`                | `BorderRadius: BorderRadius` (None, Rounded, Full) |
| high_contrast    | `bool` - Renders with higher contrast                                | Not supported                                |
| as_child         | `bool` - Merges props onto child element                             | Not supported                                |
| auto_focus       | `bool` - Focuses the button on mount                                 | Not supported                                |
| form             | `str` - Associates the button with a form by id                      | Not supported                                |
| form_action      | `str` - URL for form submission                                      | Not supported                                |
| form_enc_type    | `str` - Encoding type for form data                                  | Not supported                                |
| form_method      | `str` - HTTP method for form submission                              | Not supported                                |
| form_no_validate | `bool` - Skips form validation on submit                             | Not supported                                |
| form_target      | `str` - Target window for form submission                            | Not supported                                |
| name             | `str` - Name of the button for form submission                       | Not supported                                |
| type             | `"submit" \| "reset" \| "button"` - Button behavior type            | Not supported                                |
| value            | `Union[str, int, float]` - Value submitted with form data            | Not supported                                |
| —                | Not supported                                                        | `Title: string` - Button display text        |
| —                | Not supported                                                        | `Icon: Icons?` - Icon to display             |
| —                | Not supported                                                        | `IconPosition: Align` - Icon placement       |
| —                | Not supported                                                        | `Url: string` - Navigate to URL on click     |
| —                | Not supported                                                        | `Target: LinkTarget` - Link target behavior  |
| —                | Not supported                                                        | `Tooltip: string` - Hover tooltip text       |
