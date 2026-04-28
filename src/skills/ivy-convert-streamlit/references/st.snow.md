# st.snow

Draws celebratory snowfall across the app. A fun, visual-only effect with no configuration options. Ivy has no direct snow equivalent, but the `Confetti` effect serves the same celebratory purpose with more control over triggering.

## Streamlit

```python
import streamlit as st

st.snow()
```

## Ivy

```csharp
// Auto-trigger confetti on render (closest to st.snow)
Text.Block("Welcome!")
    .WithConfetti(AnimationTrigger.Auto)

// Confetti on button click
new Button("Celebrate!")
    .WithConfetti(AnimationTrigger.Click)
```

## Parameters

| Parameter | Documentation                                         | Ivy                                                                 |
|-----------|-------------------------------------------------------|---------------------------------------------------------------------|
| -         | `st.snow()` accepts no parameters                     | `Trigger` controls activation: `Auto`, `Click`, or `Hover`         |
| -         | Effect always plays immediately on call               | `Visible` (bool) controls whether the effect is shown              |
| -         | No child/wrapper concept; applies to the entire page  | `child` wraps a specific widget with the confetti effect            |
