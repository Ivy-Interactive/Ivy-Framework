# Rating

An input field to select a rating. Allows users to provide feedback through a visual rating interface using stars, hearts, or faces.

## Retool

```toolscript
// Star rating with half values
rating1.setValue(4.5);
rating1.allowHalf = true;
rating1.max = 5;
rating1.icons = "stars"; // "stars" | "hearts" | "faces"
```

## Ivy

```csharp
public class RatingDemo : ViewBase
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
                  .Label("Star Rating")
            | thumbsFeedback.ToFeedbackInput()
                  .Variant(FeedbackInputs.Thumbs)
                  .WithField()
                  .Label("Thumbs Up/Down")
            | emojiFeedback.ToFeedbackInput()
                  .Variant(FeedbackInputs.Emojis)
                  .WithField()
                  .Label("Emoji Feedback");
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `value` | Current rating value | `UseState(3)` passed to `FeedbackInput<int>(state)` |
| `allowHalf` | Permits selection of half-value ratings | Not supported |
| `max` | Maximum allowable rating value | Not supported (fixed 1-5 for Stars, binary for Thumbs) |
| `icons` | Display type: `stars`, `hearts`, `faces` | `.Variant(FeedbackInputs.Stars \| Thumbs \| Emojis)` |
| `disabled` | Disables user interaction | `.Disabled()` |
| `required` | Mandates a value selection | `.Invalid("message")` for validation |
| `size` | Display size: `default` or `small` | Not supported |
| `labelPosition` | Label placement: `top` or `left` | `.WithField().Label("text")` |
| `tooltipText` | Helper text shown on hover | Not supported |
| `margin` | Outer spacing of the component | Not supported |
| `isHiddenOnDesktop` | Desktop visibility toggle | `.Visible` property (not desktop-specific) |
| `isHiddenOnMobile` | Mobile visibility toggle | `.Visible` property (not mobile-specific) |
| `onChange` | Event triggered when value changes | `OnChange` event / state-based reactivity |
| `clearValue()` | Resets the current rating | Set state to default value |
| `setValue(value)` | Updates rating programmatically | `state.Set(value)` |
| `validate()` | Triggers validation check | `.Invalid("message")` |
| `focus()` | Sets keyboard focus to component | Not supported |
