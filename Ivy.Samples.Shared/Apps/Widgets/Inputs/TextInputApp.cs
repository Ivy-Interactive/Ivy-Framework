using System.Security.Cryptography;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.TextCursorInput, path: ["Widgets", "Inputs"], searchHints: ["password", "textarea", "search", "email"])]
public class TextInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var withoutValue = UseState((string?)null);
        var withValue = UseState("Hello");

        var onChangedState = UseState("");
        var onChangeLabel = UseState("");
        var onBlurState = UseState("");
        var onBlurLabel = UseState("");

        var stringState = UseState("");
        var nullStringState = UseState<string?>();

        var dataBinding = Layout.Grid().Columns(3)

                          | Text.InlineCode("string")
                          | (Layout.Vertical()
                             | stringState.ToTextInput()
                             | stringState.ToTextAreaInput()
                             | stringState.ToPasswordInput()
                             | stringState.ToSearchInput()
                          )
                          | stringState

                          | Text.InlineCode("string?")
                          | (Layout.Vertical()
                             | nullStringState.ToTextInput()
                             | nullStringState.ToTextAreaInput()
                             | nullStringState.ToPasswordInput()
                             | nullStringState.ToSearchInput()
                          )
                          | nullStringState
            ;


        return Layout.Vertical()
               | Text.H1("Text Inputs")
               | Text.H2("Sizes")
               | CreateSizesSection()
               | Text.H2("Variants")
               | (Layout.Grid().Columns(5)
                  | null!
                  | Text.InlineCode("Empty")
                  | Text.InlineCode("With Value")
                  | Text.InlineCode("Disabled")
                  | Text.InlineCode("Invalid")

                  | Text.InlineCode("TextInputs.Text")
                  | withoutValue.ToTextInput().Placeholder("Placeholder")
                  | withValue.ToTextInput()
                  | withValue.ToTextInput().Disabled()
                  | withValue.ToTextInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("TextInputs.Password")
                  | withoutValue.ToPasswordInput().Placeholder("Placeholder")
                  | withValue.ToPasswordInput()
                  | withValue.ToPasswordInput().Disabled()
                  | withValue.ToPasswordInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("TextInputs.TextArea")
                  | withoutValue.ToTextAreaInput().Placeholder("Placeholder")
                  | withValue.ToTextAreaInput()
                  | withValue.ToTextAreaInput().Disabled()
                  | withValue.ToTextAreaInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("TextInputs.Search")
                  | withoutValue.ToSearchInput().Placeholder("Placeholder").ShortcutKey("Ctrl+K")
                  | withValue.ToSearchInput()
                  | withValue.ToSearchInput().Disabled()
                  | withValue.ToSearchInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")
               )

               //Data Binding:

               | Text.H2("Data Binding")
               | dataBinding

               //Events: 

               | Text.H2("Events")
               | Text.H3("OnChange")
               | Layout.Horizontal(
                   new TextInput(onChangedState.Value, e =>
                   {
                       onChangedState.Set(e.Value);
                       onChangeLabel.Set("Changed");
                   }),
                   onChangeLabel
                )
               | Text.H3("OnBlur")
               | Layout.Horizontal(
                   onBlurState.ToTextInput().HandleBlur(e => onBlurLabel.Set("Blur")),
                   onBlurLabel
               )
            ;
    }

    private object CreateSizesSection()
    {
        var textState = UseState("Hello");
        var passwordState = UseState("Hello");
        var textareaState = UseState("Hello");
        var searchState = UseState("Hello");

        return Layout.Grid().Columns(4)
               | Text.InlineCode("Description")
               | Text.InlineCode("Small")
               | Text.InlineCode("Medium")
               | Text.InlineCode("Large")

               | Text.InlineCode("TextInputs.Text")
               | textState.ToTextInput().Small()
               | textState.ToTextInput()
               | textState.ToTextInput().Large()

               | Text.InlineCode("TextInputs.Password")
               | passwordState.ToPasswordInput().Small()
               | passwordState.ToPasswordInput()
               | passwordState.ToPasswordInput().Large()

               | Text.InlineCode("TextInputs.TextArea")
               | textareaState.ToTextAreaInput().Small()
               | textareaState.ToTextAreaInput()
               | textareaState.ToTextAreaInput().Large()

               | Text.InlineCode("TextInputs.Search")
               | searchState.ToSearchInput().Small()
               | searchState.ToSearchInput()
               | searchState.ToSearchInput().Large();
    }
}