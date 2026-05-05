# Multiselect

An input field to select multiple options using a dropdown menu or manually enter values.

## Retool

```js
// Multiselect configured via the Inspector with values/labels arrays.
// Read selected values:
multiselect1.value           // array of selected values
multiselect1.selectedLabels  // array of selected labels
multiselect1.selectedItems   // array of selected item objects
multiselect1.selectedIndexes // array of selected indexes

// Programmatic control:
multiselect1.setValue(["opt1", "opt2"]);
multiselect1.clearValue();
multiselect1.resetValue();
multiselect1.setDisabled(true);
multiselect1.setHidden(false);
multiselect1.validate();
multiselect1.clearValidation();
multiselect1.focus();
```

## Ivy

In Ivy, multi-select is achieved by using `SelectInput<T>` with a **collection state** (e.g. `string[]`, `int[]`). The framework auto-detects the collection type and enables multi-select. Three visual variants are available: `Select` (dropdown), `List` (checkboxes), and `Toggle` (buttons).

```csharp
public class MultiSelectDemo : ViewBase
{
    public override object? Build()
    {
        var selected = UseState<string[]>([]);
        var options = new[] { "Option A", "Option B", "Option C", "Option D" }.ToOptions();

        return selected.ToSelectInput(options)
            .Variant(SelectInputs.Select)
            .Placeholder("Choose options...")
            .WithField()
            .Label("Pick multiple:");
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Multiple selection | Built-in (component is always multi-select) | Use a collection state type (`T[]`, `List<T>`) with `SelectInput<T>` |
| Values / Options | `values` array configured in Inspector | `IEnumerable<IAnyOption>` via `.ToOptions()` |
| Labels | `labels` array mapped to values | Labels derived from option values or custom `Option<T>(value, label)` |
| Placeholder | `placeholder` (string) | `.Placeholder(string)` |
| Disabled | `disabled` (bool) | `.Disabled(bool)` |
| Read-only | `readOnly` (bool) | Not supported |
| Required | `required` (bool) | Not supported (handle in form validation logic) |
| Loading indicator | `loading` (bool) | Use `AsyncSelectInput` with built-in loading states |
| Allow custom values | `allowCustomValue` (bool) | Not supported |
| Search mode | `searchMode` (`fuzzy`, `caseInsensitive`, `caseSensitive`) | Not supported (built-in search in `AsyncSelectInput`) |
| Max selection count | `maxCount` (number) | Not supported |
| Min selection count | `minCount` (number) | Not supported |
| Validation message | `clearValidation()` / `validate()` methods | `.Invalid(string)` to set error message |
| Selected value | `value` (read-only) | `.Value` property on state |
| Selected labels | `selectedLabels` (read-only array) | Not supported (derive from state + options) |
| Selected indexes | `selectedIndexes` (read-only array) | Not supported |
| Selected items | `selectedItems` (read-only array) | Not supported (derive from state) |
| Set value | `setValue(value)` method | `state.Set(value)` |
| Clear value | `clearValue()` method | `state.Set(Array.Empty<T>())` |
| Reset to default | `resetValue()` method | Not supported (re-initialize state manually) |
| Set disabled | `setDisabled(bool)` method | `.Disabled(bool)` |
| Set hidden | `setHidden(bool)` method | `.Visible` property or conditional rendering |
| Focus | `focus()` method | Not supported |
| Variant / Display style | Dropdown only | `.Variant()`: `Select` (dropdown), `List` (checkboxes), `Toggle` (buttons) |
| Prefix icon | `iconBefore` | Not supported |
| Suffix icon | `iconAfter` | Not supported |
| Prefix text | `textBefore` | Not supported |
| Suffix text | `textAfter` | Not supported |
| Tooltip / Helper text | `tooltipText` (markdown) | Not supported (use `.WithField().Label()`) |
| Label position | `labelPosition` (`top`, `left`) | `.WithField().Label()` (top position) |
| Empty message | `emptyMessage` | Not supported |
| Show clear button | `showClear` (bool) | Not supported |
| Wrap tags | `wrapTags` (bool) | Not supported |
| Overlay max height | `overlayMaxHeight` (px) | Not supported |
| Overlay min width | `overlayMinWidth` (px) | Not supported |
| Margin | `margin` (`4px 8px` or `0`) | Not supported (use layout containers) |
| Hidden on desktop | `isHiddenOnDesktop` (bool) | Not supported |
| Hidden on mobile | `isHiddenOnMobile` (bool) | Not supported |
| Maintain space when hidden | `maintainSpaceWhenHidden` (bool) | Not supported |
| Show in editor | `showInEditor` (bool) | Not applicable |
| Show selection indicator | `showSelectionIndicator` (bool) | Not supported |
| Persist search term | `persistSearchTerm` (bool) | Not supported |
| Clear input on change | `clearInputValueOnChange` (bool) | Not supported |
| Disabled values | `disabledValues` (array) | Not supported |
| Style overrides | `style` (object) | Not supported (use theming) |
| Change event | `Change` event handler | `OnChange` event |
| Blur event | Not available | `OnBlur` event |
| Nullable | Not applicable | `.Nullable(bool)` |
| Separator | Not applicable | `.Separator(char)` |
| Width / Height | Drag-resize on canvas | `.Width(Size)` / `.Height(Size)` |
