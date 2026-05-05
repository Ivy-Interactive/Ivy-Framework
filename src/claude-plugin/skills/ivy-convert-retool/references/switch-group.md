# Switch Group

A selection component that displays a group of switches to toggle multiple boolean values simultaneously. Each switch in the group can be independently toggled on or off.

## Retool

```toolscript
{{switchGroup1.value}}      // Array of booleans
{{switchGroup1.selectedItems}} // Array of selected item objects

switchGroup1.setValue([true, false, true])
switchGroup1.clearValue()
switchGroup1.resetValue()
```

## Ivy

Ivy does not have a dedicated Switch Group widget. The equivalent can be achieved by composing multiple `BoolInput` widgets with the `Switch` variant, or by using a `SelectInput` with the `Toggle` or `List` variant for multi-select.

**Option A — Multiple BoolInput switches:**

```csharp
public class SwitchGroupDemo : ViewBase
{
    public override object? Build()
    {
        var read = UseState(false);
        var write = UseState(false);
        var delete = UseState(false);

        return Layout.Vertical()
               | read.ToSwitchInput(Icons.Eye).Label("Read")
               | write.ToSwitchInput(Icons.Pencil).Label("Write")
               | delete.ToSwitchInput(Icons.Trash).Label("Delete");
    }
}
```

**Option B — SelectInput with Toggle variant (multi-select):**

```csharp
public class SwitchGroupSelectDemo : ViewBase
{
    public override object? Build()
    {
        var permissions = UseState<string[]>([]);
        var options = new[] { "Read", "Write", "Delete" }.ToOptions();

        return permissions.ToSelectInput(options)
                          .Variant(SelectInputs.Toggle)
                          .WithField()
                          .Label("Permissions");
    }
}
```

## Parameters

| Parameter         | Documentation                                      | Ivy                                                                 |
|-------------------|-----------------------------------------------------|---------------------------------------------------------------------|
| `labels`          | Display text for each switch option                 | `Label()` on each `BoolInput`, or option labels via `.ToOptions()`  |
| `values`          | Available options for selection                     | Individual `UseState` per switch, or array state with `SelectInput` |
| `value`           | Current boolean values array                        | `.Value` on each state, or array state `.Value`                     |
| `selectedIndexes` | Array of selected positions                         | Not supported                                                       |
| `selectedItems`   | Selected item objects                               | Not supported (use state values directly)                           |
| `disabled`        | Prevents all interaction                            | `.Disabled(true)` on each `BoolInput` or `SelectInput`              |
| `disabledByIndex` | Per-item disabled status                            | `.Disabled()` on individual `BoolInput` widgets                     |
| `hiddenByIndex`   | Per-item visibility                                 | Conditionally render individual widgets (ternary / null)            |
| `groupLayout`     | Arrangement: multiColumn, singleColumn, wrap, auto  | Use `Layout.Vertical()`, `Layout.Horizontal()`, or `Layout.Grid()` |
| `labelPosition`   | Label placement: top or left                        | `.WithField().Label()` (field label positioning)                    |
| `captionByIndex`  | Per-item captions/descriptions                      | `.Description()` on each `BoolInput`                                |
| `required`        | Enforces selection requirement                      | `.Invalid()` with custom validation logic                           |
| `tooltipText`     | Helper text on hover                                | Wrap with `Tooltip` widget                                          |
| `clearValue()`    | Reset all selections                                | `.Set(false)` on each state, or `.Set(new string[]{})`              |
| `resetValue()`    | Restore default values                              | Re-set each state to its initial value                              |
| `setValue()`      | Programmatically set selections                     | `.Set()` on each state                                              |
| `setDisabled()`   | Enable/disable interaction                          | `.Disabled()` property                                              |
| `setHidden()`     | Toggle visibility                                   | Conditional rendering                                               |
| `focus()`         | Direct keyboard focus                               | Not supported                                                       |
| `scrollIntoView()`| Scroll component into viewport                      | Not supported                                                       |
| `Change` event    | Triggered when selection values change              | `OnChange` event on each `BoolInput` or `SelectInput`              |
