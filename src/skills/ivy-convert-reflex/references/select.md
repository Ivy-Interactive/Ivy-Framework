# Select (Low Level)

Displays a list of options for the user to pick from, triggered by a button. The low-level API provides granular control through composable sub-components (`root`, `trigger`, `content`, `item`, `group`, `label`, `separator`) for building custom select dropdowns with option grouping, individual item disabling, and full state control.

## Reflex

```python
rx.select.root(
    rx.select.trigger(placeholder="Select a Fruit"),
    rx.select.content(
        rx.select.group(
            rx.select.label("Fruits"),
            rx.select.item("Orange", value="orange"),
            rx.select.item("Apple", value="apple"),
            rx.select.item("Grape", value="grape", disabled=True),
        ),
        rx.select.separator(),
        rx.select.group(
            rx.select.label("Vegetables"),
            rx.select.item("Carrot", value="carrot"),
            rx.select.item("Potato", value="potato"),
        ),
    ),
    default_value="apple",
    on_change=SelectState.set_value,
)
```

## Ivy

```csharp
public class SelectDemo : ViewBase
{
    public override object? Build()
    {
        var options = new[]
        {
            new Option("Orange", "orange"),
            new Option("Apple", "apple"),
            new Option("Grape", "grape"),
            new Option("Carrot", "carrot"),
            new Option("Potato", "potato"),
        };
        var selected = UseState("apple");

        return selected.ToSelectInput(options)
                       .Placeholder("Select a Fruit")
                       .Variant(SelectInputs.Select);
    }
}
```

## Parameters

| Parameter       | Reflex                                                            | Ivy                                                              |
|-----------------|-------------------------------------------------------------------|------------------------------------------------------------------|
| value           | `value: str` on `rx.select.root` — controlled current selection   | `Value: TValue` — current selection via state binding            |
| default_value   | `default_value: str` on `rx.select.root` — initial value         | Initial value passed to `UseState`                               |
| placeholder     | `placeholder: str` on `rx.select.trigger` — text when empty      | `Placeholder: string`                                            |
| options         | Defined structurally via `rx.select.item` children                | `Options: IAnyOption[]` — flat list passed to constructor        |
| option grouping | `rx.select.group` + `rx.select.label` for labeled groups         | Not supported                                                    |
| separator       | `rx.select.separator` — visual divider between groups            | `Separator: char` — delimiter for multi-value display            |
| disabled        | `disabled: bool` on `root` (entire select) or `item` (per-item) | `Disabled: bool` — disables entire input (per-item not supported)|
| required        | `required: bool` on `rx.select.root` — required for form submit | Not supported (use `Invalid` for validation messaging)           |
| name            | `name: str` on `rx.select.root` — form submission key            | Not supported (Ivy uses state binding, not form field names)     |
| size            | `size: "1" \| "2" \| "3"` on `rx.select.root`                   | `Scale: Scale?`                                                  |
| variant         | `variant: "classic" \| "surface" \| "ghost"` on trigger          | `Variant: SelectInputs` (Select, List, Toggle)                   |
| color_scheme    | `color_scheme: str` on trigger — theme color                     | Not supported                                                    |
| radius          | `radius: "none" \| "small" \| "medium" \| "large" \| "full"`    | Not supported                                                    |
| high_contrast   | `high_contrast: bool` — higher contrast rendering                | Not supported                                                    |
| width           | CSS-based width                                                   | `Width: Size`                                                    |
| height          | CSS-based height                                                  | `Height: Size`                                                   |
| open            | `open: bool` / `default_open: bool` — controls dropdown state   | Not supported                                                    |
| position        | `position: "item-aligned" \| "popper"` on content               | Not supported                                                    |
| visible         | Not a direct prop (use CSS/conditional rendering)                | `Visible: bool` — toggles component visibility                   |
| nullable        | Value can be empty string                                         | `Nullable: bool` — allows clearing selection to null             |
| multi-select    | Not supported (single selection only)                             | `SelectMany: bool` — enables multiple selections                 |
| invalid         | Not a direct prop (use custom styling/state)                     | `Invalid: string` — displays validation error message            |
| on_change       | `on_change` on `rx.select.root` — value changed                 | `OnChange`                                                       |
| on_open_change  | `on_open_change` on `rx.select.root` — dropdown opened/closed   | Not supported                                                    |
| on_blur         | Not a direct event on select                                      | `OnBlur` — fired when focus is lost                              |
