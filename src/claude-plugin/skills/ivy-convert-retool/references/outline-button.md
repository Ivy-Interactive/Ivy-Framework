# Outline Button

A button to trigger actions when clicked, preconfigured with an outline style variant (border only, no filled background). In Retool this is a preset of the Button component with `styleVariant` set to `outline`. In Ivy, this maps to `Button` with `ButtonVariant.Outline`.

## Retool

```toolscript
// Basic outline button with click handler
outlineButton1.text = "Submit";
outlineButton1.styleVariant = "outline";

// Disable the button programmatically
outlineButton1.setDisabled(true);

// Hide the button programmatically
outlineButton1.setHidden(true);
```

## Ivy

```csharp
new Button("Submit", _ => client.Toast("Submitted!")).Outline()
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| text / title | `text` — the primary text content displayed on the button | `Title` — the display text, set via constructor or property |
| style variant | `styleVariant` — `"solid"` or `"outline"` (default `"outline"`) | `ButtonVariant` — `Primary`, `Secondary`, `Outline`, `Ghost`, `Link`, `Destructive`, `Success`, `Warning`, `Info` |
| disabled | `disabled` — boolean, toggleable via `setDisabled()` | `Disabled` — bool property |
| hidden | `hidden` — boolean, toggleable via `setHidden()` | Not directly named; use conditional rendering |
| loading | `loading` — displays a loading indicator | `Loading` — bool property, shows loading state |
| icon (prefix) | `iconBefore` — prefix icon key | `Icon` — via `.Icon(Icons.X)` or `Icon(Icons.X, Align.Left)` |
| icon (suffix) | `iconAfter` — suffix icon key | `Icon` with `Align.Right` — `.Icon(Icons.X, Align.Right)` |
| click event | `Click` event — configured via event handlers | `OnClick` — delegate passed in constructor or via event |
| horizontal align | `horizontalAlign` — `"left"`, `"center"`, `"right"`, `"stretch"` | Not supported |
| allow wrap | `allowWrap` — whether text can wrap to multiple lines | Not supported |
| aria label | `ariaLabel` — accessible label for screen readers | Not supported |
| margin | `margin` — `"4px 8px"` or `"0"` | Not supported (handled by layout system) |
| loader position | `loaderPosition` — `"auto"`, `"replace"`, `"left"`, `"right"` | Not supported |
| submit | `submit` — whether button submits a form | Not supported (use `OnClick` handler) |
| submit target | `submitTargetId` — the form to submit | Not supported |
| height type | `heightType` — `"fixed"` or `"auto"` | Not supported (handled by layout system) |
| maintain space when hidden | `maintainSpaceWhenHidden` — reserves space when hidden | Not supported |
| show on desktop | `isHiddenOnDesktop` — responsive visibility toggle | Not supported |
| show on mobile | `isHiddenOnMobile` — responsive visibility toggle | Not supported |
| tooltip | Not a direct property (use custom overlay) | `Tooltip` — string property for hover text |
| URL navigation | Not a direct property (use event handler with Open URL action) | `Url` — string property with `.OpenInNewTab()` modifier |
| border radius | Not a direct property (use custom CSS in `style`) | `BorderRadius` — `None`, `Rounded`, `Full` |
| foreground color | Not a direct property (use custom CSS in `style`) | `Foreground` — `Colors?` for text color |
