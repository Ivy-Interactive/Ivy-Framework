using System.Security.Cryptography;

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
                             | stringState.ToTextareaInput()
                             | stringState.ToPasswordInput()
                             | stringState.ToSearchInput()
                          )
                          | stringState

                          | Text.InlineCode("string?")
                          | (Layout.Vertical()
                             | nullStringState.ToTextInput()
                             | nullStringState.ToTextareaInput()
                             | nullStringState.ToPasswordInput()
                             | nullStringState.ToSearchInput()
                          )
                          | nullStringState
            ;

        return Layout.Vertical()
               | Text.H1("Text Inputs")
               | Text.H2("Sizes")
               | new TextInputSizes()
               | Text.H2("Variants")
               | (Layout.Grid().Columns(5)
                  | null!
                  | Text.InlineCode("Empty")
                  | Text.InlineCode("With Value")
                  | Text.InlineCode("Disabled")
                  | Text.InlineCode("Invalid")

                  | Text.InlineCode("TextInputVariants.Text")
                  | withoutValue.ToTextInput().Placeholder("Placeholder")
                  | withValue.ToTextInput()
                  | withValue.ToTextInput().Disabled()
                  | withValue.ToTextInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("TextInputVariants.Password")
                  | withoutValue.ToPasswordInput().Placeholder("Placeholder")
                  | withValue.ToPasswordInput()
                  | withValue.ToPasswordInput().Disabled()
                  | withValue.ToPasswordInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("TextInputVariants.Textarea")
                  | withoutValue.ToTextareaInput().Placeholder("Placeholder")
                  | withValue.ToTextareaInput()
                  | withValue.ToTextareaInput().Disabled()
                  | withValue.ToTextareaInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("TextInputVariants.Search")
                  | withoutValue.ToSearchInput().Placeholder("Placeholder").ShortcutKey("Ctrl+K")
                  | withValue.ToSearchInput()
                  | withValue.ToSearchInput().Disabled()
                  | withValue.ToSearchInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")
               )

               | Text.H2("Affixes")
               | new TextInputAffixes()

               //Data Binding:

               | Text.H2("Data Binding")
               | dataBinding

               | Text.H2("Length constraints")
               | new TextInputLengthConstraints()

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
                   onBlurState.ToTextInput().OnBlur(e => onBlurLabel.Set("Blur")),
                   onBlurLabel
               )
               | Text.H3("OnSubmit (press Enter)")
               | new TextInputSubmitDemo()
               | new Spacer().Height(15)
            ;
    }

    // Helper methods moved to TextInputSizes and TextInputPrefixSuffix classes
}

public class TextInputLengthConstraints : ViewBase
{
    public override object Build()
    {
        var minLengthState = UseState("");
        var maxLengthState = UseState("");
        var bothLengthState = UseState("");

        return Layout.Grid().Columns(3)
               | Text.InlineCode("MinLength(3)")
               | Text.InlineCode("MaxLength(10)")
               | Text.InlineCode("MinLength(5) + MaxLength(10)")
               | minLengthState.ToTextInput().Placeholder("At least 3 characters").MinLength(3)
               | maxLengthState.ToTextInput().Placeholder("Up to 10 characters").MaxLength(10)
               | bothLengthState.ToTextInput().Placeholder("Between 5 and 10 characters").MinLength(5).MaxLength(10);
    }
}

public class TextInputSizes : ViewBase
{
    public override object Build()
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

               | Text.InlineCode("TextInputVariants.Text")
               | textState.ToTextInput().Small()
               | textState.ToTextInput()
               | textState.ToTextInput().Large()

               | Text.InlineCode("TextInputVariants.Password")
               | passwordState.ToPasswordInput().Small()
               | passwordState.ToPasswordInput()
               | passwordState.ToPasswordInput().Large()

               | Text.InlineCode("TextInputVariants.Textarea")
               | textareaState.ToTextareaInput().Small()
               | textareaState.ToTextareaInput()
               | textareaState.ToTextareaInput().Large()

               | Text.InlineCode("TextInputVariants.Search")
               | searchState.ToSearchInput().Small()
               | searchState.ToSearchInput()
               | searchState.ToSearchInput().Large();
    }
}

public class TextInputAffixes : ViewBase
{
    public override object Build()
    {
        var textState = UseState("example");
        var nullableState = UseState<string?>((string?)null);

        return Layout.Grid().Columns(4)
               | null!
               | Text.InlineCode("Prefix only")
               | Text.InlineCode("Suffix only")
               | Text.InlineCode("Both")

               | Text.InlineCode("Text prefix/suffix")
               | textState.ToTextInput().Prefix("https://")
               | textState.ToTextInput().Suffix(".com")
               | textState.ToTextInput().Prefix("https://").Suffix(".com")

               | Text.InlineCode("Icon prefix/suffix")
               | textState.ToTextInput().Prefix(Icons.Mail)
               | textState.ToTextInput().Suffix(Icons.Mail)
               | textState.ToTextInput().Prefix(Icons.Mail).Suffix(Icons.Mail)

               | Text.InlineCode("Nullable with prefix/suffix")
               | nullableState.ToTextInput().Prefix("$").Placeholder("Amount")
               | nullableState.ToTextInput().Suffix("%").Placeholder("Percentage")
               | nullableState.ToTextInput().Prefix("https://").Suffix(".com").Placeholder("domain")

               | Text.InlineCode("Nullable + Invalid + ShortcutKey")
               | nullableState.ToTextInput().Prefix("@").Invalid("Required field").ShortcutKey("Ctrl+U")
               | nullableState.ToTextInput().Suffix(Icons.Search).Invalid("Invalid input").ShortcutKey("Ctrl+F")
               | nullableState.ToTextInput().Prefix(Icons.Mail).Suffix(".com").Invalid("Error").ShortcutKey("Ctrl+E");
    }
}

public class TextInputSubmitDemo : ViewBase
{
    public override object Build()
    {
        var searchQuery = UseState("");
        var searchResult = UseState("");
        var tag = UseState("");
        var tags = UseState<List<string>>(new List<string>());

        return Layout.Vertical()
               | Text.P("Search example (type and press Enter):")
               | Layout.Horizontal(
                   searchQuery.ToSearchInput()
                       .Placeholder("Search...")
                       .OnSubmit(() => searchResult.Set($"Searched for: {searchQuery.Value}")),
                   searchResult
               )
               | Text.P("Quick-add tags (type and press Enter to add):")
               | Layout.Horizontal(
                   tag.ToTextInput()
                       .Placeholder("Add a tag...")
                       .OnSubmit(() =>
                       {
                           if (!string.IsNullOrWhiteSpace(tag.Value))
                           {
                               tags.Set(new List<string>(tags.Value) { tag.Value });
                               tag.Set("");
                           }
                       }),
                   Layout.Horizontal().Gap(2) | tags.Value.Select(t => new Badge(t))
               );
    }
}
