---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - rating
  - stars
  - review
  - feedback
  - score
  - evaluation
---

# FeedbackInput

<Ingress>
Collect user feedback with combined rating and comment inputs, perfect for surveys, reviews, and [feedback forms](../../01_Onboarding/02_Concepts/08_Forms.md).
</Ingress>

The `FeedbackInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a specialized input for collecting user feedback. It typically includes rating options and a text field for comments, making it ideal for surveys, reviews, and feedback forms.

## Basic Usage

Here's a simple example of a `FeedbackInput` with the default `Stars` variant:

```csharp demo-below
public class BasicFeedbackDemo : ViewBase
{
    public override object? Build()
    {
        var rating = UseState(3);
        return rating.ToFeedbackInput();
    }
}
```

## Variants

`FeedbackInput`s come in several variants to suit different use cases:
For star style feedback ( 1 star to 5 stars) the variant `FeedbackInputVariant.Stars` should be used.
For binary style feedback ( yes, no, liked/disliked, recommended/not-recommended) type feedback
the variant `FeedbackInputVariant.Thumbs` should be used. `FeedbackInputVariant.Emojis` should be used
for collecting sentiment analysis feedbacks about anything.

```csharp demo-below
public class FeedbackDemo : ViewBase
{
    public override object? Build()
    {
        // Initial guess feedbacks
        var starFeedback = UseState(3);
        var thumbsFeedback = UseState(true);
        var emojiFeedback = UseState(4);
        return Layout.Vertical()
                | H3 ("Simple movie review")
                | thumbsFeedback.ToFeedbackInput()
                      .Variant(FeedbackInputVariant.Thumbs)
                      .WithField()
                      .Label("Did you like the movie ?")
                | starFeedback.ToFeedbackInput()
                      .Variant(FeedbackInputVariant.Stars)
                      .WithField()
                      .Label("How would you like to rate the movie ?")
                | emojiFeedback.ToFeedbackInput()
                      .Variant(FeedbackInputVariant.Emojis)
                      .WithField()
                      .Label("How do you feel after seeing the movie ?");
    }
}
```

## Half-value Ratings

The `AllowHalf` property enables users to select half-star or half-emoji ratings by clicking on the left or right side of a rating item. This is particularly useful for star ratings where 3.5 or 4.5 stars provide more granular feedback.

```csharp demo-below
public class HalfValueFeedbackDemo : ViewBase
{
    public override object? Build()
    {
        var rating = UseState(3.5m);
        return rating.ToFeedbackInput().AllowHalf();
    }
}
```

## Custom Maximum Rating

By default, `FeedbackInput` shows 5 rating items (stars, emojis, etc.). You can customize this using the `Max` property.

```csharp demo-below
public class CustomMaxFeedbackDemo : ViewBase
{
    public override object? Build()
    {
        var rating = UseState(0);
        return Layout.Vertical()
                | Text.Block("Rate out of 3:")
                | rating.ToFeedbackInput().Max(3)
                | Text.Block("Rate out of 10:")
                | rating.ToFeedbackInput().Max(10);
    }
}
```

## Styling and Customization

`FeedbackInput`s can be customized with various styling options, including `Disabled` and `Invalid` states:

```csharp demo-below
public class StyledFeedbackDemo : ViewBase
{
    public override object? Build()
    {
        var state = UseState(3);
        return Layout.Vertical()
                | state.ToFeedbackInput()
                      .Disabled()
                      .WithField().Label("Disabled")
                | state.ToFeedbackInput()
                      .Invalid("Validation error")
                      .WithField().Label("Invalid");
    }
}
```

## Event Handling

Feedback inputs support focus, blur, and manual `AutoFocus` behavior.

```csharp demo-tabs
public class FeedbackInputEventsDemo : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var state = UseState(3);

        return Layout.Tabs(
            new Tab("OnFocus", Layout.Vertical()
                | Text.P("The OnFocus event fires when the feedback input gains focus.")
                | state.ToFeedbackInput()
                    .OnFocus(() => focusCount.Set(focusCount.Value + 1))
                | Text.Literal($"Focus Count {focusCount.Value}")
            ),
            new Tab("OnBlur", Layout.Vertical()
                | Text.P("The OnBlur event fires when the feedback input loses focus.")
                | state.ToFeedbackInput()
                    .OnBlur(() => blurCount.Set(blurCount.Value + 1))
                | Text.Literal($"Blur Count {blurCount.Value}")
            ),
            new Tab("AutoFocus", Layout.Vertical()
                | Text.P("The AutoFocus property automatically focuses the widget upon mounting.")
                | state.ToFeedbackInput().AutoFocus()
                | Text.Lead("Focused!")
            )
        ).Variant(TabsVariant.Tabs);
    }
}
```

<WidgetDocs Type="Ivy.FeedbackInput" ExtensionTypes="Ivy.FeedbackInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/FeedbackInput.cs"/>
