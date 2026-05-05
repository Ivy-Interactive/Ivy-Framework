# Editable Text

A click-to-edit single-line input field for text values. It renders as plain text until clicked, at which point it becomes an editable input field. Functions similarly to a Text Input but with an inline-editing UX.

## Retool

```toolscript
editableText1.value = "Hello world!";
editableText1.placeholder = "Enter a value";
editableText1.readOnly = false;
editableText1.required = true;
editableText1.maxLength = 100;
editableText1.pattern = "^[a-zA-Z0-9]+$";
editableText1.textBefore = "$";
editableText1.textAfter = "USD";
editableText1.iconBefore = "/icon:bold/shopping-gift";
editableText1.showClear = true;

// Methods
editableText1.setValue("New value");
editableText1.clearValue();
editableText1.resetValue();
editableText1.focus();
editableText1.clearValidation();
```

## Ivy

Ivy does not have a dedicated click-to-edit component. The closest equivalent is `TextInput`, which is always in an editable state. A click-to-edit behavior would need to be composed manually using state toggling between a `TextBlock` and a `TextInput`.

```csharp
public class EditableTextDemo : ViewBase
{
    public override object? Build()
    {
        var text = UseState("Hello world!");
        return text.ToTextInput()
                   .Placeholder("Enter a value")
                   .MaxLength(100)
                   .Prefix("$")
                   .Suffix("USD");
    }
}
```

## Parameters

| Parameter            | Documentation                                                          | Ivy                                                    |
|----------------------|------------------------------------------------------------------------|--------------------------------------------------------|
| `value`              | The current or default value.                                          | `UseState` + constructor binding                       |
| `inputValue`         | The default or most recently entered value (read-only).                | `UseState.Value`                                       |
| `placeholder`        | Text displayed when there is no value.                                 | `.Placeholder()`                                       |
| `readOnly`           | Whether user input is read-only.                                       | `.Disabled()` (no separate read-only)                  |
| `required`           | Whether a value is required.                                           | Not supported (use manual validation with `.Invalid()`) |
| `maxLength`          | Maximum number of characters allowed.                                  | `.MaxLength()`                                         |
| `minLength`          | Minimum number of characters allowed.                                  | Not supported                                          |
| `enforceMaxLength`   | Whether to enforce the maximum length.                                 | Not supported (MaxLength always enforces)              |
| `pattern`            | Regex to validate input.                                               | Not supported (use manual validation with `.Invalid()`) |
| `patternType`        | Pattern type (email, regex, url).                                      | `.Variant()` for Email/Url types                       |
| `textBefore`         | Prefix text to display.                                                | `.Prefix("text")`                                      |
| `textAfter`          | Suffix text to display.                                                | `.Suffix("text")`                                      |
| `iconBefore`         | Prefix icon to display.                                                | `.Prefix(Icons.Name)`                                  |
| `iconAfter`          | Suffix icon to display.                                                | `.Suffix(Icons.Name)`                                  |
| `editIcon`           | Icon indicating an editable value.                                     | Not supported                                          |
| `labelPosition`      | Position of the label (top or left).                                   | `.WithField().Label()` (layout-dependent)              |
| `tooltipText`        | Tooltip text displayed on hover.                                       | Not supported                                          |
| `inputTooltip`       | Helper text displayed below input on focus.                            | Not supported                                          |
| `loading`            | Whether to display a loading indicator.                                | Not supported                                          |
| `showClear`          | Whether to display a clear button.                                     | Not supported                                          |
| `autoCapitalize`     | Automatic capitalization type for virtual keyboards.                   | Not supported                                          |
| `autoComplete`       | Whether the browser can autocomplete.                                  | Not supported                                          |
| `autoFill`           | Data type for browser autofill.                                        | Not supported                                          |
| `spellCheck`         | Whether the browser can spellcheck.                                    | Not supported                                          |
| `margin`             | Margin around the component.                                           | Not supported (use layout)                             |
| `style`              | Custom style options.                                                  | Not supported                                          |
| `isHiddenOnDesktop`  | Whether hidden on desktop.                                             | `.Visible()` (not platform-specific)                   |
| `isHiddenOnMobile`   | Whether hidden on mobile.                                              | Not supported                                          |
| `maintainSpaceWhenHidden` | Whether to reserve space when hidden.                             | Not supported                                          |
| `showInEditor`       | Whether visible in editor when hidden.                                 | Not supported                                          |
| **Events**           |                                                                        |                                                        |
| `Change`             | Triggered when the value changes.                                      | `OnChange`                                             |
| `Blur`               | Triggered when the input is deselected.                                | `OnBlur` / `HandleBlur`                                |
| `Focus`              | Triggered when the input is selected.                                  | Not supported                                          |
| **Methods**          |                                                                        |                                                        |
| `setValue()`         | Set the current value.                                                 | `UseState.Set()`                                       |
| `clearValue()`       | Clear the current value.                                               | `UseState.Set("")`                                     |
| `resetValue()`       | Reset to default value.                                                | Not supported (manual reset via state)                 |
| `focus()`            | Set focus on the component.                                            | `.ShortcutKey()` (indirect)                            |
| `clearValidation()`  | Clear the validation message.                                          | `.Invalid("")` (manual clear)                          |
| `scrollIntoView()`   | Scroll the component into view.                                        | Not supported                                          |
