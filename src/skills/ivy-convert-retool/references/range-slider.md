# Range Slider

An input field to select a range of number values. Users can define minimum and maximum values within a specified range by dragging two handles on a slider track.

## Retool

```toolscript
rangeSlider1.setValue({ start: 10, end: 90 });

// Access values
const min = rangeSlider1.value.start;
const max = rangeSlider1.value.end;

// Configure
rangeSlider1.min = 0;
rangeSlider1.max = 100;
rangeSlider1.step = 5;
```

## Ivy

Ivy does not have a dedicated Range Slider widget. The closest equivalent is `NumberInput<T>` with `Variant(NumberInputs.Slider)`, which provides a single-value slider. A range slider (two handles selecting a start and end value) would require combining two slider inputs or building a custom widget.

```csharp
// Single-value slider (closest equivalent)
var value = UseState(50.0);
return value.ToNumberInput()
    .Min(0)
    .Max(100)
    .Step(5)
    .Variant(NumberInputs.Slider)
    .WithField()
    .Label("Value");

// Simulating a range with two sliders
var start = UseState(10.0);
var end = UseState(90.0);
return new StackLayout(
    start.ToNumberInput()
        .Min(0).Max(100).Step(5)
        .Variant(NumberInputs.Slider)
        .WithField().Label("Start"),
    end.ToNumberInput()
        .Min(0).Max(100).Step(5)
        .Variant(NumberInputs.Slider)
        .WithField().Label("End")
);
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| `min` | `min` - minimum allowed value | `.Min(double)` |
| `max` | `max` - maximum allowed value | `.Max(double)` |
| `step` | `step` - increment/decrement step | `.Step(double)` |
| `value` | `value` - object with `start` and `end` | Single value only via `UseState<T>` - no range support |
| `disabled` | `disabled` - toggles interaction | `.Disabled(bool)` |
| `label` | `labelPosition` - label placement (`top`/`left`) | `.WithField().Label(string)` |
| `hideOutput` | `hideOutput` - conceals value caption | Not supported |
| `tooltipText` | `tooltipText` - helper text on hover | Not supported |
| `iconBefore` | `iconBefore` - prefix icon | Not supported |
| `iconAfter` | `iconAfter` - suffix icon | Not supported |
| `customValidation` | `customValidation` - conditional logic validation | `.Invalid(string)` - sets validation message |
| `hideValidationMessage` | `hideValidationMessage` - hides validation feedback | Not supported |
| `formDataKey` | `formDataKey` - key for Form data | Not supported |
| `isHiddenOnDesktop` | `isHiddenOnDesktop` - desktop visibility | `.Visible(bool)` |
| `isHiddenOnMobile` | `isHiddenOnMobile` - mobile visibility | Not supported (no mobile-specific toggle) |
| `maintainSpaceWhenHidden` | `maintainSpaceWhenHidden` - reserves space when hidden | Not supported |
| `margin` | `margin` - external spacing | Not directly supported on the input |
| `precision` | Not supported | `.Precision(int)` - decimal precision control |
| `formatStyle` | Not supported | `.FormatStyle(NumberFormatStyle)` - Decimal, Currency, Percent |
| `events.Change` | `Change` event on value modification | `OnChange` handler via constructor or event |
| `methods.setValue` | `setValue({start, end})` | State setter `UseState<T>.Set(value)` |
| `methods.clearValue` | `clearValue()` | Not supported (reset state manually) |
| `methods.resetValue` | `resetValue()` | Not supported (reset state manually) |
| `methods.focus` | `focus()` | Not supported |
| `methods.setDisabled` | `setDisabled(bool)` | `.Disabled(bool)` |
| `methods.setHidden` | `setHidden(bool)` | `.Visible(bool)` |
| `methods.scrollIntoView` | `scrollIntoView(options)` | Not supported |
| `methods.clearValidation` | `clearValidation()` | Not supported |
