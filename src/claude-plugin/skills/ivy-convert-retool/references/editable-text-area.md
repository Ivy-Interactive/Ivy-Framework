# Editable Text Area

A click-to-edit input field for entering multiple lines of text. Displays as plain text until the user clicks to edit it, then behaves like a standard text area.

## Retool

```toolscript
editableTextArea1.setValue("Hello, world!")
editableTextArea1.value // "Hello, world!"
editableTextArea1.clearValue()
editableTextArea1.focus()
```

## Ivy

In Ivy the equivalent is a `TextInput` with `Variant = TextInputVariants.Textarea`, or the `ToTextareaInput()` extension method on state. Wrap with `Field` for labels, descriptions, and required indicators.

```csharp
var text = UseState("");

new Field(text.ToTextInput()
    .Placeholder("Enter text here...")
    .Variant(TextInputVariants.Textarea)
    .Rows(4)
    .MaxLength(500))
    .Label("Notes")
    .Description("Enter your notes")
    .Required();

// Or using the extension method:
text.ToTextareaInput(placeholder: "Enter text here...");
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| value | `value` - Current or default value | `Value` - Current input value (or bind via `IAnyState`) |
| placeholder | `placeholder` - Default text when empty | `Placeholder` - Hint text displayed in the field |
| disabled | `disabled` - Disables user interaction | `Disabled` - Enables/disables the input field |
| readOnly | `readOnly` - Prevents user input modification | Not supported (use `Disabled` or `ReadOnlyInput` widget) |
| required | `required` - Makes field mandatory | `Field.Required()` - Marks input as mandatory via `Field` wrapper |
| maxLength | `maxLength` - Maximum character limit | `MaxLength` - Maximum character limit |
| minLength | `minLength` - Minimum character requirement | Not supported |
| showCharacterCount | `showCharacterCount` - Displays character count | Not supported |
| pattern | `pattern` - Regex pattern for validation | `Invalid` - Custom validation via string error message |
| enforceMaxLength | `enforceMaxLength` - Enforces max character length | Not supported (MaxLength enforces by default) |
| label | Label text via `labelPosition` | `Field.Label()` - Label via `Field` wrapper |
| labelPosition | `labelPosition` - Label position (top or left) | Not supported (always top) |
| tooltipText | `tooltipText` - Hover tooltip beside label | `Field.Help()` - Tooltip via `Field` wrapper |
| inputTooltip | `inputTooltip` - Helper text below input on focus | `Field.Description()` - Helper text below input |
| loading | `loading` - Displays loading indicator | Not supported |
| spellCheck | `spellCheck` - Enables browser spell-checking | Not supported |
| autoCapitalize | `autoCapitalize` - Auto capitalization behavior | Not supported |
| autoComplete | `autoComplete` - Enables browser autocomplete | Not supported |
| autoFill | `autoFill` - Data type for browser autofill | Not supported |
| editIcon | `editIcon` - Icon indicating editable field | Not supported |
| isHiddenOnDesktop | `isHiddenOnDesktop` - Toggles desktop visibility | `Visible` - Single visibility toggle |
| isHiddenOnMobile | `isHiddenOnMobile` - Toggles mobile visibility | Not supported (single `Visible` property) |
| maintainSpaceWhenHidden | `maintainSpaceWhenHidden` - Reserves space when hidden | Not supported |
| margin | `margin` - Outer margin spacing | Not supported directly (use layout) |
| style | `style` - Custom styling options | Not supported directly |
| rows | Not a direct property (auto-sized) | `Rows` - Number of visible text rows |
| prefix/suffix | Not supported | `Prefix` / `Suffix` - Static content before/after input |
| shortcutKey | Not supported | `ShortcutKey` - Associated keyboard shortcut |

### Events

| Event | Retool | Ivy |
|-------|--------|-----|
| Change | `Change` - Triggered when value is modified | `OnChange` - Triggered when value is modified |
| Blur | `Blur` - Triggered when input loses focus | `OnBlur` - Triggered when input loses focus |
| Focus | `Focus` - Triggered when input receives focus | Not supported |

### Methods

| Method | Retool | Ivy |
|--------|--------|-----|
| Set value | `setValue(value)` | Bind via `IAnyState` or set `Value` |
| Clear value | `clearValue()` | Set state to `""` |
| Reset value | `resetValue()` | Not supported directly |
| Focus | `focus()` | Not supported |
| Set hidden | `setHidden(boolean)` | Set `Visible` property |
| Clear validation | `clearValidation()` | Set `Invalid` to `null` |
| Scroll into view | `scrollIntoView(options)` | Not supported |
