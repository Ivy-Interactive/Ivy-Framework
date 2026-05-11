# st.balloons

Draws celebratory balloons across the app. Used to celebrate achievements or milestones. The closest Ivy equivalent is the **Confetti** effect, which adds celebratory confetti to any widget with customizable triggers.

## Streamlit

```python
import streamlit as st

st.balloons()
```

## Ivy

```csharp
Text.Block("Welcome!")
    .WithConfetti(AnimationTrigger.Auto)
```

## Parameters

| Parameter | Documentation                                      | Ivy                                                                  |
|-----------|----------------------------------------------------|----------------------------------------------------------------------|
| trigger   | Not supported (always plays immediately on render) | `AnimationTrigger` — `Auto`, `Click`, or `Hover`. Default is `Auto`. |
