using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.ThumbsUp, path: ["Widgets", "Inputs"])]
public class FeedbackInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var zeroState = UseState(0);
        var twoState = UseState(2);

        var variants = Layout.Grid().Columns(5)
               | Text.InlineCode("var")
               | Text.InlineCode("rating")
               | Text.InlineCode("state")
               | Text.InlineCode("Disabled")
               | Text.InlineCode("Invalid")

               | Text.InlineCode("FeedbackInputs.Stars")
               | zeroState.ToFeedbackInput().Variant(FeedbackInputs.Stars)
               | Text.InlineCode(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Variant(FeedbackInputs.Stars).Disabled()
               | twoState.ToFeedbackInput().Variant(FeedbackInputs.Stars).Invalid("Invalid feedback")

               | Text.InlineCode("FeedbackInputs.Emojis")
               | zeroState.ToFeedbackInput().Variant(FeedbackInputs.Emojis)
               | Text.InlineCode(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Variant(FeedbackInputs.Emojis).Disabled()
               | twoState.ToFeedbackInput().Variant(FeedbackInputs.Emojis).Invalid("Invalid feedback")

               | Text.InlineCode("FeedbackInputs.Thumbs")
               | zeroState.ToFeedbackInput().Variant(FeedbackInputs.Thumbs)
               | Text.InlineCode(zeroState.Value.ToString())
               | twoState.ToFeedbackInput().Variant(FeedbackInputs.Thumbs).Disabled()
               | twoState.ToFeedbackInput().Variant(FeedbackInputs.Thumbs).Invalid("Invalid feedback")
            ;

        var intState = UseState(0);
        var nullableIntState = UseState((int?)null);
        var floatState = UseState(0.0f);
        var nullableFloatState = UseState((float?)null);
        var boolState = UseState(false);
        var nullableBoolState = UseState((bool?)null);
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
               | Text.H2("Data Binding")
               | dataBinding

            ;

    }
}