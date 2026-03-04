using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.ThumbsUp, path: ["Widgets", "Inputs"], searchHints: ["rating", "stars", "review", "feedback", "score", "evaluation"])]
public class FeedbackInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var zeroState = UseState(0);
        var twoState = UseState(2);

        var sizeState = UseState(3);
        var sizeBoolState = UseState(true);
        var sizeIntState = UseState(2);

        var intState = UseState(0);
        var nullableIntState = UseState((int?)null);
        var floatState = UseState(0.0f);
        var nullableFloatState = UseState((float?)null);
        var boolState = UseState(false);
        var nullableBoolState = UseState((bool?)null);

        var variants = Layout.Grid().Columns(5)
               | Text.InlineCode("var")
               | Text.InlineCode("rating")
               | Text.InlineCode("state")
               | Text.InlineCode("Disabled")
               | Text.InlineCode("Invalid")

               | Text.InlineCode("FeedbackInputVariants.Stars")
               | zeroState.ToFeedbackInput().Variant(FeedbackInputVariants.Stars)
               | Text.InlineCode(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Variant(FeedbackInputVariants.Stars).Disabled()
               | twoState.ToFeedbackInput().Variant(FeedbackInputVariants.Stars).Invalid("Invalid feedback")

               | Text.InlineCode("FeedbackInputVariants.Emojis")
               | zeroState.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis)
               | Text.InlineCode(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis).Disabled()
               | twoState.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis).Invalid("Invalid feedback")

               | Text.InlineCode("FeedbackInputVariants.Thumbs")
               | zeroState.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs)
               | Text.InlineCode(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs).Disabled()
               | twoState.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs).Invalid("Invalid feedback")
            ;



        var sizeExamples = Layout.Grid().Columns(4)
                          | Text.InlineCode("Variant")
                          | Text.InlineCode("Small")
                          | Text.InlineCode("Default")
                          | Text.InlineCode("Large")

                          | Text.InlineCode("Stars")
                          | sizeState.ToFeedbackInput().Variant(FeedbackInputVariants.Stars).Small()
                          | sizeState.ToFeedbackInput().Variant(FeedbackInputVariants.Stars)
                          | sizeState.ToFeedbackInput().Variant(FeedbackInputVariants.Stars).Large()

                          | Text.InlineCode("Emojis")
                          | sizeIntState.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis).Small()
                          | sizeIntState.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis)
                          | sizeIntState.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis).Large()

                          | Text.InlineCode("Thumbs")
                          | sizeBoolState.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs).Small()
                          | sizeBoolState.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs)
                          | sizeBoolState.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs).Large()
        ;


        var dataBinding = Layout.Grid().Columns(3)
                          | Text.InlineCode("var")
                          | Text.InlineCode("rating")
                          | Text.InlineCode("state")

                          | Text.InlineCode("int")
                          | intState.ToFeedbackInput()
                          | Text.InlineCode(intState.Value.ToString())

                          | Text.InlineCode("int?")
                          | nullableIntState.ToFeedbackInput()
                          | (nullableIntState.Value == null ? Text.InlineCode("null") : Text.InlineCode(nullableIntState.Value.ToString() ?? "null"))

                          | Text.InlineCode("float")
                          | floatState.ToFeedbackInput()
                          | Text.InlineCode(floatState.Value.ToString())

                          | Text.InlineCode("float?")
                          | nullableFloatState.ToFeedbackInput()
                          | (nullableFloatState.Value == null ? Text.InlineCode("null") : Text.InlineCode(nullableFloatState.Value.ToString() ?? "null"))

                          | Text.InlineCode("bool")
                          | boolState.ToFeedbackInput()
                          | (boolState.Value == false ? Text.InlineCode("false") : Text.InlineCode("true"))

                          | Text.InlineCode("bool?")
                          | nullableBoolState.ToFeedbackInput()
                          | (nullableBoolState.Value == null ? Text.InlineCode("null") : (nullableBoolState.Value == false ? Text.InlineCode("false") : Text.InlineCode("true")))
        ;

        return Layout.Vertical()
               | Text.H1("Feedback Inputs")
               | Text.H2("Variants")
               | variants
               | Text.H2("Size Examples")
               | sizeExamples
               | Text.H2("Data Binding")
               | dataBinding

            ;

    }
}