using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.Check, path: ["Widgets", "Inputs"])]
public class BoolInputApp : SampleBase
{
   protected override object? BuildSample()
   {
      var falseState = UseState(false);
      var trueState = UseState(true);
      var nullState = UseState((bool?)null);

      var variants = Layout.Grid().Columns(6)
             | null!
             | Text.Block("True")
             | Text.Block("False")
             | Text.Block("Disabled")
             | Text.Block("Invalid")
             | Text.Block("Nullable")

             | Text.InlineCode("BoolInputs.Checkbox")
             | trueState.ToBoolInput().Label("Label").Description("Description")
             | falseState.ToBoolInput().Label("Label").Description("Description")
             | trueState.ToBoolInput().Label("Label").Description("Description").Disabled()
             | trueState.ToBoolInput().Label("Label").Description("Description").Invalid("Invalid")
             | nullState.ToBoolInput().Label("Label").Description("Description")

             | null!
             | trueState.ToBoolInput().Label("Label")
             | falseState.ToBoolInput().Label("Label")
             | trueState.ToBoolInput().Label("Label").Disabled()
             | trueState.ToBoolInput().Label("Label").Invalid("Invalid")
             | nullState.ToBoolInput().Label("Label")

             | Text.InlineCode("BoolInputs.Switch")
             | trueState.ToSwitchInput().Label("Label").Description("Description")
             | falseState.ToSwitchInput().Label("Label").Description("Description")
             | trueState.ToSwitchInput().Label("Label").Description("Description").Disabled()
             | trueState.ToSwitchInput().Label("Label").Description("Description").Invalid("Invalid")
             | new Box("Not Implemented")

             | null!
             | trueState.ToSwitchInput().Label("Label")
             | falseState.ToSwitchInput().Label("Label")
             | trueState.ToSwitchInput().Label("Label").Disabled()
             | trueState.ToSwitchInput().Label("Label").Invalid("Invalid")
             | new Box("Not Implemented")

             | Text.InlineCode("BoolInputs.Toggle")
             | trueState.ToToggleInput(Icons.Magnet).Label("Label").Description("Description")
             | falseState.ToToggleInput(Icons.Magnet).Label("Label").Description("Description")
             | trueState.ToToggleInput(Icons.Magnet).Label("Label").Description("Description").Disabled()
             | trueState.ToToggleInput(Icons.Magnet).Label("Label").Description("Description").Invalid("Invalid")
             | new Box("Not Implemented")

             | null!
             | trueState.ToToggleInput(Icons.Baby).Label("Label")
             | falseState.ToToggleInput(Icons.Baby).Label("Label")
             | trueState.ToToggleInput(Icons.Baby).Label("Label").Disabled()
             | trueState.ToToggleInput(Icons.Baby).Label("Label").Invalid("Invalid")
             | new Box("Not Implemented")
          ;


      var intState = this.UseState(0);
      var nullableIntState = this.UseState<int?>((int?)null);
      // var floatState = this.UseState(0.0f);
      // var nullableFloatState = this.UseState<float?>((float?)null);
      var boolState = this.UseState(false);
      var nullableBoolState = this.UseState(false);

      var dataBinding = Layout.Grid().Columns(3)

                        | Text.InlineCode("int")
                        | (Layout.Vertical()
                              | intState.ToBoolInput()
                              | intState.ToBoolInput().Variant(BoolInputs.Switch)
                              | intState.ToBoolInput().Variant(BoolInputs.Toggle).Icon(Icons.Star)
                          )
                        | intState

                        | Text.InlineCode("int? (NOT WORKING)")
                        | (Layout.Vertical()
                           | nullableIntState.ToBoolInput()
                           | nullableIntState.ToBoolInput().Variant(BoolInputs.Switch)
                           | nullableIntState.ToBoolInput().Variant(BoolInputs.Toggle).Icon(Icons.Star)
                        )
                        | nullableIntState

                        | Text.InlineCode("bool")
                        | (Layout.Vertical()
                           | boolState.ToBoolInput()
                           | boolState.ToBoolInput().Variant(BoolInputs.Switch)
                           | boolState.ToBoolInput().Variant(BoolInputs.Toggle).Icon(Icons.Star)
                        )
                        | boolState

                        | Text.InlineCode("bool?")
                        | (Layout.Vertical()
                           | nullableBoolState.ToBoolInput()
                           | nullableBoolState.ToBoolInput().Variant(BoolInputs.Switch)
                           | nullableBoolState.ToBoolInput().Variant(BoolInputs.Toggle).Icon(Icons.Star)
                        )
                        | nullableBoolState

                        ;


      return Layout.Vertical()
             | Text.H1("BoolInput")
             | Text.H2("Variants")
             | variants
             | Text.H2("Data Binding")
             | dataBinding
             ;

   }
}