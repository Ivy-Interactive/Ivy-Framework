# st.feedback

Display an icon-based button group for collecting user feedback. Commonly used in chat and AI applications to let users rate responses. Returns an integer index (0-based) or None if unselected.

## Streamlit

```python
import streamlit as st

sentiment_mapping = ["one", "two", "three", "four", "five"]
selected = st.feedback("stars")
if selected is not None:
    st.markdown(f"You selected {sentiment_mapping[selected]} star(s).")
```

## Ivy

```csharp
public class BasicFeedbackDemo : ViewBase
{
    public override object? Build()
    {
        var starFeedback = UseState(3);
        var thumbsFeedback = UseState(true);
        var emojiFeedback = UseState(4);

        return Layout.Vertical()
            | starFeedback.ToFeedbackInput()
                  .Variant(FeedbackInputs.Stars)
                  .WithField()
                  .Label("How would you rate this?")
            | thumbsFeedback.ToFeedbackInput()
                  .Variant(FeedbackInputs.Thumbs)
                  .WithField()
                  .Label("Did you like it?")
            | emojiFeedback.ToFeedbackInput()
                  .Variant(FeedbackInputs.Emojis)
                  .WithField()
                  .Label("How do you feel?");
    }
}
```

## Parameters

| Parameter   | Documentation                                                                 | Ivy                                                        |
|-------------|-------------------------------------------------------------------------------|------------------------------------------------------------|
| options     | Feedback style: `"thumbs"`, `"faces"`, or `"stars"` (default `"thumbs"`)      | `.Variant(FeedbackInputs.Stars/Thumbs/Emojis)`             |
| default     | Initial selected value (int or None)                                          | Initial value passed via `UseState(value)`                  |
| disabled    | Disables the widget when `True`                                               | `.Disabled()`                                               |
| on_change   | Callback when value changes                                                   | `OnChange` event or reactive state via `UseState`           |
