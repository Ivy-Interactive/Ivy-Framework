# st.badge

Display a colored badge with an icon and label. Useful for showing statuses, counts, or short labels.

## Streamlit

```python
import streamlit as st

st.badge("New")
st.badge("Success", icon=":material/check:", color="green")

st.markdown(
    ":violet-badge[:material/star: Favorite] :orange-badge[⚠️ Needs review] :gray-badge[Deprecated]"
)
```

## Ivy

```csharp
new Badge("New");
new Badge("Success", icon: Icons.Check).Success();

new Badge("Favorite", icon: Icons.Star);
new Badge("Needs review", BadgeVariant.Warning);
new Badge("Deprecated").Secondary();
```

## Parameters

| Parameter | Streamlit                                                                                              | Ivy                                                                                   |
|-----------|--------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|
| label     | `str` — Text displayed in the badge. Supports GitHub-flavored Markdown.                                | `string title` — Badge text content.                                                  |
| icon      | `str` or `None` — Emoji or Material Symbol (e.g. `:material/check:`). Default `None`.                 | `Icons?` — Icon enum (e.g. `Icons.Bell`). Supports `IconPosition` via `Align`.        |
| color     | `str` — One of red, orange, yellow, blue, green, violet, gray/grey, primary. Default `"blue"`.         | `BadgeVariant` — Primary, Destructive, Outline, Secondary, Success, Warning, Info.    |
| help      | `str` or `None` — Tooltip on hover with GitHub-flavored Markdown support. Default `None`.              | Not supported                                                                         |
