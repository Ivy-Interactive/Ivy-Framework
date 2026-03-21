
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Calendar, group: ["Widgets", "Inputs"], searchHints: ["calendar", "date", "time", "picker", "datetime", "timestamp"])]
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

        // FirstDayOfWeek states
        var mondayFirstDate = UseState(DateOnly.FromDateTime(DateTime.Now));
        var mondayFirstRange = UseState<(DateOnly, DateOnly)>((DateOnly.FromDateTime(DateTime.Now.AddDays(-7)), DateOnly.FromDateTime(DateTime.Now)));

        // Size examples
        var sizeExamplesGrid = Layout.Grid().Columns(7)
            | Text.Monospaced("Size")
            | Text.Monospaced("Date Input")
            | Text.Monospaced("DateTime Input")
            | Text.Monospaced("Time Input")
            | Text.Monospaced("Month Input")
            | Text.Monospaced("Week Input")
            | Text.Monospaced("Year Input")

            | Text.Monospaced("Small")
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

            | Text.Monospaced("Medium")
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

            | Text.Monospaced("Large")
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
            | Text.Monospaced("Normal")
            | Text.Monospaced("Disabled")
            | Text.Monospaced("Invalid")
            | Text.Monospaced("Nullable")
            | Text.Monospaced("Nullable + Invalid")

            | Text.Monospaced("Date")
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

            | Text.Monospaced("DateTime")
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

            | Text.Monospaced("Time")
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

            | Text.Monospaced("Month")
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

            | Text.Monospaced("Week")
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

            | Text.Monospaced("Year")
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
            | Text.Monospaced("Type")
            | Text.Monospaced("Input")
            | Text.Monospaced("Current Value")

            | Text.Monospaced("DateTime")
            | dateState.ToDateTimeInput().Variant(DateTimeInputVariant.DateTime).TestId("datetime-input-datetime-binding")
            | Text.Monospaced($"{dateState.Value:yyyy-MM-dd HH:mm:ss}")

            | Text.Monospaced("DateOnly")
            | dateOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .TestId("datetime-input-dateonly-binding")
            | Text.Monospaced($"{dateOnlyState.Value:yyyy-MM-dd}")

            | Text.Monospaced("TimeOnly")
            | timeOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .TestId("datetime-input-timeonly-binding")
            | Text.Monospaced($"{timeOnlyState.Value:HH:mm:ss}")

            | Text.Monospaced("string (ISO)")
            | stringState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .TestId("datetime-input-string-binding")
            | Text.Monospaced(stringState.Value)

            | Text.Monospaced("DateTime?")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .TestId("datetime-input-datetime-nullable-binding")
            | Text.Monospaced(nullableDateState.Value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null")

            | Text.Monospaced("DateOnly?")
            | nullableDateOnlyState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .TestId("datetime-input-dateonly-nullable-binding")
            | Text.Monospaced(nullableDateOnlyState.Value?.ToString("yyyy-MM-dd") ?? "null")

            | Text.Monospaced("TimeOnly?")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .TestId("datetime-input-timeonly-nullable-binding")
            | Text.Monospaced(nullableTimeState.Value?.ToString("HH:mm:ss") ?? "null")

            | Text.Monospaced("DateTimeOffset")
            | dateTimeOffsetState.ToDateTimeInput().Variant(DateTimeInputVariant.DateTime).TestId("datetime-input-datetimeoffset-binding")
            | Text.Monospaced($"{dateTimeOffsetState.Value:yyyy-MM-dd HH:mm:ss zzz}")

            | Text.Monospaced("DateTimeOffset?")
            | nullableDateTimeOffsetState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .TestId("datetime-input-datetimeoffset-nullable-binding")
            | Text.Monospaced(nullableDateTimeOffsetState.Value?.ToString("yyyy-MM-dd HH:mm:ss zzz") ?? "null")

            | Text.Monospaced("Month")
            | monthState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .TestId("datetime-input-month-binding")
            | Text.Monospaced($"{monthState.Value:yyyy-MM}")

            | Text.Monospaced("Week")
            | weekState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .TestId("datetime-input-week-binding")
            | Text.Monospaced($"{weekState.Value:yyyy-'W'ww}")

            | Text.Monospaced("Year")
            | yearState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
            .TestId("datetime-input-year-binding")
            | Text.Monospaced($"{yearState.Value:yyyy}");

        // Placeholder examples
        var placeholderExamplesGrid = Layout.Grid().Columns(3)
            | Text.Monospaced("Variant")
            | Text.Monospaced("Placeholder Text")
            | Text.Monospaced("Input")

            | Text.Monospaced("Date")
            | Text.Monospaced("Birthday")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("Birthday")
                .TestId("datetime-input-placeholder-birthday")

            | Text.Monospaced("Date")
            | Text.Monospaced("When did you start?")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Date)
                .Placeholder("When did you start?")
                .TestId("datetime-input-placeholder-start-date")

            | Text.Monospaced("DateTime")
            | Text.Monospaced("Meeting time")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Meeting time")
                .TestId("datetime-input-placeholder-meeting")

            | Text.Monospaced("DateTime")
            | Text.Monospaced("Deadline")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.DateTime)
                .Placeholder("Deadline")
                .TestId("datetime-input-placeholder-deadline")

            | Text.Monospaced("Time")
            | Text.Monospaced("Start time")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Start time")
                .TestId("datetime-input-placeholder-start-time")

            | Text.Monospaced("Time")
            | Text.Monospaced("Lunch break")
            | nullableTimeState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Time)
                .Placeholder("Lunch break")
                .TestId("datetime-input-placeholder-lunch-time")

            | Text.Monospaced("Month")
            | Text.Monospaced("Billing period")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Month)
                .Placeholder("Billing period")
                .TestId("datetime-input-placeholder-month")

            | Text.Monospaced("Week")
            | Text.Monospaced("Project week")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Week)
                .Placeholder("Project week")
                .TestId("datetime-input-placeholder-week")

            | Text.Monospaced("Year")
            | Text.Monospaced("Fiscal year")
            | nullableDateState
                .ToDateTimeInput()
                .Variant(DateTimeInputVariant.Year)
                .Placeholder("Fiscal year")
                .TestId("datetime-input-placeholder-year");

        var firstDayOfWeekGrid = Layout.Grid().Columns(2)
            | Text.Monospaced("Monday-first DateInput")
            | mondayFirstDate
                .ToDateInput()
                .FirstDayOfWeek(DayOfWeek.Monday)
                .TestId("datetime-input-monday-first")
            | Text.Monospaced("Monday-first DateRangeInput")
            | mondayFirstRange
                .ToDateRangeInput()
                .FirstDayOfWeek(DayOfWeek.Monday)
                .TestId("daterange-input-monday-first");

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
            | Text.H2("FirstDayOfWeek")
            | firstDayOfWeekGrid
            | currentValues;
    }
}
