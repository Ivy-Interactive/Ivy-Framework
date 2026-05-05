# Select

An input field to select an option using a dropdown menu or manually enter a value.

## Retool

```js
// Set options via mapped data or static values
select1.setValue("option2");

// Read the selected value
const current = select1.value;
const label = select1.selectedLabel;
const item = select1.selectedItem;

// Event handler on change
select1.onChange(() => {
  query1.trigger();
});

// Programmatic control
select1.setDisabled(true);
select1.setHidden(false);
select1.clearValue();
select1.resetValue();
select1.focus();
select1.blur();
```

## Ivy

```csharp
// Basic select with ToSelectInput extension method
var favLang = UseState("C#");
var langs = new[] { "C#", "Java", "Go", "JavaScript", "F#" };

return favLang.ToSelectInput(langs.ToOptions())
    .Variant(SelectInputs.Select)
    .Placeholder("Choose a language...")
    .WithField()
    .Label("Favourite Language")
    .Width(Size.Full());

// With explicit onChange handler
var selectedSkill = UseState("");
var skillOptions = new[] { "C#", "Java", "Python" }.ToOptions();

selectedSkill.ToSelectInput(skillOptions)
    .Placeholder("Select a skill...");

// Multi-select (array-typed state auto-enables multi-select)
var languages = UseState<string[]>([]);
var options = new[] { "Option A", "Option B", "Option C" }.ToOptions();

languages.ToSelectInput(options)
    .Variant(SelectInputs.List);    // Checkbox-based list variant

// Toggle variant with integers
var nums = UseState<int[]>([]);
nums.ToSelectInput(new[] { 1, 2, 3, 4, 5 }.ToOptions())
    .Variant(SelectInputs.Toggle);  // Button-based toggle variant

// Validation and disabled states
normalSelect.ToSelectInput(options)
    .Invalid("This field is required")
    .Disabled(true);
```

## Parameters

| Parameter | Retool Documentation | Ivy |
|-----------|---------------------|-----|
| `value` | The current value (read-only). | `Value` property on `SelectInput<TValue>`. |
| `placeholder` | Text displayed when there is no value. Default `"Enter a value"`. | `Placeholder` constructor param / property. |
| `disabled` | Whether interaction is disabled. Default `false`. | `Disabled` constructor param / property. |
| `options` | Configured via mapped data source or static labels/values. | `Options` — `IEnumerable<IAnyOption>` passed to constructor. |
| `allowCustomValue` | Whether to allow values not in the options list. Default `false`. | Not supported |
| `allowDeselect` | Whether to allow deselecting an item. Default `false`. | `Nullable` property controls whether the value can be cleared. |
| `clearInputValueOnChange` | Whether to clear input value when selection changes. Default `null`. | Not supported |
| `disabledValues` | A list of values unavailable for selection. | Not supported |
| `emptyMessage` | Message displayed when no value exists. | Not supported |
| `inputValue` | Currently or recently entered text (read-only). | Not supported |
| `readOnly` | Whether input is read-only. Default `false`. | Not supported (use `Disabled` instead) |
| `required` | Whether a value must be selected. Default `false`. | `Invalid` property — set a validation message string to indicate invalid state. |
| `searchMode` | Search type: `fuzzy`, `caseInsensitive`, `caseSensitive`. | Not supported |
| `selectedIndex` | Index of selected value (read-only). | Not supported |
| `selectedItem` | The selected item object (read-only). | Not supported (access via state directly) |
| `selectedLabel` | Label of selected value (read-only). | Not supported (access via state/options) |
| `showClear` | Whether to display a clear button. Default `false`. | `Nullable` — when true, shows clear affordance. |
| `loading` | Whether to display a loading indicator. Default `false`. | Not supported (see `AsyncSelectInput` for async loading) |
| `tooltipText` | Tooltip text displayed next to the label on hover. | Not supported |
| `labelPosition` | Position of the label (`top` or `left`). Default `left`. | Not supported (use `Field` wrapper for labels) |
| `overlayMaxHeight` | Maximum height of the dropdown list in pixels. | Not supported |
| `overlayMinWidth` | Minimum width of the dropdown list in pixels. | Not supported |
| `showSelectionIndicator` | Whether to show an icon next to the selected item. Default `true`. | Not supported |
| `margin` | Margin around the component. Default `4px 8px`. | Not supported (handled by layout system) |
| `isHiddenOnDesktop` | Whether hidden in desktop layout. Default `false`. | `Visible` property. |
| `isHiddenOnMobile` | Whether hidden in mobile layout. Default `true`. | `Visible` property. |
| `style` | Custom style options. | `Scale` property for sizing. |
| `id` | Unique identifier. Default `select1`. | Not applicable (C# variable reference) |
| `selectMany` | N/A — Retool uses a separate Multiselect component. | `SelectMany` constructor param / property. |
| `variant` | N/A — Retool uses separate components for each variant. | `Variant` — `SelectInputs.Select`, `SelectInputs.List`, or `SelectInputs.Toggle`. |
| `separator` | N/A | `Separator` — character used to separate multi-select display values. |

### Methods

| Method | Retool | Ivy |
|--------|--------|-----|
| Set value | `select.setValue(value)` | Set via bound `State<T>` |
| Clear value | `select.clearValue()` | Set state to `default` / `null` |
| Reset value | `select.resetValue()` | Re-assign original state value |
| Focus | `select.focus()` | Not supported |
| Blur | `select.blur()` | Not supported |
| Set disabled | `select.setDisabled(bool)` | `select.Disabled = true` |
| Set hidden | `select.setHidden(bool)` | `select.Visible = false` |
| Clear validation | `select.clearValidation()` | `select.Invalid = null` |
| Scroll into view | `select.scrollIntoView(opts)` | Not supported |

### Events

| Event | Retool | Ivy |
|-------|--------|-----|
| Change | `Change` event handler | `OnChange` callback |
| Blur | `Blur` event handler | `OnBlur` callback |
| Focus | `Focus` event handler | Not supported |
| Input Value Change | `Input Value Change` event handler | Not supported |
