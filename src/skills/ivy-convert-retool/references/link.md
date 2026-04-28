# Link

A clickable text link used to trigger actions or navigate to URLs. Displays as styled text with optional icons, underline, and tooltip.

## Retool

```toolscript
Link {
  text: "Visit Documentation"
  showUnderline: "hover"
  underlineStyle: "solid"
  iconBefore: "bold/interface-arrows-right"
  tooltipText: "Opens docs in a new tab"
  disabled: false
  loading: false
  horizontalAlign: "left"
  Event handlers: [{ event: "click", method: "openUrl", options: { url: "https://example.com", newTab: true } }]
}
```

## Ivy

```csharp
new Button("Visit Documentation")
    .Link()
    .Url("https://example.com")
    .OpenInNewTab()
    .Icon(Icons.ArrowRight)
    .Tooltip("Opens docs in a new tab")
```

## Parameters

| Parameter        | Documentation                                              | Ivy                                                                 |
|------------------|------------------------------------------------------------|---------------------------------------------------------------------|
| text             | Primary text content of the link                           | `Title` property / constructor parameter                            |
| disabled         | Disables interaction when true                             | `.Disabled()` / `Disabled` property                                 |
| loading          | Shows a loading indicator                                  | `.Loading()` / `Loading` property                                   |
| iconBefore       | Prefix icon to display                                     | `.Icon(Icons.X)` (default position is left)                         |
| iconAfter        | Suffix icon to display                                     | `.Icon(Icons.X, Align.Right)`                                       |
| tooltipText      | Hover tooltip text (supports Markdown in Retool)           | `.Tooltip(string)` / `Tooltip` property                             |
| showUnderline    | Underline behavior: always, hover, never                   | Not supported (use `.Link()` variant for link styling)              |
| underlineStyle   | Underline appearance: solid, dashed, dotted                | Not supported                                                       |
| horizontalAlign  | Content alignment: left, center, right                     | Not supported (use parent layout for alignment)                     |
| allowWrap        | Whether content can wrap to multiple lines                 | Not supported                                                       |
| margin           | External spacing around the component                      | Not supported (use parent layout for spacing)                       |
| isHiddenOnDesktop| Hides the component on desktop                             | `Visible` property (no per-breakpoint control)                      |
| isHiddenOnMobile | Hides the component on mobile                              | `Visible` property (no per-breakpoint control)                      |
| loaderPosition   | Loading indicator position: auto, replace, left, right     | Not supported                                                       |
| **Events**       |                                                            |                                                                     |
| Click            | Triggered when the link is clicked                         | `OnClick` handler via constructor or event                          |
| **Methods**      |                                                            |                                                                     |
| scrollIntoView   | Scrolls the component into the visible area                | Not supported                                                       |
| setDisabled      | Programmatically toggles disabled state                    | Set `Disabled` property directly                                    |
| setHidden        | Programmatically toggles visibility                        | Set `Visible` property directly                                     |
| **Ivy-only**     |                                                            |                                                                     |
| —                | —                                                          | `Url` property — navigates to a URL on click                        |
| —                | —                                                          | `.OpenInNewTab()` — sets `Target` to open in new tab                |
| —                | —                                                          | `Variant` — `ButtonVariant.Link` renders as a text link             |
| —                | —                                                          | `BorderRadius` — configurable corner radius                         |
| —                | —                                                          | `Foreground` — custom text/icon color                               |
| —                | —                                                          | `Scale` — size scaling                                              |
