using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.DollarSign, path: ["Widgets", "Inputs"])]
public class NumberInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var nullIntValue = UseState<int?>();
        var intValue = UseState(12345);

        var onChangedState = UseState(0);
        var onChangeLabel = UseState("");
        var onBlurState = UseState(0);
        var onBlurLabel = UseState("");

        var dataBinding = Layout.Grid().Columns(3)

                          | Text.InlineCode("int")
                          | (Layout.Vertical()
                             | intValue.ToNumberInput()
                             | intValue.ToSliderInput()
                          )
                          | intValue

                          | Text.InlineCode("int?")
                          | (Layout.Vertical()
                             | nullIntValue.ToNumberInput()
                             | nullIntValue.ToSliderInput()
                          )
                          | nullIntValue
            ;

        return Layout.Vertical()
               | Text.H1("Number Inputs")
               | Text.H2("Variants")
               | (Layout.Grid().Columns(5)
                  | null!
                  | Text.Block("Null")
                  | Text.Block("With Value")
                  | Text.Block("Disabled")
                  | Text.Block("Invalid")

                  | Text.InlineCode("ToNumberInput()")
                  | nullIntValue.ToNumberInput().Placeholder("Placeholder")
                  | intValue.ToNumberInput()
                  | intValue.ToNumberInput().Disabled()
                  | intValue.ToNumberInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("ToSliderInput()")
                  | nullIntValue.ToSliderInput().Placeholder("Placeholder")
                  | intValue.ToSliderInput()
                  | intValue.ToSliderInput().Disabled()
                  | intValue.ToSliderInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")
               )

               //Data Binding:

               | Text.H2("Data Binding")
               | dataBinding

               //Events: 

               | Text.H2("Events")
               | Text.H3("OnChange")
               | Layout.Horizontal(
                   new NumberInput<int>(onChangedState.Value, e =>
                   {
                       onChangedState.Set(e);
                       onChangeLabel.Set("Changed");
                   }),
                   onChangeLabel
                )
               | Text.H3("OnBlur")
               | Layout.Horizontal(
                   onBlurState.ToNumberInput().HandleBlur(e => onBlurLabel.Set("Blur")),
                   onBlurLabel
               )
            ;
    }
}