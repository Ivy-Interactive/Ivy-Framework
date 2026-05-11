# Link

A semantic element for navigation between pages. Renders as an anchor (`<a>`) tag that navigates to internal or external URLs when clicked.

## Reflex

```python
import reflex as rx

rx.link("Reflex Home Page.", href="https://reflex.dev/")
rx.link("Docs", href="/docs/library")
rx.link("Anchor", href="/docs/library/typography/link#example")
```

## Ivy

Ivy has no dedicated Link widget. The closest equivalent is `Button` with the `Link` variant and `.Url()` method, which renders a link-styled button that navigates to a URL.

```csharp
new Button("Ivy Home Page.").Url("https://docs.ivy.app").Variant(ButtonVariant.Link)
new Button("External").Url("https://example.com").OpenInNewTab().Variant(ButtonVariant.Link)
```

For programmatic navigation (e.g. in response to events), use the `UseNavigation()` hook:

```csharp
var navigator = UseNavigation();
navigator.Navigate("https://docs.ivy.app");
navigator.Navigate(typeof(AnotherApp));
```

## Parameters

| Parameter        | Documentation                                          | Ivy                                                  |
|------------------|--------------------------------------------------------|------------------------------------------------------|
| `href`           | URL the link navigates to                              | `.Url("...")` on Button                              |
| `is_external`    | Opens link in new tab                                  | `.OpenInNewTab()` on Button                          |
| `size`           | Controls text size (`"1"` through `"9"`)               | `.Small()` / `.Medium()` / `.Large()` on Button      |
| `weight`         | Font weight (`"light"`, `"regular"`, `"bold"`, etc.)   | Not supported                                        |
| `underline`      | Underline visibility (`"auto"`, `"hover"`, `"always"`) | Not supported (styled via variant)                   |
| `color_scheme`   | Color theme (`"tomato"`, `"red"`, `"blue"`, etc.)      | Not supported (use `.Success()` / `.Warning()` etc.) |
| `high_contrast`  | Increases contrast with background                     | Not supported                                        |
| `trim`           | Trims leading whitespace at start/end                  | Not supported                                        |
| `as_child`       | Render as child element instead of anchor              | Not supported                                        |
