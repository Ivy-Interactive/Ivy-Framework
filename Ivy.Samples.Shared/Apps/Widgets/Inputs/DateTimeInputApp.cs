using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Calendar, path: ["Widgets", "Inputs"])]
public class DateTimeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        // States for variants
        var dateState = UseState(DateTime.Now);
        var dateTimeState = UseState(DateTime.Now);
        var timeState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var nullableDateState = UseState<DateTime?>(() => null);
        var invalidDateState = UseState(DateTime.Now);
        var disabledDateState = UseState(DateTime.Now);

        // States for data binding
        var dateOnlyState = UseState(DateOnly.FromDateTime(DateTime.Now));
        var timeOnlyState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var stringState = UseState(DateTime.Now.ToString("O"));
        var nullableTimeState = UseState<TimeOnly?>(() => null);
        var nullableDateOnlyState = UseState<DateOnly?>(() => null);

        // Variants grid
        var variantsGrid = Layout.Grid().Columns(6)
            | null!
            | Text.InlineCode("Normal")
            | Text.InlineCode("Disabled")
            | Text.InlineCode("Invalid")
            | Text.InlineCode("Nullable")
            | Text.InlineCode("Nullable + Invalid")

            | Text.InlineCode("Date")
            | dateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .Placeholder("Pick a date")
                .Format("yyyy-MM-dd")
                .TestId("datetime-input-date-main")
            | disabledDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .Placeholder("Pick a date")
                .Disabled()
                .TestId("datetime-input-date-disabled-main")
            | invalidDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .Placeholder("Pick a date")
                .Format("yyyy-MM-dd")
                .Invalid("Invalid date")
                .TestId("datetime-input-date-invalid-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .Placeholder("Pick a date")
                .TestId("datetime-input-date-nullable-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .Placeholder("Pick a date")
                .Invalid("Nullable invalid date")
                .TestId("datetime-input-date-nullable-invalid-main")

            | Text.InlineCode("DateTime")
            | dateTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .Placeholder("Pick date & time")
                .TestId("datetime-input-datetime-main")
            | disabledDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .Placeholder("Pick date & time")
                .Disabled()
                .TestId("datetime-input-datetime-disabled-main")
            | invalidDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .Placeholder("Pick date & time")
                .Invalid("Invalid datetime")
                .TestId("datetime-input-datetime-invalid-main")
            | nullableDateState.ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .Placeholder("Pick date & time")
                .TestId("datetime-input-datetime-nullable-main")
            | nullableDateState.ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .Placeholder("Pick date & time")
                .Invalid("Nullable invalid datetime")
                .TestId("datetime-input-datetime-nullable-invalid-main")

            | Text.InlineCode("Time")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .Placeholder("Pick a time")
                .TestId("datetime-input-time-main")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .Placeholder("Pick a time")
                .Disabled()
                .TestId("datetime-input-time-disabled-main")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .Placeholder("Pick a time")
                .Invalid("Invalid time")
                .TestId("datetime-input-time-invalid-main")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .Placeholder("Pick a time")
                .TestId("datetime-input-time-nullable-main")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .Placeholder("Pick a time")
                .Invalid("Nullable invalid time")
                .TestId("datetime-input-time-nullable-invalid-main");

        // Data binding grid
        var dataBindingGrid = Layout.Grid().Columns(3)
            | Text.InlineCode("Type")
            | Text.InlineCode("Input")
            | Text.InlineCode("Current Value")

            | Text.InlineCode("DateTime")
            | dateState.ToDateTimeInput().Variant(DateTimeInputs.DateTime).TestId("datetime-input-datetime-binding")
            | Text.InlineCode($"{dateState.Value:yyyy-MM-dd HH:mm:ss}")

            | Text.InlineCode("DateOnly")
            | dateOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .TestId("datetime-input-dateonly-binding")
            | Text.InlineCode($"{dateOnlyState.Value:yyyy-MM-dd}")

            | Text.InlineCode("TimeOnly")
            | timeOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .TestId("datetime-input-timeonly-binding")
            | Text.InlineCode($"{timeOnlyState.Value:HH:mm:ss}")

            | Text.InlineCode("string (ISO)")
            | stringState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .TestId("datetime-input-string-binding")
            | Text.InlineCode(stringState.Value)

            | Text.InlineCode("DateTime?")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .TestId("datetime-input-datetime-nullable-binding")
            | Text.InlineCode(nullableDateState.Value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null")

            | Text.InlineCode("DateOnly?")
            | nullableDateOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .TestId("datetime-input-dateonly-nullable-binding")
            | Text.InlineCode(nullableDateOnlyState.Value?.ToString("yyyy-MM-dd") ?? "null")

            | Text.InlineCode("TimeOnly?")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .TestId("datetime-input-timeonly-nullable-binding")
            | Text.InlineCode(nullableTimeState.Value?.ToString("HH:mm:ss") ?? "null");

        // Placeholder examples
        var placeholderExamplesGrid = Layout.Grid().Columns(3)
            | Text.InlineCode("Variant")
            | Text.InlineCode("Placeholder Text")
            | Text.InlineCode("Input")

            | Text.InlineCode("Date")
            | Text.InlineCode("Birthday")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .Placeholder("Birthday")
                .TestId("datetime-input-placeholder-birthday")

            | Text.InlineCode("Date")
            | Text.InlineCode("When did you start?")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .Placeholder("When did you start?")
                .TestId("datetime-input-placeholder-start-date")

            | Text.InlineCode("DateTime")
            | Text.InlineCode("Meeting time")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .Placeholder("Meeting time")
                .TestId("datetime-input-placeholder-meeting")

            | Text.InlineCode("DateTime")
            | Text.InlineCode("Deadline")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.DateTime)
                .Placeholder("Deadline")
                .TestId("datetime-input-placeholder-deadline")

            | Text.InlineCode("Time")
            | Text.InlineCode("Start time")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .Placeholder("Start time")
                .TestId("datetime-input-placeholder-start-time")

            | Text.InlineCode("Time")
            | Text.InlineCode("Lunch break")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputs.Time)
                .Placeholder("Lunch break")
                .TestId("datetime-input-placeholder-lunch-time");

        // Current values section
        var currentValues = Layout.Vertical()
            | Text.H3("Current Values")
            | Text.Block($"Date: {dateState.Value:yyyy-MM-dd}")
            | Text.Block($"DateTime: {dateTimeState.Value:yyyy-MM-dd HH:mm:ss}")
            | Text.Block($"Time: {timeState.Value:HH:mm:ss}")
            | Text.Block($"Nullable DateTime: {nullableDateState.Value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null"}")
            | Text.Block($"DateOnly: {dateOnlyState.Value:yyyy-MM-dd}")
            | Text.Block($"TimeOnly: {timeOnlyState.Value:HH:mm:ss}")
            | Text.Block($"string: {stringState.Value}")
            | Text.Block($"Nullable DateOnly: {nullableDateOnlyState.Value?.ToString("yyyy-MM-dd") ?? "null"}")
            | Text.Block($"Nullable TimeOnly: {nullableTimeState.Value?.ToString("HH:mm:ss") ?? "null"}");

        return Layout.Vertical()
            | Text.H1("DateTimeInput")
            | Text.H2("Variants")
            | variantsGrid
            | Text.H2("Data Binding")
            | dataBindingGrid
            | Text.H2("Placeholder Examples")
            | placeholderExamplesGrid
            | currentValues;
    }
}