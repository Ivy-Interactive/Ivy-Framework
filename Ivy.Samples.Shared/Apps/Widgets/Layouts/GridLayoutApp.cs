using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.Grid3x3)]
public class GridLayoutApp : ViewBase
{
    public override object? Build()
    {
        var withoutValue = UseState((string?)null);
        var withValue = UseState("Hello");

        return Layout.Grid().Columns(5)
               | null!
               | Text.Block("Empty")
               | Text.Block("With Value")
               | Text.Block("Disabled")
               | Text.Block("Invalid")

               | Text.InlineCode("TextVariant.Text")
               | withoutValue.ToTextInput().Placeholder("Placeholder")
               | withValue.ToTextInput()
               | withValue.ToTextInput().Disabled()
               | withValue.ToTextInput().Invalid("Error")

               | Text.InlineCode("TextVariant.Password")
               | withoutValue.ToPasswordInput().Placeholder("Placeholder")
               | withValue.ToPasswordInput()
               | withValue.ToPasswordInput().Disabled()
               | withValue.ToPasswordInput().Invalid("Error")

               | Text.InlineCode("TextVariant.TextArea")
               | withoutValue.ToTextAreaInput().Placeholder("Placeholder")
               | withValue.ToTextAreaInput()
               | withValue.ToTextAreaInput().Disabled()
               | withValue.ToTextAreaInput().Invalid("Error")

               | Text.InlineCode("TextVariant.Search")
               | withoutValue.ToSearchInput().Placeholder("Placeholder")
               | withValue.ToSearchInput()
               | withValue.ToSearchInput().Disabled()
               | withValue.ToSearchInput().Invalid("Error")

            ;
    }
}