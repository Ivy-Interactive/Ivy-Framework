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
        return new FeedbackInput<int>(rating);
    }
}    
```

## Variants

`FeedbackInput`s come in several variants to suit different use cases:
 For star style feedback ( 1 star to 5 stars) the variant `FeedbackInputs.Stars` should be used.
 For binary style feedback ( yes, no, liked/disliked, recommended/not-recommended) type feedback
 the variant `FeedbackInputs.Thumbs` should be used. `FeedbackInputs.Emojis` should be used
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
                | new FeedbackInput<bool>(thumbsFeedback)
                      .Variant(FeedbackInputs.Thumbs)
                      .WithField()
                      .Label("Did you like the movie ?")
                | new FeedbackInput<int>(starFeedback)
                      .Variant(FeedbackInputs.Stars)
                      .WithField()
                      .Label("How would you like to rate the movie ?")
                | new FeedbackInput<int>(emojiFeedback)
                      .Variant(FeedbackInputs.Emojis)
                      .WithField()
                      .Label("How do you feel after seeing the movie ?");
    }  
}    
```

## Event Handling

The following example shows how change events can be handled for `FeedbackInput`s.

```csharp demo-below
public class FeedbackHandling: ViewBase
{    
    public override object? Build()
    {    
        var feedbackState = UseState(1);
        var exclamation = UseState("");
        exclamation.Set(feedbackState.Value switch
        {
            0 => "No rating yet",
            1 => "Seriously?",
            2 => "Oh! is it that bad?",
            3 => "Ah! you almost liked it!",
            4 => "Cool! Tell me more!",
            5 => "WOW! Would you recommend it?",
            _ => "Invalid rating"
        });
        return Layout.Vertical() 
                | new FeedbackInput<int>(feedbackState)
                | Text.Block(exclamation);
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
                | new FeedbackInput<int>(state)
                      .Disabled()
                      .WithField().Label("Disabled")
                | new FeedbackInput<int>(state)
                      .Invalid("Validation error")
                      .WithField().Label("Invalid");
    }
}        
```

<WidgetDocs Type="Ivy.FeedbackInput" ExtensionTypes="Ivy.FeedbackInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/FeedbackInput.cs"/>
