
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.ThumbsUp, group: ["Widgets", "Inputs"], searchHints: ["rating", "stars", "review", "feedback", "score", "evaluation"])]
public class FeedbackInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var zeroState = UseState(0);
        var twoState = UseState(2);

        var sizeState = UseState(3);
        var sizeBoolState = UseState(true);
        var sizeIntState = UseState(2);

        var decimalState = UseState(3.5m);
        var nullableDecimalState = UseState((decimal?)null);

        var intState = UseState(0);
        var nullableIntState = UseState((int?)null);
        var floatState = UseState(0.0f);
        var nullableFloatState = UseState((float?)null);
        var boolState = UseState(false);
        var nullableBoolState = UseState((bool?)null);

        var onBlurState = UseState(0);
        var onBlurLabel = UseState("");
        var onFocusState = UseState(0);
        var onFocusLabel = UseState("");
        var maxState3 = UseState(0);
        var maxState10 = UseState(0);

        var variants = Layout.Grid().Columns(5)
               | Text.Monospaced("var")
               | Text.Monospaced("rating")
               | Text.Monospaced("state")
               | Text.Monospaced("Disabled")
               | Text.Monospaced("Invalid")

               | Text.Monospaced("FeedbackInputVariant.Stars")
               | zeroState.ToFeedbackInput().Stars()
               | Text.Monospaced(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Stars().Disabled()
               | twoState.ToFeedbackInput().Stars().Invalid("Invalid feedback")

               | Text.Monospaced("FeedbackInputVariant.Emojis")
               | zeroState.ToFeedbackInput().Emojis()
               | Text.Monospaced(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Emojis().Disabled()
               | twoState.ToFeedbackInput().Emojis().Invalid("Invalid feedback")

               | Text.Monospaced("FeedbackInputVariant.Thumbs")
               | zeroState.ToFeedbackInput().Thumbs()
               | Text.Monospaced(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Thumbs().Disabled()
               | twoState.ToFeedbackInput().Thumbs().Invalid("Invalid feedback")
            ;



        var sizeExamples = Layout.Grid().Columns(4)
                          | Text.Monospaced("Variant")
                          | Text.Monospaced("Small")
                          | Text.Monospaced("Default")
                          | Text.Monospaced("Large")

                          | Text.Monospaced("Stars")
                          | sizeState.ToFeedbackInput().Stars().Small()
                          | sizeState.ToFeedbackInput().Stars()
                          | sizeState.ToFeedbackInput().Stars().Large()

                          | Text.Monospaced("Emojis")
                          | sizeIntState.ToFeedbackInput().Emojis().Small()
                          | sizeIntState.ToFeedbackInput().Emojis()
                          | sizeIntState.ToFeedbackInput().Emojis().Large()

                          | Text.Monospaced("Thumbs")
                          | sizeBoolState.ToFeedbackInput().Thumbs().Small()
                          | sizeBoolState.ToFeedbackInput().Thumbs()
                          | sizeBoolState.ToFeedbackInput().Thumbs().Large()
        ;


        var dataBinding = Layout.Grid().Columns(3)
                          | Text.Monospaced("var")
                          | Text.Monospaced("rating")
                          | Text.Monospaced("state")

                          | Text.Monospaced("int")
                          | intState.ToFeedbackInput()
                          | Text.Monospaced(intState.Value.ToString())

                          | Text.Monospaced("int?")
                          | nullableIntState.ToFeedbackInput()
                          | (nullableIntState.Value == null ? Text.Monospaced("null") : Text.Monospaced(nullableIntState.Value.ToString() ?? "null"))

                          | Text.Monospaced("float")
                          | floatState.ToFeedbackInput()
                          | Text.Monospaced(floatState.Value.ToString())

                          | Text.Monospaced("float?")
                          | nullableFloatState.ToFeedbackInput()
                          | (nullableFloatState.Value == null ? Text.Monospaced("null") : Text.Monospaced(nullableFloatState.Value.ToString() ?? "null"))

                          | Text.Monospaced("bool")
                          | boolState.ToFeedbackInput()
                          | (boolState.Value == false ? Text.Monospaced("false") : Text.Monospaced("true"))

                          | Text.Monospaced("bool?")
                          | nullableBoolState.ToFeedbackInput()
                          | (nullableBoolState.Value == null ? Text.Monospaced("null") : (nullableBoolState.Value == false ? Text.Monospaced("false") : Text.Monospaced("true")))
        ;

        var allowHalfExamples = Layout.Grid().Columns(3)
                                | Text.Monospaced("Variant")
                                | Text.Monospaced("rating")
                                | Text.Monospaced("state")

                                | Text.Monospaced("Stars (decimal)")
                                | decimalState.ToFeedbackInput().Variant(FeedbackInputVariant.Stars).AllowHalf()
                                | Text.Monospaced(decimalState.Value.ToString())

                                | Text.Monospaced("Stars (decimal?)")
                                | nullableDecimalState.ToFeedbackInput().Variant(FeedbackInputVariant.Stars).AllowHalf()
                                | (nullableDecimalState.Value == null ? Text.Monospaced("null") : Text.Monospaced(nullableDecimalState.Value.ToString() ?? "null"))

                                | Text.Monospaced("Emojis (decimal)")
                                | decimalState.ToFeedbackInput().Variant(FeedbackInputVariant.Emojis).AllowHalf()
                                | Text.Monospaced(decimalState.Value.ToString())
        ;

        var maxExamples = Layout.Grid().Columns(3)
                          | Text.Monospaced("Max")
                          | Text.Monospaced("rating")
                          | Text.Monospaced("state")

                          | Text.Monospaced("Max(3) Stars")
                          | maxState3.ToFeedbackInput().Variant(FeedbackInputVariant.Stars).Max(3)
                          | Text.Monospaced(maxState3.Value.ToString())

                          | Text.Monospaced("Max(10) Stars")
                          | maxState10.ToFeedbackInput().Variant(FeedbackInputVariant.Stars).Max(10)
                          | Text.Monospaced(maxState10.Value.ToString())

                          | Text.Monospaced("Max(3) Emojis")
                          | maxState3.ToFeedbackInput().Variant(FeedbackInputVariant.Emojis).Max(3)
                          | Text.Monospaced(maxState3.Value.ToString())

                          | Text.Monospaced("Max(10) Emojis")
                          | maxState10.ToFeedbackInput().Variant(FeedbackInputVariant.Emojis).Max(10)
                          | Text.Monospaced(maxState10.Value.ToString())
        ;

        return Layout.Vertical()
               | Text.H1("Feedback Input")
               | Text.H2("Variants")
               | variants
               | Text.H2("Size Examples")
               | sizeExamples
               | Text.H2("Custom Max")
               | maxExamples
               | Text.H2("Allow Half")
               | allowHalfExamples
               | Text.H2("Data Binding")
               | dataBinding
               | Text.H2("Events")
               | (Layout.Vertical()
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The blur event fires when the feedback input loses focus.").Small()
                           | onBlurState.ToFeedbackInput().OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                           | (onBlurLabel.Value != ""
                               ? Callout.Success(onBlurLabel.Value)
                               : Callout.Info("Interact then click away to see blur events"))
                   ).Title("OnBlur Handler")
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The focus event fires when you click on or tab into the feedback input.").Small()
                           | onFocusState.ToFeedbackInput().OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                           | (onFocusLabel.Value != ""
                               ? Callout.Success(onFocusLabel.Value)
                               : Callout.Info("Click or tab into the input to see focus events"))
                   ).Title("OnFocus Handler")
               )

            ;

    }
}
