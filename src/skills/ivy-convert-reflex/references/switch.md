# Switch

A toggle switch alternative to the checkbox, allowing users to switch between two states (on/off). Commonly used for settings and boolean preferences.

## Reflex

```python
class SwitchState(rx.State):
    is_checked: bool = False

    def toggle(self, checked: bool):
        self.is_checked = checked

def index():
    return rx.switch(
        checked=SwitchState.is_checked,
        on_change=SwitchState.toggle,
    )
```

## Ivy

```csharp
public class SwitchDemo : ViewBase
{
    public override object? Build()
    {
        var state = UseState(false);
        return state.ToBoolInput()
            .Label("Accept Terms")
            .Variant(BoolInputs.Switch);
    }
}
```

## Parameters

| Parameter        | Documentation                                          | Ivy                                                    |
|------------------|--------------------------------------------------------|--------------------------------------------------------|
| `checked`        | Controls the switch state (`bool`)                     | `Value` / state passed via constructor                 |
| `default_checked`| Sets the initial checked state (`bool`)                | Initial value passed to `UseState()`                   |
| `disabled`       | Prevents user interaction when `True`                  | `.Disabled(bool)`                                      |
| `on_change`      | Fired when the value of the switch changes             | `.OnChange(Func<Event<IInput<bool>, bool>, ValueTask>)`|
| `name`           | Name for form submission (`str`)                       | Not supported                                          |
| `value`          | Value used for form submission (`str`)                 | Not supported                                          |
| `required`       | Requires user to check before form submission (`bool`) | `.Invalid(string)` for validation messaging            |
| `size`           | Size variant: `"1"`, `"2"`, `"3"`                      | Not supported                                          |
| `variant`        | Visual style: `"classic"`, `"surface"`, `"soft"`       | `BoolInputs.Switch` / `.Checkbox` / `.Toggle`          |
| `color_scheme`   | Color theme (e.g. `"tomato"`, `"red"`, `"blue"`)       | Not supported                                          |
| `high_contrast`  | Renders in high contrast mode (`bool`)                 | Not supported                                          |
| `radius`         | Border radius: `"none"`, `"small"`, `"full"`           | Not supported                                          |
| `as_child`       | Merges props onto child element (`bool`)               | Not supported                                          |
