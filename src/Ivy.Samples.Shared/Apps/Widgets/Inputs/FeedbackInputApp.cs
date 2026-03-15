
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
