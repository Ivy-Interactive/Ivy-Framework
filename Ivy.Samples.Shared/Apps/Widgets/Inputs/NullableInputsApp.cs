using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.CircleOff, path: ["Widgets", "Inputs"], searchHints: ["nullable", "null", "clear", "optional"])]
public class NullableInputsApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Text Inputs
        var nullableText = UseState((string?)null);
        var nullableTextarea = UseState((string?)null);
        var nullablePassword = UseState((string?)null);
        var nullableSearch = UseState((string?)null);
        var nullableEmail = UseState((string?)null);

        // Number Inputs
        var nullableInt = UseState((int?)null);
        var nullableDecimal = UseState((decimal?)null);
        var nullableDouble = UseState((double?)null);

        // DateTime Inputs
        var nullableDate = UseState((DateOnly?)null);
        var nullableDateTime = UseState((DateTime?)null);
        var nullableTime = UseState((TimeOnly?)null);

        // DateRange Input
        var nullableDateRange = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

        // Select Inputs
        var nullableSelect = UseState((string?)null);
        var nullableMultiSelect = UseState((string[]?)null);

        // Code Input
        var nullableCode = UseState((string?)null);

        // Color Input
        var nullableColor = UseState((string?)null);

        // Bool Input
        var nullableBool = UseState((bool?)null);

        // Feedback Input
        var nullableFeedback = UseState((int?)null);
        var nullableFeedbackBool = UseState((bool?)null);

        var nonNullableText = UseState("Hello");
        var nonNullableInt = UseState(42);

        return Layout.Vertical()
               | Text.H1("Nullable Inputs")
               | Text.P("This app demonstrates nullable input functionality. When an input is nullable and has a value, you'll see a clear (X) button to reset it to null.")

               | Text.H2("Text Inputs")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Input")
                  | Text.InlineCode("Current Value")

                  | Text.Block("Text (string?)")
                  | nullableText.ToTextInput().Placeholder("Enter text...").Nullable()
                  | (nullableText.Value == null ? Text.InlineCode("null") : Text.Block(nullableText.Value))

                  | Text.Block("Textarea (string?)")
                  | nullableTextarea.ToTextAreaInput().Placeholder("Enter multiline text...").Nullable()
                  | (nullableTextarea.Value == null ? Text.InlineCode("null") : Text.Block(nullableTextarea.Value))

                  | Text.Block("Password (string?)")
                  | nullablePassword.ToPasswordInput().Placeholder("Enter password...").Nullable()
                  | (nullablePassword.Value == null ? Text.InlineCode("null") : Text.Block("***"))

                  | Text.Block("Search (string?)")
                  | nullableSearch.ToSearchInput().Placeholder("Search...").Nullable()
                  | (nullableSearch.Value == null ? Text.InlineCode("null") : Text.Block(nullableSearch.Value))

                  | Text.Block("Email (string?)")
                  | nullableEmail.ToEmailInput().Placeholder("Enter email...").Nullable()
                  | (nullableEmail.Value == null ? Text.InlineCode("null") : Text.Block(nullableEmail.Value))

                  | Text.Block("Code Input (string?)")
                  | nullableCode.ToCodeInput().Placeholder("Enter code...").Nullable()
                  | (nullableCode.Value == null ? Text.InlineCode("null") : Text.Block(nullableCode.Value ?? ""))
               )

               | Text.H2("Number Inputs")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Input")
                  | Text.InlineCode("Current Value")

                  | Text.Block("Integer (int?)")
                  | nullableInt.ToNumberInput().Placeholder("Enter number...").Nullable()
                  | (nullableInt.Value == null ? Text.InlineCode("null") : Text.Block(nullableInt.Value.ToString()!))

                  | Text.Block("Decimal (decimal?)")
                  | nullableDecimal.ToNumberInput().Placeholder("Enter decimal...").Nullable()
                  | (nullableDecimal.Value == null ? Text.InlineCode("null") : Text.Block(nullableDecimal.Value.ToString()!))

                  | Text.Block("Double (double?)")
                  | nullableDouble.ToNumberInput().Placeholder("Enter number...").Nullable()
                  | (nullableDouble.Value == null ? Text.InlineCode("null") : Text.Block(nullableDouble.Value.ToString()!))

                  | Text.Block("Slider (int?)")
                  | nullableInt.ToSliderInput().Placeholder("Slide...").Nullable()
                  | (nullableInt.Value == null ? Text.InlineCode("null") : Text.Block(nullableInt.Value.ToString()!))
               )

               | Text.H2("DateTime Inputs")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Input")
                  | Text.InlineCode("Current Value")

                  | Text.Block("Date (DateOnly?)")
                  | nullableDate.ToDateInput().Placeholder("Select date...").Nullable()
                  | (nullableDate.Value == null ? Text.InlineCode("null") : Text.Block(nullableDate.Value.Value.ToString("yyyy-MM-dd")))

                  | Text.Block("DateTime (DateTime?)")
                  | nullableDateTime.ToDateTimeInput().Placeholder("Select date/time...").Nullable()
                  | (nullableDateTime.Value == null ? Text.InlineCode("null") : Text.Block(nullableDateTime.Value.Value.ToString("yyyy-MM-dd HH:mm:ss")))

                  | Text.Block("Time (TimeOnly?)")
                  | nullableTime.ToTimeInput().Placeholder("Select time...").Nullable()
                  | (nullableTime.Value == null ? Text.InlineCode("null") : Text.Block(nullableTime.Value.Value.ToString("HH:mm:ss")))

                  | Text.Block("DateRange ((DateOnly?, DateOnly?))")
                  | nullableDateRange.ToDateRangeInput().Placeholder("Select date range...").Nullable()
                  | (nullableDateRange.Value.Item1 == null && nullableDateRange.Value.Item2 == null
                      ? Text.InlineCode("null")
                      : Text.Block($"{nullableDateRange.Value.Item1?.ToString("yyyy-MM-dd") ?? "null"} - {nullableDateRange.Value.Item2?.ToString("yyyy-MM-dd") ?? "null"}"))
               )

               | Text.H2("Select Inputs")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Input")
                  | Text.InlineCode("Current Value")

                  | Text.Block("Select (string?)")
                  | nullableSelect.ToSelectInput(
                      new[]
                      {
                          new Option<string>("option1", "Option 1"),
                          new Option<string>("option2", "Option 2"),
                          new Option<string>("option3", "Option 3")
                      },
                      "Select an option...")
                      .Nullable()
                  | (nullableSelect.Value == null ? Text.InlineCode("null") : Text.Block(nullableSelect.Value))

                  | Text.Block("Multi-Select (string[]?)")
                  | nullableMultiSelect.ToSelectInput(
                      new[]
                      {
                          new Option<string>("option1", "Option 1"),
                          new Option<string>("option2", "Option 2"),
                          new Option<string>("option3", "Option 3")
                      },
                      "Select options...")
                      .Nullable()
                  | (nullableMultiSelect.Value == null
                      ? Text.InlineCode("null")
                      : Text.Block(string.Join(", ", nullableMultiSelect.Value ?? Array.Empty<string>())))
               )

               | Text.H2("Other Inputs")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Input")
                  | Text.InlineCode("Current Value")

                  | Text.Block("Color (string?)")
                  | nullableColor.ToColorInput().Placeholder("Select color...").Nullable()
                  | (nullableColor.Value == null ? Text.InlineCode("null") : Text.Block(nullableColor.Value))

                  | Text.Block("Boolean (bool?)")
                  | nullableBool.ToBoolInput("Nullable boolean").Nullable()
                  | (nullableBool.Value == null ? Text.InlineCode("null") : Text.Block(nullableBool.Value.Value.ToString()))

                  | Text.Block("Feedback Stars (int?)")
                  | nullableFeedback.ToFeedbackInput(placeholder: "Rate us...").Nullable()
                  | (nullableFeedback.Value == null ? Text.InlineCode("null") : Text.Block(nullableFeedback.Value.ToString()!))

                  | Text.Block("Feedback Thumbs (bool?)")
                  | nullableFeedbackBool.ToFeedbackInput(placeholder: "Give feedback...", variant: FeedbackInputs.Thumbs).Nullable()
                  | (nullableFeedbackBool.Value == null ? Text.InlineCode("null") : Text.Block(nullableFeedbackBool.Value.Value.ToString()))
               )

               | Text.H2("With Invalid State")
               | Text.P("Nullable inputs can also display validation errors:")
               | (Layout.Grid().Columns(2)
                  | Text.InlineCode("Input")
                  | Text.InlineCode("Description")

                  | nullableText.ToTextInput().Placeholder("Required field").Invalid("This field is required").Nullable()
                  | Text.Block("Nullable text input with validation error")

                  | nullableInt.ToNumberInput().Placeholder("Enter number").Invalid("Invalid number").Nullable()
                  | Text.Block("Nullable number input with validation error")

                  | nullableDate.ToDateInput().Placeholder("Select date").Invalid("Date is required").Nullable()
                  | Text.Block("Nullable date input with validation error")
               )

               | Text.H2("Non-Nullable vs Nullable Comparison")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Non-Nullable")
                  | Text.InlineCode("Nullable")

                  | Text.Block("Text Input")
                  | nonNullableText.ToTextInput()
                  | nullableText.ToTextInput().Placeholder("Can be cleared").Nullable()

                  | Text.Block("Number Input")
                  | nonNullableInt.ToNumberInput()
                  | nullableInt.ToNumberInput().Placeholder("Can be cleared").Nullable()

                  | Text.Block("Date Input")
                  | UseState(DateOnly.FromDateTime(DateTime.Today)).ToDateInput()
                  | nullableDate.ToDateInput().Placeholder("Can be cleared").Nullable()
               )

               | Text.H2("Automatic Nullable Detection Test")
               | Text.P("These inputs are created from nullable states WITHOUT explicitly calling .Nullable(). The Nullable property should be automatically set to true based on the state type.")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Input (Auto-Detected Nullable)")
                  | Text.InlineCode("Current Value")

                  | Text.Block("Text (string?) - Auto")
                  | nullableText.ToTextInput().Placeholder("Auto-detected nullable...")
                  | (nullableText.Value == null ? Text.InlineCode("null") : Text.Block(nullableText.Value))

                  | Text.Block("Number (int?) - Auto")
                  | nullableInt.ToNumberInput().Placeholder("Auto-detected nullable...")
                  | (nullableInt.Value == null ? Text.InlineCode("null") : Text.Block(nullableInt.Value?.ToString() ?? "null"))

                  | Text.Block("Decimal (decimal?) - Auto")
                  | nullableDecimal.ToNumberInput().Placeholder("Auto-detected nullable...")
                  | (nullableDecimal.Value == null ? Text.InlineCode("null") : Text.Block(nullableDecimal.Value?.ToString() ?? "null"))

                  | Text.Block("Date (DateOnly?) - Auto")
                  | nullableDate.ToDateInput().Placeholder("Auto-detected nullable...")
                  | (nullableDate.Value == null ? Text.InlineCode("null") : Text.Block(nullableDate.Value?.ToString("yyyy-MM-dd") ?? "null"))
               )

               | Text.H2("Non-Nullable States (Should Not Be Nullable)")
               | Text.P("These inputs are created from non-nullable states. The Nullable property should be automatically set to false.")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Type")
                  | Text.InlineCode("Input (Non-Nullable)")
                  | Text.InlineCode("Current Value")

                  | Text.Block("Text (string) - Non-Nullable")
                  | nonNullableText.ToTextInput().Placeholder("Non-nullable string...")
                  | Text.Block(nonNullableText.Value)

                  | Text.Block("Number (int) - Non-Nullable")
                  | nonNullableInt.ToNumberInput().Placeholder("Non-nullable int...")
                  | Text.Block(nonNullableInt.Value.ToString())
               )

               | Text.H2("🔍 DEBUG: Nullable Property Comparison")
               | Text.P("Compare these two inputs - one WITHOUT .Nullable() and one WITH .Nullable(). Check the browser console/network tab to see the difference in their serialized props.")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Description")
                  | Text.InlineCode("Input")
                  | Text.InlineCode("Current Value")

                  | Text.Block("TextInput WITHOUT .Nullable()")
                  | nullableText.ToTextInput().Placeholder("No .Nullable() called...")
                  | (nullableText.Value == null ? Text.InlineCode("null") : Text.Block(nullableText.Value))

                  | Text.Block("TextInput WITH .Nullable()")
                  | nullableText.ToTextInput().Placeholder("With .Nullable() called...").Nullable()
                  | (nullableText.Value == null ? Text.InlineCode("null") : Text.Block(nullableText.Value))
               )
            ;
    }
}
