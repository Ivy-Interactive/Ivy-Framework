
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.CalendarRange, group: ["Widgets", "Inputs"], searchHints: ["calendar", "date", "picker", "range", "period", "dates"])]
public class DateRangeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Add missing states for data binding and current values
        var dateOnlyRangeState = UseState<(DateOnly, DateOnly)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var nullableDateOnlyRangeState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        // States for variants
        var nullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var disabledNullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var requiredNullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var nullableInvalidDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var nullableDisabledDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var emptyNullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));
        var onBlurState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));
        var onBlurLabel = UseState("");
        var onFocusState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));
        var onFocusLabel = UseState("");
        var constrainedRangeState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

        // Size examples
        var sizeExamplesGrid = Layout.Grid().Columns(4)
            | Text.Monospaced("Size")
            | Text.Monospaced("Normal")
            | Text.Monospaced("Nullable")
            | Text.Monospaced("Disabled")

            | Text.Monospaced("Small")
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Small().TestId("daterange-input-dateonly-small")
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Small().TestId("daterange-input-dateonly-small-nullable")
            | disabledNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Small().Disabled().TestId("daterange-input-dateonly-small-disabled")

            | Text.Monospaced("Medium")
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Density(Density.Medium).TestId("daterange-input-dateonly-medium")
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Density(Density.Medium).TestId("daterange-input-dateonly-medium-nullable")
            | disabledNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Density(Density.Medium).Disabled().TestId("daterange-input-dateonly-medium-disabled")

            | Text.Monospaced("Large")
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Large().TestId("daterange-input-dateonly-large")
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Large().TestId("daterange-input-dateonly-large-nullable")
            | disabledNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Large().Disabled().TestId("daterange-input-dateonly-large-disabled");

        // Variants grid
        var variantsGrid = Layout.Grid().Columns(5)
            // Header row (update last col)
            | Text.Monospaced("Normal") | Text.Monospaced("Disabled") | Text.Monospaced("Required") | Text.Monospaced("Nullable") | Text.Monospaced("Nullable Invalid")
            // Example row (update last widget)
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").TestId("daterange-input-dateonly-nullable-main")
            | disabledNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Disabled().TestId("daterange-input-dateonly-nullable-disabled-main")
            | requiredNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Invalid("Required").TestId("daterange-input-dateonly-nullable-required-main")
            | nullableInvalidDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").TestId("daterange-input-dateonly-nullable-nullable-main")
            | nullableInvalidDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Invalid("Invalid").TestId("daterange-input-dateonly-nullable-nullable-invalid-main");

        // Data binding grid
        var dataBindingGrid = Layout.Grid().Columns(3)
            | Text.Monospaced("Type")
            | Text.Monospaced("Input")
            | Text.Monospaced("Current Value")

            | Text.Monospaced("(DateOnly, DateOnly)")
            | dateOnlyRangeState
                .ToDateRangeInput()
                .TestId("daterange-input-dateonly-binding")
            | Text.Monospaced($"({dateOnlyRangeState.Value.Item1:yyyy-MM-dd}, {dateOnlyRangeState.Value.Item2:yyyy-MM-dd})")

            | Text.Monospaced("(DateOnly?, DateOnly?)")
            | nullableDateOnlyRangeState
                .ToDateRangeInput()
                .TestId("daterange-input-dateonly-nullable-binding")
            | Text.Monospaced($"({nullableDateOnlyRangeState.Value.Item1?.ToString("yyyy-MM-dd") ?? "null"}, {nullableDateOnlyRangeState.Value.Item2?.ToString("yyyy-MM-dd") ?? "null"})");

        // Current values section
        var currentValues = Layout.Vertical()
            | Text.H3("Current Values")
            | Text.Block($"DateOnly Range: {dateOnlyRangeState.Value.Item1:yyyy-MM-dd} to {dateOnlyRangeState.Value.Item2:yyyy-MM-dd}")
            | Text.Block($"Nullable DateOnly Range: {nullableDateOnlyRangeState.Value.Item1?.ToString("yyyy-MM-dd") ?? "null"} to {nullableDateOnlyRangeState.Value.Item2?.ToString("yyyy-MM-dd") ?? "null"}");

        // Start/End Placeholders
        var startEndPlaceholderExample = emptyNullableDateOnlyState.ToDateRangeInput()
            .StartPlaceholder("Check-in")
            .EndPlaceholder("Check-out")
            .Format("MM/dd/yyyy")
            .TestId("daterange-input-start-end-placeholder");

        // Min/Max Constraints Example
        var minMaxExample = Layout.Vertical().Gap(2)
            | Text.P("Date range constrained to 2026 only").Small()
            | constrainedRangeState.ToDateRangeInput()
                .Min(new DateOnly(2026, 1, 1))
                .Max(new DateOnly(2026, 12, 31))
                .Placeholder("Select dates within 2026")
                .Format("MM/dd/yyyy")
                .TestId("daterange-input-min-max-example");

        return Layout.Vertical()
            | Text.H1("DateRang Input")
            | Text.H2("Size Examples")
            | sizeExamplesGrid
            | Text.H2("Variants")
            | variantsGrid
            | Text.H2("Start/End Placeholders")
            | startEndPlaceholderExample
            | Text.H2("Min/Max Constraints")
            | minMaxExample
            | Text.H2("Data Binding")
            | dataBindingGrid
            | Text.H2("Events")
            | (Layout.Vertical()
                | new Card(
                    Layout.Vertical().Gap(2)
                        | Text.P("The blur event fires when the input loses focus.").Small()
                        | onBlurState.ToDateRangeInput().OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                        | (onBlurLabel.Value != ""
                            ? Callout.Success(onBlurLabel.Value)
                            : Callout.Info("Interact then click away to see blur events"))
                ).Title("OnBlur Handler")
                | new Card(
                    Layout.Vertical().Gap(2)
                        | Text.P("The focus event fires when you click on or tab into the input.").Small()
                        | onFocusState.ToDateRangeInput().OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                        | (onFocusLabel.Value != ""
                            ? Callout.Success(onFocusLabel.Value)
                            : Callout.Info("Click or tab into the input to see focus events"))
                ).Title("OnFocus Handler")
            )
            | currentValues;
    }
}
