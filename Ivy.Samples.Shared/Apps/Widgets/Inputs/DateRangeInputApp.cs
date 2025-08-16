using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.CalendarRange, path: ["Widgets", "Inputs"])]
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

        // Variants grid
        var variantsGrid = Layout.Grid().Columns(5)
            // Header row (update last col)
            | Text.InlineCode("Normal") | Text.InlineCode("Disabled") | Text.InlineCode("Required") | Text.InlineCode("Nullable") | Text.InlineCode("Nullable Invalid")
            // Example row (update last widget)
            | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").TestId("daterange-input-dateonly-nullable-main")
            | disabledNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Disabled().TestId("daterange-input-dateonly-nullable-disabled-main")
            | requiredNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Invalid("Required").TestId("daterange-input-dateonly-nullable-required-main")
            | nullableInvalidDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").TestId("daterange-input-dateonly-nullable-nullable-main")
            | nullableInvalidDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Invalid("Invalid").TestId("daterange-input-dateonly-nullable-nullable-invalid-main");

        // Data binding grid
        var dataBindingGrid = Layout.Grid().Columns(3)
            | Text.InlineCode("Type")
            | Text.InlineCode("Input")
            | Text.InlineCode("Current Value")

            | Text.InlineCode("(DateOnly, DateOnly)")
            | dateOnlyRangeState
                .ToDateRangeInput()
                .TestId("daterange-input-dateonly-binding")
            | Text.InlineCode($"({dateOnlyRangeState.Value.Item1:yyyy-MM-dd}, {dateOnlyRangeState.Value.Item2:yyyy-MM-dd})")

            | Text.InlineCode("(DateOnly?, DateOnly?)")
            | nullableDateOnlyRangeState
                .ToDateRangeInput()
                .TestId("daterange-input-dateonly-nullable-binding")
            | Text.InlineCode($"({nullableDateOnlyRangeState.Value.Item1?.ToString("yyyy-MM-dd") ?? "null"}, {nullableDateOnlyRangeState.Value.Item2?.ToString("yyyy-MM-dd") ?? "null"})");

        // Current values section
        var currentValues = Layout.Vertical()
            | Text.H3("Current Values")
            | Text.Block($"DateOnly Range: {dateOnlyRangeState.Value.Item1:yyyy-MM-dd} to {dateOnlyRangeState.Value.Item2:yyyy-MM-dd}")
            | Text.Block($"Nullable DateOnly Range: {nullableDateOnlyRangeState.Value.Item1?.ToString("yyyy-MM-dd") ?? "null"} to {nullableDateOnlyRangeState.Value.Item2?.ToString("yyyy-MM-dd") ?? "null"}");

        return Layout.Vertical()
            | Text.H1("DateRangeInput")
            | Text.H2("Variants")
            | variantsGrid
            | Text.H2("Data Binding")
            | dataBindingGrid
            | currentValues;
    }
}