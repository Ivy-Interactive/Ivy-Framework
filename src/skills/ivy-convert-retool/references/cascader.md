# Cascader

A hierarchical selection input that lets users pick options through a multi-level tree structure. Commonly used for location selection (Country > State > City), category browsing, or any nested taxonomy. Supports single and multiple selection, search, and various layout modes (multi-column, single-column, tree).

## Retool

```toolscript
// Cascader configured with hierarchical options
cascader.value          // Current selected value (array)
cascader.selectedItems  // Array of selected item objects
cascader.selectedLabels // Labels for selected values

// Methods
cascader.setValue(["electronics", "phones", "android"])
cascader.clearValue()
cascader.resetValue()
cascader.setDisabled(true)
cascader.setHidden(false)
cascader.focus()
cascader.blur()
cascader.scrollIntoView({ behavior: "auto", block: "nearest" })
```

## Ivy

Ivy does not have a dedicated Cascader widget. The equivalent pattern is achieved by chaining multiple `SelectInput` widgets where each level dynamically filters the options of the next. This is idiomatic in Ivy since state changes automatically trigger re-renders.

```csharp
public class CascaderDemo : ViewBase
{
    private static readonly Dictionary<string, string[]> CategoryOptions = new()
    {
        ["Programming"] = new[] { "C#", "Java", "Python", "JavaScript" },
        ["Design"]      = new[] { "Photoshop", "Figma", "Sketch" },
        ["Database"]    = new[] { "SQL Server", "PostgreSQL", "MongoDB" }
    };

    public override object? Build()
    {
        var selectedCategory = UseState("Programming");
        var selectedSkill = UseState("");

        var categoryOptions = CategoryOptions.Keys.ToOptions();
        var skillOptions = CategoryOptions[selectedCategory.Value].ToOptions();

        return Layout.Vertical()
            | selectedCategory.ToSelectInput(categoryOptions)
                .Placeholder("Choose a category...")
                .WithField()
                .Label("Category:")
            | selectedSkill.ToSelectInput(skillOptions)
                .Placeholder("Select a skill...")
                .WithField()
                .Label("Skill:");
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Value | `value` - current selected value (array) | `state.Value` via `UseState` |
| Options | `values` - hierarchical tree of options | `options` parameter on `SelectInput` (flat per level) |
| Disabled | `disabled` - toggles disabled state | `.Disabled(bool)` on `SelectInput` |
| Required / Validation | `required` - mandates selection | `.Invalid(string)` for validation messages |
| Placeholder | Not documented | `.Placeholder(string)` |
| Multiple selection | `maxCount` / `minCount` for constraining selections | `selectMany: true` or use array state type |
| Label position | `labelPosition` - `top` or `left` | `.WithField().Label(string)` (top by default) |
| Height | `heightType` - `auto`, `fixed`, or `fill` | `.Height(Size)` |
| Width | Not documented | `.Width(Size)` |
| Layout mode | `groupLayout` - `multiColumn`, `singleColumn`, `wrap`, `tree` | Not supported (use chained `SelectInput` widgets) |
| Check strictly (independent parent/child) | `checkStrictly` - parent/child not auto-linked | Not applicable (each level is an independent `SelectInput`) |
| Delimiter | `delimiter` - separator for path strings | `.Separator(char)` |
| Selected items (read) | `selectedItems` - array of selected objects | Read from state directly |
| Selected labels (read) | `selectedLabels` - labels array | Read from state directly |
| Checked paths (read) | `checkedPathArray` / `checkedPathStrings` | Not supported (manual composition) |
| Variant | Not supported | `.Variant(SelectInputs)` - `Select`, `List`, or `Toggle` |
| Change event | `Change` event | `onChange` callback or `OnChange` event |
| Focus / Blur methods | `focus()` / `blur()` | Not supported |
| Set value method | `setValue(value)` | `state.Set(value)` |
| Clear value method | `clearValue()` | `state.Set(default)` |
| Reset value method | `resetValue()` | Not supported (manual reset to initial value) |
| Set hidden method | `setHidden(boolean)` | `.Visible(bool)` property |
| Scroll into view | `scrollIntoView(options)` | Not supported |
| Hierarchical tree structure | Native - single widget handles all levels | Not supported - requires chaining multiple `SelectInput` widgets |
