# st.success

Display a success message. Shows a green callout box to communicate positive outcomes or completed actions.

## Streamlit

```python
import streamlit as st

st.success("This is a success message!", icon="✅")
```

## Ivy

```csharp
new Callout("This is a success message!", variant: CalloutVariant.Success, icon: Icons.CheckCircle)
```

## Parameters

| Parameter | Documentation                                                                                           | Ivy                                                                |
|-----------|---------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| body      | The success message to display. Supports GitHub-flavored Markdown and additional Markdown directives.   | `description` parameter in the Callout constructor                 |
| icon      | An optional emoji or Material Symbol icon displayed next to the alert. Default: `None`.                 | `icon` parameter of type `Icons?`. Overrides the default variant icon. |
