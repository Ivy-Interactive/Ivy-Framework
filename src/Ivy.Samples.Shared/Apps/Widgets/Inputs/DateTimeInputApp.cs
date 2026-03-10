
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Calendar, path: ["Widgets", "Inputs"], searchHints: ["calendar", "date", "time", "picker", "datetime", "timestamp"])]
public class DateTimeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        // States for variants
        var dateState = UseState(DateTime.Now);
        var dateTimeState = UseState(DateTime.Now);
        var timeState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var monthState = UseState(DateTime.Now);
        var weekState = UseState(DateTime.Now);
        var yearState = UseState(DateTime.Now);
        var nullableDateState = UseState<DateTime?>(() => null);
        var invalidDateState = UseState(DateTime.Now);
        var disabledDateState = UseState(DateTime.Now);

        // States for data binding
        var dateOnlyState = UseState(DateOnly.FromDateTime(DateTime.Now));
        var timeOnlyState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var stringState = UseState(DateTime.Now.ToString("O"));
        var nullableTimeState = UseState<TimeOnly?>(() => null);
        var nullableDateOnlyState = UseState<DateOnly?>(() => null);
        var dateTimeOffsetState = UseState(DateTimeOffset.Now);
        var nullableDateTimeOffsetState = UseState<DateTimeOffset?>(() => null);

        // Size examples
        var sizeExamplesGrid = Layout.Grid().Columns(7)
            | Text.InlineCode("Size")
            | Text.InlineCode("Date Input")
            | Text.InlineCode("DateTime Input")
            | Text.InlineCode("Time Input")
            | Text.InlineCode("Month Input")
            | Text.InlineCode("Week Input")
            | Text.InlineCode("Year Input")

            | Text.InlineCode("Small")
            | dateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Small()
                .Placeholder("Small date")
                .TestId("datetime-input-date-small")
            | dateTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Small()
                .Placeholder("Small datetime")
                .TestId("datetime-input-datetime-small")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Small()
                .Placeholder("Small time")
                .TestId("datetime-input-time-small")
            | monthState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Small()
                .Placeholder("Small month")
                .TestId("datetime-input-month-small")
            | weekState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Small()
                .Placeholder("Small week")
                .TestId("datetime-input-week-small")
            | yearState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Small()
                .Placeholder("Small year")
                .TestId("datetime-input-year-small")

            | Text.InlineCode("Medium")
            | dateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Medium()
                .Placeholder("Medium date")
                .TestId("datetime-input-date-medium")
            | dateTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Medium()
                .Placeholder("Medium datetime")
                .TestId("datetime-input-datetime-medium")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Medium()
                .Placeholder("Medium time")
                .TestId("datetime-input-time-medium")
            | monthState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Medium()
                .Placeholder("Medium month")
                .TestId("datetime-input-month-medium")
            | weekState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Medium()
                .Placeholder("Medium week")
                .TestId("datetime-input-week-medium")
            | yearState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Medium()
                .Placeholder("Medium year")
                .TestId("datetime-input-year-medium")

            | Text.InlineCode("Large")
            | dateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Large()
                .Placeholder("Large date")
                .TestId("datetime-input-date-large")
            | dateTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Large()
                .Placeholder("Large datetime")
                .TestId("datetime-input-datetime-large")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Large()
                .Placeholder("Large time")
                .TestId("datetime-input-time-large")
            | monthState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Large()
                .Placeholder("Large month")
                .TestId("datetime-input-month-large")
            | weekState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Large()
                .Placeholder("Large week")
                .TestId("datetime-input-week-large")
            | yearState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Large()
                .Placeholder("Large year")
                .TestId("datetime-input-year-large");

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
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("Pick a date")
                .Format("yyyy-MM-dd")
                .TestId("datetime-input-date-main")
            | disabledDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("Pick a date")
                .Disabled()
                .TestId("datetime-input-date-disabled-main")
            | invalidDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("Pick a date")
                .Format("yyyy-MM-dd")
                .Invalid("Invalid date")
                .TestId("datetime-input-date-invalid-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("Pick a date")
                .TestId("datetime-input-date-nullable-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("Pick a date")
                .Invalid("Nullable invalid date")
                .TestId("datetime-input-date-nullable-invalid-main")

            | Text.InlineCode("DateTime")
            | dateTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Pick date & time")
                .TestId("datetime-input-datetime-main")
            | disabledDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Pick date & time")
                .Disabled()
                .TestId("datetime-input-datetime-disabled-main")
            | invalidDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Pick date & time")
                .Invalid("Invalid datetime")
                .TestId("datetime-input-datetime-invalid-main")
            | nullableDateState.ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Pick date & time")
                .TestId("datetime-input-datetime-nullable-main")
            | nullableDateState.ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Pick date & time")
                .Invalid("Nullable invalid datetime")
                .TestId("datetime-input-datetime-nullable-invalid-main")

            | Text.InlineCode("Time")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Pick a time")
                .TestId("datetime-input-time-main")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Pick a time")
                .Disabled()
                .TestId("datetime-input-time-disabled-main")
            | timeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Pick a time")
                .Invalid("Invalid time")
                .TestId("datetime-input-time-invalid-main")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Pick a time")
                .TestId("datetime-input-time-nullable-main")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Pick a time")
                .Invalid("Nullable invalid time")
                .TestId("datetime-input-time-nullable-invalid-main")

            | Text.InlineCode("Month")
            | monthState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Placeholder("Pick month")
                .TestId("datetime-input-month-main")
            | disabledDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Placeholder("Pick month")
                .Disabled()
                .TestId("datetime-input-month-disabled-main")
            | invalidDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Placeholder("Pick month")
                .Invalid("Invalid month")
                .TestId("datetime-input-month-invalid-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Placeholder("Pick month")
                .TestId("datetime-input-month-nullable-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Placeholder("Pick month")
                .Invalid("Nullable invalid month")
                .TestId("datetime-input-month-nullable-invalid-main")

            | Text.InlineCode("Week")
            | weekState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Placeholder("Pick week")
                .TestId("datetime-input-week-main")
            | disabledDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Placeholder("Pick week")
                .Disabled()
                .TestId("datetime-input-week-disabled-main")
            | invalidDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Placeholder("Pick week")
                .Invalid("Invalid week")
                .TestId("datetime-input-week-invalid-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Placeholder("Pick week")
                .TestId("datetime-input-week-nullable-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Placeholder("Pick week")
                .Invalid("Nullable invalid week")
                .TestId("datetime-input-week-nullable-invalid-main")

            | Text.InlineCode("Year")
            | yearState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Placeholder("Pick year")
                .TestId("datetime-input-year-main")
            | disabledDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Placeholder("Pick year")
                .Disabled()
                .TestId("datetime-input-year-disabled-main")
            | invalidDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Placeholder("Pick year")
                .Invalid("Invalid year")
                .TestId("datetime-input-year-invalid-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Placeholder("Pick year")
                .TestId("datetime-input-year-nullable-main")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Placeholder("Pick year")
                .Invalid("Nullable invalid year")
                .TestId("datetime-input-year-nullable-invalid-main");

        // Data binding grid
        var dataBindingGrid = Layout.Grid().Columns(3)
            | Text.InlineCode("Type")
            | Text.InlineCode("Input")
            | Text.InlineCode("Current Value")

            | Text.InlineCode("DateTime")
            | dateState.ToDateTimeInput().Variant(DateTimeInputVariant.DateTime).TestId("datetime-input-datetime-binding")
            | Text.InlineCode($"{dateState.Value:yyyy-MM-dd HH:mm:ss}")

            | Text.InlineCode("DateOnly")
            | dateOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .TestId("datetime-input-dateonly-binding")
            | Text.InlineCode($"{dateOnlyState.Value:yyyy-MM-dd}")

            | Text.InlineCode("TimeOnly")
            | timeOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .TestId("datetime-input-timeonly-binding")
            | Text.InlineCode($"{timeOnlyState.Value:HH:mm:ss}")

            | Text.InlineCode("string (ISO)")
            | stringState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .TestId("datetime-input-string-binding")
            | Text.InlineCode(stringState.Value)

            | Text.InlineCode("DateTime?")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .TestId("datetime-input-datetime-nullable-binding")
            | Text.InlineCode(nullableDateState.Value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null")

            | Text.InlineCode("DateOnly?")
            | nullableDateOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .TestId("datetime-input-dateonly-nullable-binding")
            | Text.InlineCode(nullableDateOnlyState.Value?.ToString("yyyy-MM-dd") ?? "null")

            | Text.InlineCode("TimeOnly?")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .TestId("datetime-input-timeonly-nullable-binding")
            | Text.InlineCode(nullableTimeState.Value?.ToString("HH:mm:ss") ?? "null")

            | Text.InlineCode("DateTimeOffset")
            | dateTimeOffsetState.ToDateTimeInput().Variant(DateTimeInputVariant.DateTime).TestId("datetime-input-datetimeoffset-binding")
            | Text.InlineCode($"{dateTimeOffsetState.Value:yyyy-MM-dd HH:mm:ss zzz}")

            | Text.InlineCode("DateTimeOffset?")
            | nullableDateTimeOffsetState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .TestId("datetime-input-datetimeoffset-nullable-binding")
            | Text.InlineCode(nullableDateTimeOffsetState.Value?.ToString("yyyy-MM-dd HH:mm:ss zzz") ?? "null")

            | Text.InlineCode("Month")
            | monthState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .TestId("datetime-input-month-binding")
            | Text.InlineCode($"{monthState.Value:yyyy-MM}")

            | Text.InlineCode("Week")
            | weekState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .TestId("datetime-input-week-binding")
            | Text.InlineCode($"{weekState.Value:yyyy-'W'ww}")

            | Text.InlineCode("Year")
            | yearState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
            .TestId("datetime-input-year-binding")
            | Text.InlineCode($"{yearState.Value:yyyy}");

        // Placeholder examples
        var placeholderExamplesGrid = Layout.Grid().Columns(3)
            | Text.InlineCode("Variant")
            | Text.InlineCode("Placeholder Text")
            | Text.InlineCode("Input")

            | Text.InlineCode("Date")
            | Text.InlineCode("Birthday")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("Birthday")
                .TestId("datetime-input-placeholder-birthday")

            | Text.InlineCode("Date")
            | Text.InlineCode("When did you start?")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("When did you start?")
                .TestId("datetime-input-placeholder-start-date")

            | Text.InlineCode("DateTime")
            | Text.InlineCode("Meeting time")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Meeting time")
                .TestId("datetime-input-placeholder-meeting")

            | Text.InlineCode("DateTime")
            | Text.InlineCode("Deadline")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Deadline")
                .TestId("datetime-input-placeholder-deadline")

            | Text.InlineCode("Time")
            | Text.InlineCode("Start time")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Start time")
                .TestId("datetime-input-placeholder-start-time")

            | Text.InlineCode("Time")
            | Text.InlineCode("Lunch break")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Lunch break")
                .TestId("datetime-input-placeholder-lunch-time")

            | Text.InlineCode("Month")
            | Text.InlineCode("Billing period")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Placeholder("Billing period")
                .TestId("datetime-input-placeholder-month")

            | Text.InlineCode("Week")
            | Text.InlineCode("Project week")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Placeholder("Project week")
                .TestId("datetime-input-placeholder-week")

            | Text.InlineCode("Year")
            | Text.InlineCode("Fiscal year")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Placeholder("Fiscal year")
                .TestId("datetime-input-placeholder-year");

        // Current values section
        var currentValues = Layout.Vertical()
            | Text.H3("Current Values")
            | Text.Block($"Date: {dateState.Value:yyyy-MM-dd}")
            | Text.Block($"DateTime: {dateTimeState.Value:yyyy-MM-dd HH:mm:ss}")
            | Text.Block($"Time: {timeState.Value:HH:mm:ss}")
            | Text.Block($"Month: {monthState.Value:yyyy-MM}")
            | Text.Block($"Week: {weekState.Value:yyyy-'W'ww}")
            | Text.Block($"Year: {yearState.Value:yyyy}")
            | Text.Block($"Nullable DateTime: {nullableDateState.Value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null"}")
            | Text.Block($"DateOnly: {dateOnlyState.Value:yyyy-MM-dd}")
            | Text.Block($"TimeOnly: {timeOnlyState.Value:HH:mm:ss}")
            | Text.Block($"string: {stringState.Value}")
            | Text.Block($"Nullable DateOnly: {nullableDateOnlyState.Value?.ToString("yyyy-MM-dd") ?? "null"}")
            | Text.Block($"Nullable TimeOnly: {nullableTimeState.Value?.ToString("HH:mm:ss") ?? "null"}")
            | Text.Block($"DateTimeOffset: {dateTimeOffsetState.Value:yyyy-MM-dd HH:mm:ss zzz}")
            | Text.Block($"Nullable DateTimeOffset: {nullableDateTimeOffsetState.Value?.ToString("yyyy-MM-dd HH:mm:ss zzz") ?? "null"}");

        return Layout.Vertical()
            | Text.H1("DateTimeInput")
            | Text.H2("Size Examples")
            | sizeExamplesGrid
            | Text.H2("Variants")
            | variantsGrid
            | Text.H2("Data Binding")
            | dataBindingGrid
            | Text.H2("Placeholder Examples")
            | placeholderExamplesGrid
            | currentValues;
    }
}
