# Phone Number Input

An input field designed to accept phone number entries. Supports country code selection, formatting, and validation of phone numbers.

## Retool

```toolscript
phoneNumberInput.setValue("+1 (555) 123-4567");
phoneNumberInput.validate();

// Read formatted value
const formatted = phoneNumberInput.formattedValue;

// Events: Change, Focus, Blur, Submit
```

## Ivy

```csharp
// Using ToTelInput extension method
var phone = state.ToTelInput(placeholder: "Enter phone number");

// Or using the TextInput constructor with Tel variant
var phone = state.ToTextInput().Placeholder("Enter phone number").Variant(TextInputVariants.Tel);
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Value | `value` - current phone number string | `Value` - current string value |
| Disabled | `disabled` - disables input | `Disabled` - disables input |
| Placeholder | N/A (configured via label/tooltip) | `Placeholder` - placeholder text |
| Required | `required` - value must be entered | Not supported (use `Invalid` for validation) |
| Read Only | `readOnly` - prevents editing | Not supported |
| Show Clear | `showClear` - displays clear button | `Nullable` - allows clearing the value |
| Locked Country Code | `lockedCountryCode` - locks country code | Not supported |
| Loading | `loading` - shows loading indicator | Not supported |
| Label Position | `labelPosition` - top or left | Not supported (use `Field` wrapper) |
| Icon Before | `iconBefore` - prefix icon | `Prefix` - prefix affix |
| Icon After | `iconAfter` - suffix icon | `Suffix` - suffix affix |
| Text Before | `textBefore` - prefix text | `Prefix` - prefix affix |
| Text After | `textAfter` - suffix text | `Suffix` - suffix affix |
| Input Tooltip | `inputTooltip` - helper text on focus | Not supported |
| Tooltip Text | `tooltipText` - label hover tooltip | Not supported |
| Hidden Desktop | `isHiddenOnDesktop` - hide on desktop | `Visible` - controls visibility |
| Hidden Mobile | `isHiddenOnMobile` - hide on mobile | Not supported |
| Margin | `margin` - outer margin | Not supported |
| Style | `style` - custom styling | Not supported |
| Formatted Value | `formattedValue` - formatted output (read-only) | Not supported |
| Parsed Value | `parsedValue` - parsed phone object (read-only) | Not supported |
| Max Length | N/A | `MaxLength` - max character length |
| Variant | N/A (dedicated component) | `Variant` - `TextInputVariants.Tel` |
| Shortcut Key | N/A | `ShortcutKey` - keyboard shortcut |
| Validation | `validate()` / `clearValidation()` methods | `Invalid` - validation message string |
| On Change | `Change` event | `OnChange` event |
| On Focus | `Focus` event | Not supported |
| On Blur | `Blur` event | `OnBlur` event |
| On Submit | `Submit` event | Not supported |
| Set Value | `setValue()` method | Bind via `IAnyState` |
| Clear Value | `clearValue()` method | Set state to null (`Nullable`) |
| Focus | `focus()` method | Not supported |
| Reset Value | `resetValue()` method | Not supported |
| Scroll Into View | `scrollIntoView()` method | Not supported |
| Width | N/A | `Width` - component width |
| Height | N/A | `Height` - component height |
| Scale | N/A | `Scale` - component scale |
| Rows | N/A | `Rows` - textarea rows (not applicable for Tel) |
