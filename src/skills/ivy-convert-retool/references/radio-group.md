# Radio Group

A selection component that enables users to choose a single value from a group of radio buttons.

## Retool

```toolscript
radioGroup1.value          // selected value
radioGroup1.selectedLabel  // display text of selected option
radioGroup1.selectedIndex  // position of selected item

// Configuration
{
  labels: ["Option A", "Option B", "Option C"],
  values: ["a", "b", "c"],
  value: "a",
  groupLayout: "singleColumn",
  disabled: false,
  required: false
}

// Methods
radioGroup1.setValue("b")
radioGroup1.clearValue()
radioGroup1.resetValue()
radioGroup1.setDisabled(true)
radioGroup1.setHidden(false)
radioGroup1.focus()
radioGroup1.validate()
radioGroup1.clearValidation()
```

## Ivy

Ivy does not have a dedicated Radio Group widget. The closest equivalent is `SelectInput<T>` with the `Toggle` variant, which renders inline button-style selection.

```csharp
// Single selection with toggle variant (closest to radio group)
var selected = new State<string>("a");

selected.ToSelectInput(new[]
{
    new Option<string>("a", "Option A"),
    new Option<string>("b", "Option B"),
    new Option<string>("c", "Option C"),
}).Variant(SelectInputs.Toggle);

// With change handler
var selection = UseState("a");
selection.ToSelectInput(new[]
{
    new Option<string>("a", "Option A"),
    new Option<string>("b", "Option B"),
    new Option<string>("c", "Option C"),
}).Variant(SelectInputs.Toggle);

// With field label and disabled state
selected.ToSelectInput(new[]
{
    new Option<string>("a", "Option A"),
    new Option<string>("b", "Option B"),
    new Option<string>("c", "Option C"),
}).Variant(SelectInputs.Toggle).Disabled()
    .Label("Pick one");
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Options / labels | `labels`, `values` arrays | `IEnumerable<IAnyOption>` passed to constructor |
| Selected value | `value` | `State<T>` or `TValue` parameter |
| Disabled | `disabled: bool` | `disabled: bool` / `.Disabled(bool)` |
| Layout | `groupLayout`: `auto`, `singleColumn`, `multiColumn`, `wrap` | Not supported |
| Caption per item | `captionByIndex: string[]` | Not supported |
| Tooltip per item | `tooltipByIndex: string[]` | Not supported |
| Disabled per item | `disabledByIndex: bool[]` / `disabledValues` | Not supported |
| Hidden per item | `hiddenByIndex: bool[]` | Not supported |
| Placeholder | Not applicable | `placeholder: string` |
| Required validation | `required: bool` | `.Invalid(string)` (manual validation) |
| Label position | `labelPosition`: `top` or `left` | `.Label(string)` via `.WithField()` |
| Visibility | `isHiddenOnDesktop`, `isHiddenOnMobile`, `setHidden()` | `.Visible(bool)` |
| Height | `heightType`: `auto`, `fixed`, `fill` | `.Height(Size)` |
| Margin | `margin: string` | Not supported (handled by layout) |
| Tooltip | `tooltipText: string` | Not supported |
| Style | `style: object` | Not supported |
| Change event | `events` (Change) | `OnChange` callback |
| Blur event | Not documented | `OnBlur` callback |
| `setValue()` | `setValue(value)` | Set via `State<T>.Value` |
| `clearValue()` | `clearValue()` | Set state to default |
| `resetValue()` | `resetValue()` | Set state to initial value |
| `focus()` | `focus()` | Not supported |
| `scrollIntoView()` | `scrollIntoView(options)` | Not supported |
| `validate()` / `clearValidation()` | `validate()`, `clearValidation()` | `.Invalid(string)` (manual) |
| Multi-select | Not applicable (use CheckboxGroup) | `selectMany: bool` |
| Variant | Fixed as radio buttons | `SelectInputs.Toggle` / `Select` / `List` |
