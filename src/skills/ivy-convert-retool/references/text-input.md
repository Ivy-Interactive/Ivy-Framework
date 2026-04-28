# Text Input

An input field to enter a single line of text. Supports variants like password, email, telephone, URL, search, and multi-line textarea.

## Retool

```toolscript
// Basic text input with default value and placeholder
textInput1.setValue("Hello world!");
textInput1.placeholder = "Enter a value";

// Validation
textInput1.required = true;
textInput1.minLength = 5;
textInput1.maxLength = 100;
textInput1.pattern = "^[a-zA-Z0-9]+$";

// Prefix/suffix
textInput1.textBefore = "$";
textInput1.textAfter = "USD";
textInput1.iconBefore = "/icon:bold/shopping-gift";

// Events
textInput1.events = [{ event: "change", type: "script" }];

// Methods
textInput1.focus();
textInput1.blur();
textInput1.clearValue();
textInput1.resetValue();
textInput1.setDisabled(true);
textInput1.setHidden(true);
```

## Ivy

```csharp
// Basic text input with placeholder
var text = UseState("");
text.ToTextInput().Placeholder("Enter a value");

// Variants
text.ToTextInput().Variant(TextInputVariants.Password);
text.ToTextInput().Variant(TextInputVariants.Email);
text.ToTextInput().Variant(TextInputVariants.Tel);
text.ToTextInput().Variant(TextInputVariants.Url);
text.ToTextInput().Variant(TextInputVariants.Search);
text.ToTextInput().Variant(TextInputVariants.Textarea);

// Or using extension methods
text.ToPasswordInput();
text.ToEmailInput();
text.ToTelInput();
text.ToUrlInput();
text.ToSearchInput();
text.ToTextareaInput();

// Validation
text.ToTextInput().MaxLength(100).Invalid("This field has an error");

// Prefix/suffix
text.ToTextInput().Prefix(Icons.Globe).Suffix(".com");

// Events
text.ToTextInput().Placeholder("Enter text...");

// Disabled
text.ToTextInput().Disabled();

// Keyboard shortcut
text.ToTextInput().ShortcutKey("Ctrl+S");

// With label
text.ToTextInput().Placeholder("Name").WithField().Label("Full Name");
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `value` | Default or current value (`string`) | Bound via `IAnyState` or value + `OnChange` callback |
| `placeholder` | Placeholder text when empty | `.Placeholder("...")` |
| `disabled` | Disable interaction (`boolean`) | `.Disabled()` or `.Disabled(bool)` |
| `readOnly` | Read-only mode (`boolean`) | Not supported |
| `required` | Mark as required (`boolean`) | Not supported (handle via form validation) |
| `minLength` | Minimum character count | Not supported |
| `maxLength` | Maximum character count | `.MaxLength(int)` |
| `enforceMaxLength` | Prevent typing beyond max length | Not supported (MaxLength enforces by default) |
| `pattern` | Regex validation pattern | Not supported (use manual validation in `OnChange`) |
| `patternType` | Built-in patterns: `email`, `url`, `regex` | Use `.Variant(TextInputVariants.Email)` / `.Variant(TextInputVariants.Url)` for type hints |
| `label` | Text label for the input | `.WithField().Label("...")` |
| `labelPosition` | Label position: `top` or `left` | Not supported (handled by `Field` layout) |
| `textBefore` | Prefix text (e.g. `$`) | `.Prefix("...")` or `.Prefix(IWidget)` |
| `textAfter` | Suffix text (e.g. `USD`) | `.Suffix("...")` or `.Suffix(IWidget)` |
| `iconBefore` | Prefix icon | `.Prefix(Icons.Globe)` (via widget prefix) |
| `iconAfter` | Suffix icon | `.Suffix(Icons.Globe)` (via widget suffix) |
| `tooltipText` | Tooltip on label hover | Not supported |
| `inputTooltip` | Helper text below input on focus | Not supported |
| `showClear` | Show clear button | Not supported |
| `showCharacterCount` | Show character count | Not supported |
| `loading` | Show loading indicator | Not supported |
| `autoComplete` | Browser autocomplete | Not supported |
| `autoFill` | Browser autofill type | Not supported |
| `autoCapitalize` | Auto-capitalization mode: `none`, `sentences`, `words`, `characters` | Not supported |
| `spellCheck` | Browser spellcheck | Not supported |
| `style` | Custom CSS styles | Not supported (use `Scale` for sizing) |
| `margin` | Outer margin: `"4px 8px"` or `"0"` | Not supported |
| `isHiddenOnDesktop` | Hide on desktop layout | Not supported |
| `isHiddenOnMobile` | Hide on mobile layout | Not supported |
| `maintainSpaceWhenHidden` | Keep space when hidden | Not supported |
| `showInEditor` | Always show in editor | Not supported |
| `id` | Unique identifier / name (`string`) | Not applicable (C# variable reference) |
| N/A | N/A | `.Variant(TextInputVariants)` — Password, Textarea, Search, Email, Tel, Url |
| N/A | N/A | `.ShortcutKey("Ctrl+S")` — keyboard shortcut to focus |
| N/A | N/A | `.Rows(int)` — number of rows for textarea |
| N/A | N/A | `.Nullable(bool)` — allow null values |
| N/A | N/A | `.Height(Size)` / `.Width(Size)` — sizing |
| N/A | N/A | `.Visible(bool)` — visibility control |
| N/A | N/A | `.Invalid("msg")` — validation error message |

### Methods

| Method | Documentation | Ivy |
|--------|---------------|-----|
| Set value | `textInput.setValue(value)` | `state.Set(value)` |
| Clear value | `textInput.clearValue()` | `state.Set("")` |
| Reset to default | `textInput.resetValue()` | Not supported |
| Focus | `textInput.focus()` | `.ShortcutKey("Ctrl+...")` (indirect) |
| Blur | `textInput.blur()` | Not supported |
| Set disabled | `textInput.setDisabled(bool)` | `.Disabled(bool)` (declarative) |
| Set hidden | `textInput.setHidden(bool)` | `.Visible(bool)` (declarative) |
| Clear validation | `textInput.clearValidation()` | Set `.Invalid("")` to clear |
| Scroll into view | `textInput.scrollIntoView(options)` | Not supported |

### Events

| Event | Documentation | Ivy |
|-------|---------------|-----|
| Change | `change` event handler | `OnChange` callback |
| Blur | `blur` event handler | `OnBlur` / `.HandleBlur()` |
| Focus | `focus` event handler | Not supported |
| Submit | `submit` event handler | Not supported |
