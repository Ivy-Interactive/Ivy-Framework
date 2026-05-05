# Slider

A slider provides user selection of a numeric value from a range by dragging a thumb along a track. Supports single-value and range selection with customizable min, max, and step.

## Reflex

```python
class SliderState(rx.State):
    value: int = 50

    @rx.event
    def set_end(self, value: list[int | float]):
        self.value = value[0]


def slider_intro():
    return rx.vstack(
        rx.heading(SliderState.value),
        rx.slider(
            default_value=50,
            min_=0,
            max=100,
            on_value_commit=SliderState.set_end,
        ),
        width="100%",
    )
```

## Ivy

Ivy does not have a dedicated Slider widget. The closest equivalent is `NumberInput` with the `ToSliderInput` factory method, which renders a slider-style numeric input.

```csharp
public class SliderDemo : ViewBase
{
    public override object? Build()
    {
        var value = UseState(50);
        return NumberInput.ToSliderInput(value)
                          .Min(0)
                          .Max(100);
    }
}
```

## Parameters

| Parameter        | Documentation                                              | Ivy                                                  |
|------------------|------------------------------------------------------------|------------------------------------------------------|
| `default_value`  | Initial slider value(s); pass a list for range slider      | Constructor argument or state binding                 |
| `value`          | Controlled current value                                   | State binding via `UseState`                          |
| `min`            | Minimum allowed value                                      | `.Min(double)`                                        |
| `max`            | Maximum allowed value                                      | `.Max(double)`                                        |
| `step`           | Increment step between values                              | `.Step(double)`                                       |
| `name`           | Identifier for form data submission                        | Not supported                                         |
| `width`          | Component width (default `"100%"`)                         | `.Width(Size)`                                        |
| `disabled`       | Disables user interaction                                  | `disabled` constructor parameter                      |
| `orientation`    | `"vertical"` or `"horizontal"` slider direction            | Not supported                                         |
| `size`           | Visual size (`"1"`, `"2"`, `"3"`)                          | `.Height(Size)` / `.Width(Size)`                      |
| `color_scheme`   | Theme color (e.g. `"tomato"`, `"blue"`)                    | Not supported                                         |
| `high_contrast`  | Enhanced contrast rendering                                | Not supported                                         |
| `on_change`      | Fires continuously while dragging                          | `OnChange` event handler                              |
| `on_value_commit`| Fires when thumb is released after dragging                | Not supported                                         |
| Range mode       | Pass `default_value=[25, 75]` for two-thumb range slider   | Not supported                                         |
| `precision`      | Not available                                              | `.Precision(int)` - decimal places to display         |
| `format_style`   | Not available                                              | `.FormatStyle()` - Decimal, Currency, or Percent      |
