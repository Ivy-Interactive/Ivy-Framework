
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.CalendarRange, group: ["Widgets", "Inputs"], searchHints: ["calendar", "date", "picker", "range", "period", "dates"])]
public class DateRangeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("DateRange Input")
               | Layout.Tabs(
                   new Tab("Variants", new DateRangeInputVariantsTab()),
                   new Tab("Sizes", new DateRangeInputSizesTab()),
                   new Tab("Constraints", new DateRangeInputConstraintsTab()),
                   new Tab("Data Binding", new DateRangeInputDataBindingTab()),
                   new Tab("Events", new DateRangeInputEventsTab())
               ).Variant(TabsVariant.Content);
    }
}

public class DateRangeInputVariantsTab : ViewBase
{
    public override object Build()
    {
        var nullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var disabledNullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var requiredNullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var nullableInvalidDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));

        return Layout.Grid().Columns(5)
               | Text.Monospaced("Normal") | Text.Monospaced("Disabled") | Text.Monospaced("Required") | Text.Monospaced("Nullable") | Text.Monospaced("Nullable Invalid")
               | nullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").TestId("daterange-input-dateonly-nullable-main")
               | disabledNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Disabled().TestId("daterange-input-dateonly-nullable-disabled-main")
               | requiredNullableDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Invalid("Required").TestId("daterange-input-dateonly-nullable-required-main")
               | nullableInvalidDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").TestId("daterange-input-dateonly-nullable-nullable-main")
               | nullableInvalidDateOnlyState.ToDateRangeInput().Placeholder("Select date range").Format("yyyy-MM-dd").Invalid("Invalid").TestId("daterange-input-dateonly-nullable-nullable-invalid-main");
    }
}

public class DateRangeInputSizesTab : ViewBase
{
    public override object Build()
    {
        var nullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var disabledNullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));

        return Layout.Grid().Columns(4)
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
    }
}

public class DateRangeInputConstraintsTab : ViewBase
{
    public override object Build()
    {
        var emptyNullableDateOnlyState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));
        var constrainedRangeState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

        return Layout.Vertical()
               | Text.H2("Start/End Placeholders")
               | emptyNullableDateOnlyState.ToDateRangeInput()
                   .StartPlaceholder("Check-in")
                   .EndPlaceholder("Check-out")
                   .Format("MM/dd/yyyy")
                   .TestId("daterange-input-start-end-placeholder")
               | Text.H2("Min/Max Constraints")
               | (Layout.Vertical().Gap(2)
                   | Text.P("Date range constrained to 2026 only").Small()
                   | constrainedRangeState.ToDateRangeInput()
                       .Min(new DateOnly(2026, 1, 1))
                       .Max(new DateOnly(2026, 12, 31))
                       .Placeholder("Select dates within 2026")
                       .Format("MM/dd/yyyy")
                       .TestId("daterange-input-min-max-example"));
    }
}

public class DateRangeInputDataBindingTab : ViewBase
{
    public override object Build()
    {
        var dateOnlyRangeState = UseState<(DateOnly, DateOnly)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var nullableDateOnlyRangeState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));

        return Layout.Grid().Columns(3)
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
    }
}

public class DateRangeInputEventsTab : ViewBase
{
    public override object Build()
    {
        var dateOnlyRangeState = UseState<(DateOnly, DateOnly)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var nullableDateOnlyRangeState = UseState<(DateOnly?, DateOnly?)>(() => (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), DateOnly.FromDateTime(DateTime.Today)));
        var onBlurState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));
        var onBlurLabel = UseState("");
        var onFocusState = UseState<(DateOnly?, DateOnly?)>(() => (null, null));
        var onFocusLabel = UseState("");

        return Layout.Vertical()
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
               | Text.H3("Current Values")
               | Text.Block($"DateOnly Range: {dateOnlyRangeState.Value.Item1:yyyy-MM-dd} to {dateOnlyRangeState.Value.Item2:yyyy-MM-dd}")
               | Text.Block($"Nullable DateOnly Range: {nullableDateOnlyRangeState.Value.Item1?.ToString("yyyy-MM-dd") ?? "null"} to {nullableDateOnlyRangeState.Value.Item2?.ToString("yyyy-MM-dd") ?? "null"}");

    }
}
