# st.info

Display an informational message styled as a callout box, typically used to convey non-critical information to users.

## Streamlit

```python
import streamlit as st

st.info('This is a purely informational message', icon="ℹ️")
```

## Ivy

```csharp
Callout.Info("This is a purely informational message");
```

## Parameters

| Parameter | Documentation                                                                 | Ivy                                                            |
|-----------|-------------------------------------------------------------------------------|----------------------------------------------------------------|
| body      | The info text. Can use markdown.                                              | `description` parameter                                        |
| icon      | An emoji or Material Symbols icon to display next to the alert. E.g. `"ℹ️"`. | `icon` parameter (uses `Icons` enum instead of emoji strings)  |
| —         | —                                                                             | `title` parameter — optional heading for the callout           |
| —         | —                                                                             | `variant` parameter — `Info`, `Success`, `Warning`, or `Error` |
