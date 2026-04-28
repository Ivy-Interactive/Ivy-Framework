# Checkbox Tree

A group of checkboxes to toggle boolean values in a multi-level tree. It displays options in a nested hierarchy where checking a parent can automatically check/uncheck its children, and child selections can propagate up to parents.

## Retool

```toolscript
// Checkbox Tree with nested options and event handler
checkboxTree1.setValue(["option1", "option2"]);

// Access selected values
checkboxTree1.value;               // current selected values
checkboxTree1.checkedPathArray;    // selected items in nested order
checkboxTree1.checkedPathStrings;  // selected items as delimited strings
checkboxTree1.selectedItems;       // list of selected item objects

// Control the component
checkboxTree1.clearValue();
checkboxTree1.resetValue();
checkboxTree1.setDisabled(true);
checkboxTree1.setHidden(false);
```

## Ivy

Ivy does not have a dedicated Checkbox Tree widget. The closest equivalent is `SelectInput` with the `List` variant and a collection type for multi-select, which renders checkboxes in a flat list.

```csharp
// Flat checkbox list (no tree hierarchy)
var selected = useState(new string[] { "Option A", "Option C" });

selected.ToSelectInput()
    .Placeholder("Select items")
    .Variant(SelectInputs.List)
.OnChange(async e => {
    // handle selection change
});
```

For a tree-like visual structure, `Expandable` widgets can be nested manually with `BoolInput` checkboxes, but parent-child check propagation must be implemented manually.

```csharp
var parentChecked = useState(false);
var child1Checked = useState(false);
var child2Checked = useState(false);

new Expandable(
    parentChecked.ToBoolInput().Label("Parent"),
    Layout.Vertical()
        | child1Checked.ToBoolInput().Label("Child 1")
        | child2Checked.ToBoolInput().Label("Child 2")
);
```

## Parameters

| Parameter | Documentation | Ivy |
|---|---|---|
| `value` | The current selected values (array) | `SelectInput.Value` / manual `useState` |
| `checkedPathArray` | Selected items in nested order | Not supported |
| `checkedPathStrings` | Selected items as delimited strings | Not supported |
| `selectedItems` | List of selected item objects | Not supported |
| `selectedIndexes` | Indices of selected values | Not supported |
| `checkStrictly` | Only check target node, exclude parents/children | Not supported |
| `delimiter` | Separator for `checkedPathStrings` | Not supported |
| `disabled` | Disables interaction | `SelectInput.Disabled` / `BoolInput.Disabled` |
| `required` | Enforces a selection | `SelectInput.Invalid` (manual validation) |
| `groupLayout` | Layout mode: `tree`, `singleColumn`, `wrap`, `multiColumn` | Not supported (flat list only) |
| `heightType` | Height mode: `auto`, `fixed`, `fill` | `SelectInput.Height` |
| `labels` | Display labels for items | Options list on `SelectInput` |
| `labelPosition` | Label position: `top` or `left` | Not supported |
| `margin` | Outer spacing | Not supported |
| `minCount` | Minimum selectable items | Not supported |
| `maxCount` | Maximum selectable items | Not supported |
| `disabledValues` | Values unavailable for selection | Not supported |
| `disabledByIndex` | Disabled state per item by index | Not supported |
| `captionByIndex` | Captions per item by index | Not supported |
| `tooltipByIndex` | Tooltips per item by index | Not supported |
| `tooltipText` | Tooltip text next to label | Not supported |
| `parentKeysByIndex` | Parent keys per item (tree structure) | Not supported |
| `leafPathArray` | List of expanded keys | Not supported |
| `leafPathStrings` | Expanded keys in delimited format | Not supported |
| `setValue()` | Set current values | `useState` setter |
| `clearValue()` | Clear all selections | `useState` setter with empty array |
| `resetValue()` | Reset to defaults | `useState` setter with initial value |
| `setDisabled()` | Toggle disabled state | `.Disabled()` modifier |
| `setHidden()` | Toggle visibility | `.Visible` property |
| `focus()` / `blur()` | Manage focus | Not supported |
| `clearValidation()` | Remove validation messages | `.Invalid(null)` |
| `scrollIntoView()` | Scroll component into view | Not supported |
| Change event | Triggered when selection changes | `OnChange` event handler |
