# Segmented Control

A segmented control provides a set of mutually exclusive options displayed as adjacent buttons, allowing users to switch between predefined values or views. Only one option can be active at a time (or multiple, if configured).

## Reflex

```python
rx.segmented_control.root(
    rx.segmented_control.item(label="Home", value="home"),
    rx.segmented_control.item(label="About", value="about"),
    rx.segmented_control.item(label="Test", value="test"),
    value=SegmentedState.control,
    on_change=SegmentedState.setvar("control"),
)
```

## Ivy

```csharp
var selected = UseState("home");
var options = new[] { "home", "about", "test" }.ToOptions();

selected.ToSelectInput(options)
    .Variant(SelectInputs.Toggle);
```

## Parameters

| Parameter       | Documentation                                                                 | Ivy                                                      |
|-----------------|-------------------------------------------------------------------------------|----------------------------------------------------------|
| `value`         | Currently selected value(s). `Union[str, Sequence]`                           | Bound via `UseState` / `state` parameter                 |
| `default_value` | Initial value when uncontrolled. `Union[str, Sequence]`                       | Set through the initial value of `UseState`              |
| `on_change`     | Callback fired when the selection changes                                     | `onChange` event handler                                 |
| `type`          | `"single"` or `"multiple"` selection mode                                     | Automatic — uses `selectMany` / collection-typed state   |
| `size`          | Size of the control (`"1"`, `"2"`, etc.)                                      | Not supported                                            |
| `variant`       | Visual style: `"classic"` or `"surface"`                                      | Not supported (Toggle is a single visual style)          |
| `color_scheme`  | Color theme (`"tomato"`, `"red"`, etc.)                                       | Not supported (uses global theming)                      |
| `radius`        | Border radius (`"none"`, `"small"`, `"medium"`, `"large"`, `"full"`)          | Not supported                                            |
| `disabled`      | Not a direct prop — handled per-item or via state                             | `Disabled` property on `SelectInput`                     |
| `placeholder`   | Not supported                                                                 | `Placeholder` property on `SelectInput`                  |
| `options`       | Defined by composing `rx.segmented_control.item` children                     | Passed as `IEnumerable<IAnyOption>` via `.ToOptions()`   |
