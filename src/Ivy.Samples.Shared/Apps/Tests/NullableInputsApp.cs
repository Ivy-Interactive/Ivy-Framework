namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.CircleOff, path: ["Widgets", "Inputs"], isVisible: false, searchHints: ["nullable", "null", "clear", "optional"])]
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
        var nonNullableDate = UseState(DateOnly.FromDateTime(DateTime.Today));

        // States for scale examples - set initial values so X button appears
        var scaleText = UseState((string?)"Sample text");
        var scaleTextarea = UseState((string?)"Multiline text");
        var scalePassword = UseState((string?)"password123");
        var scaleSearch = UseState((string?)"Search query");
        var scaleEmail = UseState((string?)"user@example.com");
        var scaleCode = UseState((string?)"const x = 1;");
        var scaleNumber = UseState((int?)123);
        var scaleDecimal = UseState((decimal?)123.45m);
        var scaleDouble = UseState((double?)123.45);
        var scaleDate = UseState((DateOnly?)DateOnly.FromDateTime(DateTime.Today));
        var scaleDateTime = UseState((DateTime?)DateTime.Now);
        var scaleTime = UseState((TimeOnly?)TimeOnly.FromDateTime(DateTime.Now));
        var scaleSelect = UseState((string?)"option1");
        var scaleMultiSelect = UseState((string[]?)new[] { "option1", "option2" });
        var scaleColor = UseState((string?)"#FF0000");
        var scaleBool = UseState((bool?)true);
        var scaleFeedback = UseState((int?)4);
        var scaleFeedbackBool = UseState((bool?)true);

        return Layout.Vertical()
             | Text.H1("Nullable Inputs")
             | Text.P("This app demonstrates nullable input functionality. When an input is nullable and has a value, you'll see a clear (X) button to reset it to null.")

             | Text.H2("Density Examples with Nullable Inputs")
             | Text.P("These inputs demonstrate how the X button appears at different scales (Small, Medium, Large). All inputs have values and are nullable.")

             | Text.H3("Text Inputs - Density Comparison")
             | (Layout.Grid().Columns(6)
                | Text.Monospaced("Density")
                | Text.Monospaced("Text")
                | Text.Monospaced("Textarea")
                | Text.Monospaced("Search")
                | Text.Monospaced("Password")
                | Text.Monospaced("Email")

                | Text.Block("Small")
                | scaleText.ToTextInput().Placeholder("Small...").Nullable().Small()
                | scaleTextarea.ToTextareaInput().Placeholder("Small...").Nullable().Small()
                | scaleSearch.ToSearchInput().Placeholder("Small...").Nullable().Small()
                | scalePassword.ToPasswordInput().Placeholder("Small...").Nullable().Small()
                | scaleEmail.ToEmailInput().Placeholder("Small...").Nullable().Small()

                | Text.Block("Medium")
                | scaleText.ToTextInput().Placeholder("Medium...").Nullable().Medium()
                | scaleTextarea.ToTextareaInput().Placeholder("Medium...").Nullable().Medium()
                | scaleSearch.ToSearchInput().Placeholder("Medium...").Nullable().Medium()
                | scalePassword.ToPasswordInput().Placeholder("Medium...").Nullable().Medium()
                | scaleEmail.ToEmailInput().Placeholder("Medium...").Nullable().Medium()

                | Text.Block("Large")
                | scaleText.ToTextInput().Placeholder("Large...").Nullable().Large()
                | scaleTextarea.ToTextareaInput().Placeholder("Large...").Nullable().Large()
                | scaleSearch.ToSearchInput().Placeholder("Large...").Nullable().Large()
                | scalePassword.ToPasswordInput().Placeholder("Large...").Nullable().Large()
                | scaleEmail.ToEmailInput().Placeholder("Large...").Nullable().Large()
             )

             | Text.H3("Number Inputs - Density Comparison")
             | (Layout.Grid().Columns(4)
                | Text.Monospaced("Density")
                | Text.Monospaced("Integer")
                | Text.Monospaced("Decimal")
                | Text.Monospaced("Slider")

                | Text.Block("Small")
                | scaleNumber.ToNumberInput().Placeholder("Small...").Nullable().Small()
                | scaleDecimal.ToNumberInput().Placeholder("Small...").Nullable().Small()
                | scaleNumber.ToSliderInput().Placeholder("Small...").Nullable().Small()

                | Text.Block("Medium")
                | scaleNumber.ToNumberInput().Placeholder("Medium...").Nullable().Medium()
                | scaleDecimal.ToNumberInput().Placeholder("Medium...").Nullable().Medium()
                | scaleNumber.ToSliderInput().Placeholder("Medium...").Nullable().Medium()

                | Text.Block("Large")
                | scaleNumber.ToNumberInput().Placeholder("Large...").Nullable().Large()
                | scaleDecimal.ToNumberInput().Placeholder("Large...").Nullable().Large()
                | scaleNumber.ToSliderInput().Placeholder("Large...").Nullable().Large()
             )

             | Text.H3("DateTime Inputs - Density Comparison")
             | (Layout.Grid().Columns(4)
                | Text.Monospaced("Density")
                | Text.Monospaced("Date")
                | Text.Monospaced("DateTime")
                | Text.Monospaced("Time")

                | Text.Block("Small")
                | scaleDate.ToDateInput().Placeholder("Small...").Nullable().Small()
                | scaleDateTime.ToDateTimeInput().Placeholder("Small...").Nullable().Small()
                | scaleTime.ToTimeInput().Placeholder("Small...").Nullable().Small()

                | Text.Block("Medium")
                | scaleDate.ToDateInput().Placeholder("Medium...").Nullable().Medium()
                | scaleDateTime.ToDateTimeInput().Placeholder("Medium...").Nullable().Medium()
                | scaleTime.ToTimeInput().Placeholder("Medium...").Nullable().Medium()

                | Text.Block("Large")
                | scaleDate.ToDateInput().Placeholder("Large...").Nullable().Large()
                | scaleDateTime.ToDateTimeInput().Placeholder("Large...").Nullable().Large()
                | scaleTime.ToTimeInput().Placeholder("Large...").Nullable().Large()
             )
             | Text.H3("Code Inputs - Density Comparison")
             | (Layout.Grid().Columns(4)
               | Text.Block("Without .Nullable - render only copy to clipboard button if there is a value")
                  | scaleCode.ToCodeInput().Placeholder("Small...").Small()
                  | scaleCode.ToCodeInput().Placeholder("Medium...").Medium()
                  | scaleCode.ToCodeInput().Placeholder("Large...").Large()
               | Text.Block("With .Nullable - render copy to clipboard button and clear button if there is a value")
                  | scaleCode.ToCodeInput().Placeholder("Small...").Nullable().Small()
                  | scaleCode.ToCodeInput().Placeholder("Medium...").Nullable().Medium()
                  | scaleCode.ToCodeInput().Placeholder("Large...").Nullable().Large()

             )
             | Text.H3("Select & Other Inputs - Density Comparison")
             | (Layout.Grid().Columns(5)
                | Text.Monospaced("Density")
                | Text.Monospaced("Select")
                | Text.Monospaced("Color")
                | Text.Monospaced("Boolean")
                | Text.Monospaced("Feedback")

                | Text.Block("Small")
                | scaleSelect.ToSelectInput(
                    new[]
                    {
                          new Option<string>("option1", "Option 1"),
                          new Option<string>("option2", "Option 2"),
                          new Option<string>("option3", "Option 3")
                    },
                    "Small...").Nullable().Small()
                | scaleColor.ToColorInput().Placeholder("Small...").Nullable().Small()
                | scaleBool.ToBoolInput("Small").Nullable().Small()
                | scaleFeedback.ToFeedbackInput(placeholder: "Small...").Nullable().Small()

                | Text.Block("Medium")
                | scaleSelect.ToSelectInput(
                    new[]
                    {
                          new Option<string>("option1", "Option 1"),
                          new Option<string>("option2", "Option 2"),
                          new Option<string>("option3", "Option 3")
                    },
                    "Medium...").Nullable().Medium()
                | scaleColor.ToColorInput().Placeholder("Medium...").Nullable().Medium()
                | scaleBool.ToBoolInput("Medium").Nullable().Medium()
                | scaleFeedback.ToFeedbackInput(placeholder: "Medium...").Nullable().Medium()

                | Text.Block("Large")
                | scaleSelect.ToSelectInput(
                    new[]
                    {
                          new Option<string>("option1", "Option 1"),
                          new Option<string>("option2", "Option 2"),
                          new Option<string>("option3", "Option 3")
                    },
                    "Large...").Nullable().Large()
                | scaleColor.ToColorInput().Placeholder("Large...").Nullable().Large()
                | scaleBool.ToBoolInput("Large").Nullable().Large()
                | scaleFeedback.ToFeedbackInput(placeholder: "Large...").Nullable().Large()
             )

             | Text.H2("Text Inputs")
             | (Layout.Grid().Columns(3)
                | Text.Monospaced("Type")
                | Text.Monospaced("Input")
                | Text.Monospaced("Current Value")

                | Text.Block("Text (string?)")
                | nullableText.ToTextInput().Placeholder("Enter text...").Nullable()
                | (nullableText.Value == null ? Text.Monospaced("null") : Text.Block(nullableText.Value))

                | Text.Block("Textarea (string?)")
                | nullableTextarea.ToTextareaInput().Placeholder("Enter multiline text...").Nullable()
                | (nullableTextarea.Value == null ? Text.Monospaced("null") : Text.Block(nullableTextarea.Value))

                | Text.Block("Search (string?)")
                | nullableSearch.ToSearchInput().Placeholder("Search...").Nullable()
                | (nullableSearch.Value == null ? Text.Monospaced("null") : Text.Block(nullableSearch.Value))

                | Text.Block("Email (string?)")
                | nullableEmail.ToEmailInput().Placeholder("Enter email...").Nullable()
                | (nullableEmail.Value == null ? Text.Monospaced("null") : Text.Block(nullableEmail.Value))

                | Text.Block("Password (string?)")
                | nullablePassword.ToPasswordInput().Placeholder("Enter password...").Nullable()
                | (nullablePassword.Value == null ? Text.Monospaced("null") : Text.Block("***"))

                | Text.Block("Code Input (string?)")
                | nullableCode.ToCodeInput().Placeholder("Enter code...").Nullable().ShowCopyButton(true)
                | (nullableCode.Value == null ? Text.Monospaced("null") : Text.Block(nullableCode.Value ?? ""))
             )

             | Text.H2("Number Inputs")
             | (Layout.Grid().Columns(3)
                | Text.Monospaced("Type")
                | Text.Monospaced("Input")
                | Text.Monospaced("Current Value")

                | Text.Block("Integer (int?)")
                | nullableInt.ToNumberInput().Placeholder("Enter number...").Nullable()
                | (nullableInt.Value == null ? Text.Monospaced("null") : Text.Block(nullableInt.Value.ToString()!))

                | Text.Block("Decimal (decimal?)")
                | nullableDecimal.ToNumberInput().Placeholder("Enter decimal...").Nullable()
                | (nullableDecimal.Value == null ? Text.Monospaced("null") : Text.Block(nullableDecimal.Value.ToString()!))

                | Text.Block("Double (double?)")
                | nullableDouble.ToNumberInput().Placeholder("Enter number...").Nullable()
                | (nullableDouble.Value == null ? Text.Monospaced("null") : Text.Block(nullableDouble.Value.ToString()!))

                | Text.Block("Slider (int?)")
                | nullableInt.ToSliderInput().Placeholder("Slide...").Nullable()
                | (nullableInt.Value == null ? Text.Monospaced("null") : Text.Block(nullableInt.Value.ToString()!))
             )

             | Text.H2("DateTime Inputs")
             | (Layout.Grid().Columns(3)
                | Text.Monospaced("Type")
                | Text.Monospaced("Input")
                | Text.Monospaced("Current Value")

                | Text.Block("Date (DateOnly?)")
                | nullableDate.ToDateInput().Placeholder("Select date...").Nullable()
                | (nullableDate.Value == null ? Text.Monospaced("null") : Text.Block(nullableDate.Value.Value.ToString("yyyy-MM-dd")))

                | Text.Block("DateTime (DateTime?)")
                | nullableDateTime.ToDateTimeInput().Placeholder("Select date/time...").Nullable()
                | (nullableDateTime.Value == null ? Text.Monospaced("null") : Text.Block(nullableDateTime.Value.Value.ToString("yyyy-MM-dd HH:mm:ss")))

                | Text.Block("Time (TimeOnly?)")
                | nullableTime.ToTimeInput().Placeholder("Select time...").Nullable()
                | (nullableTime.Value == null ? Text.Monospaced("null") : Text.Block(nullableTime.Value.Value.ToString("HH:mm:ss")))

                | Text.Block("DateRange ((DateOnly?, DateOnly?))")
                | nullableDateRange.ToDateRangeInput().Placeholder("Select date range...").Nullable()
                | (nullableDateRange.Value.Item1 == null && nullableDateRange.Value.Item2 == null
                    ? Text.Monospaced("null")
                    : Text.Block($"{nullableDateRange.Value.Item1?.ToString("yyyy-MM-dd") ?? "null"} - {nullableDateRange.Value.Item2?.ToString("yyyy-MM-dd") ?? "null"}"))
             )

             | Text.H2("Select Inputs")
             | (Layout.Grid().Columns(3)
                | Text.Monospaced("Type")
                | Text.Monospaced("Input")
                | Text.Monospaced("Current Value")

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
                | (nullableSelect.Value == null ? Text.Monospaced("null") : Text.Block(nullableSelect.Value))

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
                    ? Text.Monospaced("null")
                    : Text.Block(string.Join(", ", nullableMultiSelect.Value ?? Array.Empty<string>())))
             )

             | Text.H2("Other Inputs")
             | (Layout.Grid().Columns(3)
                | Text.Monospaced("Type")
                | Text.Monospaced("Input")
                | Text.Monospaced("Current Value")

                | Text.Block("Color (string?)")
                | nullableColor.ToColorInput().Placeholder("Select color...").Nullable()
                | (nullableColor.Value == null ? Text.Monospaced("null") : Text.Block(nullableColor.Value))

                | Text.Block("Boolean (bool?)")
                | nullableBool.ToBoolInput("Nullable boolean").Nullable()
                | (nullableBool.Value == null ? Text.Monospaced("null") : Text.Block(nullableBool.Value.Value.ToString()))

                | Text.Block("Feedback Stars (int?)")
                | nullableFeedback.ToFeedbackInput(placeholder: "Rate us...").Nullable()
                | (nullableFeedback.Value == null ? Text.Monospaced("null") : Text.Block(nullableFeedback.Value.ToString()!))

                  | Text.Block("Feedback Thumbs (bool?)")
                  | nullableFeedbackBool.ToFeedbackInput(placeholder: "Give feedback...").Thumbs().Nullable()
                  | (nullableFeedbackBool.Value == null ? Text.Monospaced("null") : Text.Block(nullableFeedbackBool.Value.Value.ToString()))
               )

             | Text.H2("With Invalid State")
             | Text.P("Nullable inputs can also display validation errors:")
             | (Layout.Grid().Columns(2)
                | Text.Monospaced("Input")
                | Text.Monospaced("Description")

                | nullableText.ToTextInput().Placeholder("Required field").Invalid("This field is required").Nullable()
                | Text.Block("Nullable text input with validation error")

                | nullableTextarea.ToTextareaInput().Placeholder("Required field").Invalid("This field is required").Nullable()
                | Text.Block("Nullable textarea input with validation error")

                | nullablePassword.ToPasswordInput().Placeholder("Required field").Invalid("This field is required").Nullable()
                | Text.Block("Nullable password input with validation error")

                | nullableSearch.ToSearchInput().Placeholder("Required field").Invalid("This field is required").Nullable()
                | Text.Block("Nullable search input with validation error")

                | nullableEmail.ToEmailInput().Placeholder("Required field").Invalid("This field is required").Nullable()
                | Text.Block("Nullable email input with validation error")

                | nullableInt.ToNumberInput().Placeholder("Enter number").Invalid("Invalid number").Nullable()
                | Text.Block("Nullable number input with validation error")

                | nullableDecimal.ToNumberInput().Placeholder("Enter decimal").Invalid("Invalid decimal").Nullable()
                | Text.Block("Nullable decimal input with validation error")

                | nullableDouble.ToNumberInput().Placeholder("Enter number").Invalid("Invalid number").Nullable()
                | Text.Block("Nullable double input with validation error")

                | nullableDate.ToDateInput().Placeholder("Select date").Invalid("Date is required").Nullable()
                | Text.Block("Nullable date input with validation error")

                | nullableDateTime.ToDateTimeInput().Placeholder("Select date/time").Invalid("Date/time is required").Nullable()
                | Text.Block("Nullable datetime input with validation error")

                | nullableTime.ToTimeInput().Placeholder("Select time").Invalid("Time is required").Nullable()
                | Text.Block("Nullable time input with validation error")

                | nullableCode.ToCodeInput().Placeholder("Enter code").Invalid("Code is required").Nullable()
                | Text.Block("Nullable code input with validation error")

                | nullableSelect.ToSelectInput(
                    new[]
                    {
                        new Option<string>("option1", "Option 1"),
                        new Option<string>("option2", "Option 2"),
                        new Option<string>("option3", "Option 3")
                    },
                    "Select an option...")
                    .Invalid("Selection is required")
                    .Nullable()
                | Text.Block("Nullable select input with validation error")

                | nullableMultiSelect.ToSelectInput(
                    new[]
                    {
                        new Option<string>("option1", "Option 1"),
                        new Option<string>("option2", "Option 2"),
                        new Option<string>("option3", "Option 3")
                    },
                    "Select options...")
                    .Invalid("Selection is required")
                    .Nullable()
                | Text.Block("Nullable multi-select input with validation error")

                | nullableColor.ToColorInput().Placeholder("Select color").Invalid("Color is required").Nullable()
                | Text.Block("Nullable color input with validation error")

                | nullableBool.ToBoolInput("Required field").Invalid("This field is required").Nullable()
                | Text.Block("Nullable boolean input with validation error")

                | nullableFeedback.ToFeedbackInput(placeholder: "Rate us...").Invalid("Rating is required").Nullable()
                | Text.Block("Nullable feedback stars input with validation error")

                | nullableFeedbackBool.ToFeedbackInput(placeholder: "Give feedback...", variant: FeedbackInputVariant.Thumbs).Invalid("Feedback is required").Nullable()
                | Text.Block("Nullable feedback thumbs input with validation error")
             )

             | Text.H2("Non-Nullable vs Nullable Comparison")
             | (Layout.Grid().Columns(3)
                | Text.Monospaced("Type")
                | Text.Monospaced("Non-Nullable")
                | Text.Monospaced("Nullable")

                | Text.Block("Text Input")
                | nonNullableText.ToTextInput()
                | nullableText.ToTextInput().Placeholder("Can be cleared").Nullable()

                | Text.Block("Number Input")
                | nonNullableInt.ToNumberInput()
                | nullableInt.ToNumberInput().Placeholder("Can be cleared").Nullable()

                | Text.Block("Date Input")
                | nonNullableDate.ToDateInput()
                | nullableDate.ToDateInput().Placeholder("Can be cleared").Nullable()
             )
          ;
    }
}
