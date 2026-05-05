# Markdown

Renders Markdown text as formatted HTML with support for GitHub Flavored Markdown, syntax highlighting, math equations, tables, images, and more.

## Reflex

```python
import reflex as rx

rx.markdown(
    "# Hello World!\n\n"
    "Support us on [Github](https://github.com/reflex-dev/reflex).\n\n"
    "Use `reflex deploy` to deploy your app with **a single command**."
)
```

## Ivy

```csharp
var markdown = new Markdown(
    "# Hello World!\n\n"
    + "Support us on [Github](https://github.com/reflex-dev/reflex).\n\n"
    + "Use `reflex deploy` to deploy your app with **a single command**."
);
```

## Parameters

| Parameter          | Documentation                                              | Ivy                                          |
|--------------------|------------------------------------------------------------|----------------------------------------------|
| content            | The markdown text to render                                | `Content` property (also constructor arg)    |
| use_gfm            | Enable GitHub Flavored Markdown (remark-gfm)               | Enabled by default                           |
| use_math           | Enable math equation rendering (remark-math)                | Supported via KaTeX                          |
| use_katex          | Enable LaTeX rendering (rehype-katex)                       | Supported via KaTeX                          |
| use_unwrap_images  | Remove paragraph tags around images (rehype-unwrap-images)  | Not supported                                |
| use_raw            | Allow raw HTML rendering (rehype-raw)                       | HTML tags supported by default               |
| remark_plugins     | List of remark plugins for extending parsing                | Not supported                                |
| rehype_plugins     | List of rehype plugins for post-processing                  | Not supported                                |
| component_map      | Map markdown elements to custom components                  | Not supported                                |
| on_link_click      | Not supported                                              | `HandleLinkClick` event handler              |
| width              | CSS width                                                  | `Width` property                             |
| height             | CSS height                                                 | `Height` property                            |
| scale              | Not supported                                              | `Scale` property                             |
| visible            | Not supported                                              | `Visible` property                           |
