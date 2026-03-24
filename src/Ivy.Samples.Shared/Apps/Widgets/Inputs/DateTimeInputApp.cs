
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Calendar, group: ["Widgets", "Inputs"], searchHints: ["calendar", "date", "time", "picker", "datetime", "timestamp"])]
public class DateTimeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | Text.H1("DateTimeInput")
            | Layout.Tabs(
                new Tab("Variants", new DateTimeInputVariantSamples()),
                new Tab("Size", new DateTimeInputSizeVariants()),
                new Tab("Data Binding", new DateTimeInputDataBinding()),
                new Tab("Placeholders", new DateTimeInputPlaceholderSamples()),
                new Tab("First Day of Week", new DateTimeInputFirstDayOfWeekSamples()),
                new Tab("Min / Max / Step", new DateTimeInputConstraintSamples())
            ).Variant(TabsVariant.Content);
    }
}

public class DateTimeInputVariantSamples : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Now);
        var dateTimeState = UseState(DateTime.Now);
        var timeState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var monthState = UseState(DateTime.Now);
        var weekState = UseState(DateTime.Now);
        var yearState = UseState(DateTime.Now);
        var nullableDateState = UseState<DateTime?>(() => null);
        var invalidDateState = UseState(DateTime.Now);
        var disabledDateState = UseState(DateTime.Now);
        var nullableTimeState = UseState<TimeOnly?>(() => null);

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

        var currentValues = Layout.Vertical()
            | Text.H3("Current values (this tab)")
            | Text.Block($"Date: {dateState.Value:yyyy-MM-dd}")
            | Text.Block($"DateTime: {dateTimeState.Value:yyyy-MM-dd HH:mm:ss}")
            | Text.Block($"Time: {timeState.Value:HH:mm:ss}")
            | Text.Block($"Month: {monthState.Value:yyyy-MM}")
            | Text.Block($"Week: {weekState.Value:yyyy-'W'ww}")
            | Text.Block($"Year: {yearState.Value:yyyy}")
            | Text.Block($"Nullable DateTime: {nullableDateState.Value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null"}");

        return Layout.Vertical()
            | Text.H2("Variants")
            | Text.P("Normal, disabled, invalid, nullable, and nullable+invalid for each DateTimeInput variant.")
            | variantsGrid
            | currentValues;
    }
}

public class DateTimeInputSizeVariants : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Now);
        var dateTimeState = UseState(DateTime.Now);
        var timeState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var monthState = UseState(DateTime.Now);
        var weekState = UseState(DateTime.Now);
        var yearState = UseState(DateTime.Now);

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

        var currentValues = Layout.Vertical()
            | Text.H3("Current values (this tab)")
            | Text.Block($"Date: {dateState.Value:yyyy-MM-dd}")
            | Text.Block($"DateTime: {dateTimeState.Value:yyyy-MM-dd HH:mm:ss}")
            | Text.Block($"Time: {timeState.Value:HH:mm:ss}")
            | Text.Block($"Month: {monthState.Value:yyyy-MM}")
            | Text.Block($"Week: {weekState.Value:yyyy-'W'ww}")
            | Text.Block($"Year: {yearState.Value:yyyy}");

        return Layout.Vertical()
            | Text.H2("Size")
            | Text.P("Small, Medium, and Large density for each variant.")
            | sizeExamplesGrid
            | currentValues;
    }
}

public class DateTimeInputDataBinding : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Now);
        var dateOnlyState = UseState(DateOnly.FromDateTime(DateTime.Now));
        var timeOnlyState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var stringState = UseState(DateTime.Now.ToString("O"));
        var nullableDateState = UseState<DateTime?>(() => null);
        var nullableTimeState = UseState<TimeOnly?>(() => null);
        var nullableDateOnlyState = UseState<DateOnly?>(() => null);
        var dateTimeOffsetState = UseState(DateTimeOffset.Now);
        var nullableDateTimeOffsetState = UseState<DateTimeOffset?>(() => null);
        var monthState = UseState(DateTime.Now);
        var weekState = UseState(DateTime.Now);
        var yearState = UseState(DateTime.Now);

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

        return Layout.Vertical()
            | Text.H2("Data binding")
            | Text.P("DateTimeInput bound to different CLR types.")
            | dataBindingGrid;
    }
}

public class DateTimeInputPlaceholderSamples : ViewBase
{
    public override object? Build()
    {
        var nullableDateState = UseState<DateTime?>(() => null);
        var nullableTimeState = UseState<TimeOnly?>(() => null);

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

        return Layout.Vertical()
            | Text.H2("Placeholders")
            | Text.P("Placeholder copy per variant.")
            | placeholderExamplesGrid;
    }
}

public class DateTimeInputFirstDayOfWeekSamples : ViewBase
{
    public override object? Build()
    {
        var sundayDate = UseState(DateOnly.FromDateTime(DateTime.Now));
        var mondayDate = UseState(DateOnly.FromDateTime(DateTime.Now));
        var saturdayDate = UseState(DateOnly.FromDateTime(DateTime.Now));
        var sundayDateTime = UseState(DateTime.Now);
        var mondayDateTime = UseState(DateTime.Now);
        var saturdayDateTime = UseState(DateTime.Now);
        var sundayRange = UseState<(DateOnly, DateOnly)>((DateOnly.FromDateTime(DateTime.Now.AddDays(-7)), DateOnly.FromDateTime(DateTime.Now)));
        var mondayRange = UseState<(DateOnly, DateOnly)>((DateOnly.FromDateTime(DateTime.Now.AddDays(-7)), DateOnly.FromDateTime(DateTime.Now)));
        var saturdayRange = UseState<(DateOnly, DateOnly)>((DateOnly.FromDateTime(DateTime.Now.AddDays(-7)), DateOnly.FromDateTime(DateTime.Now)));

        var firstDayOfWeekGrid = Layout.Grid().Columns(3)
            | Text.Monospaced("Sample")
            | Text.Monospaced("Notes")
            | Text.Monospaced("Input")

            | Text.Monospaced("Sunday — Date")
            | Text.Block("Common in US; calendar header starts Sun")
            | sundayDate
                .ToDateInput()
                .FirstDayOfWeek(DayOfWeek.Sunday)
                .Placeholder("Sunday-first week")
                .TestId("datetime-input-fdow-sunday-date")

            | Text.Monospaced("Monday — Date")
            | Text.Block("ISO-style week; common in EU")
            | mondayDate
                .ToDateInput()
                .FirstDayOfWeek(DayOfWeek.Monday)
                .Placeholder("Monday-first week")
                .TestId("datetime-input-fdow-monday-date")

            | Text.Monospaced("Saturday — Date")
            | Text.Block("Used in some Middle Eastern locales")
            | saturdayDate
                .ToDateInput()
                .FirstDayOfWeek(DayOfWeek.Saturday)
                .Placeholder("Saturday-first week")
                .TestId("datetime-input-fdow-saturday-date")

            | Text.Monospaced("Sunday — DateTime")
            | Text.Block("Popover calendar matches Sunday-first rows")
            | sundayDateTime
                .ToDateTimeInput()
                .FirstDayOfWeek(DayOfWeek.Sunday)
                .Placeholder("Date & time (Sun)")
                .TestId("datetime-input-fdow-sunday-datetime")

            | Text.Monospaced("Monday — DateTime")
            | Text.Block("Popover calendar matches Monday-first rows")
            | mondayDateTime
                .ToDateTimeInput()
                .FirstDayOfWeek(DayOfWeek.Monday)
                .Placeholder("Date & time (Mon)")
                .TestId("datetime-input-fdow-monday-datetime")

            | Text.Monospaced("Saturday — DateTime")
            | Text.Block("Saturday-first calendar in combined picker")
            | saturdayDateTime
                .ToDateTimeInput()
                .FirstDayOfWeek(DayOfWeek.Saturday)
                .Placeholder("Date & time (Sat)")
                .TestId("datetime-input-fdow-saturday-datetime")

            | Text.Monospaced("Sunday — DateRange")
            | Text.Block("Range picker with Sun-first grid")
            | sundayRange
                .ToDateRangeInput()
                .FirstDayOfWeek(DayOfWeek.Sunday)
                .TestId("daterange-input-fdow-sunday")

            | Text.Monospaced("Monday — DateRange")
            | Text.Block("Range picker with Mon-first grid")
            | mondayRange
                .ToDateRangeInput()
                .FirstDayOfWeek(DayOfWeek.Monday)
                .TestId("daterange-input-fdow-monday")

            | Text.Monospaced("Saturday — DateRange")
            | Text.Block("Range picker with Sat-first grid")
            | saturdayRange
                .ToDateRangeInput()
                .FirstDayOfWeek(DayOfWeek.Saturday)
                .TestId("daterange-input-fdow-saturday");

        return Layout.Vertical()
            | Text.H2("First day of week")
            | Text.P(
                "DateInput, DateTimeInput, and DateRangeInput pass FirstDayOfWeek into the calendar so week rows start on Sunday, Monday, or Saturday.")
            | firstDayOfWeekGrid;
    }
}

public class DateTimeInputConstraintSamples : ViewBase
{
    public override object? Build()
    {
        var constrainedDateState = UseState(DateTime.Now);
        var appointmentState = UseState(DateTime.Now);
        var timeSlotState = UseState(TimeOnly.FromDateTime(DateTime.Now));
        var constrainedMonthState = UseState(new DateTime(DateTime.Now.Year, 6, 1));
        var constrainedWeekState = UseState(DateTime.Now);
        var constrainedYearState = UseState(new DateTime(DateTime.Now.Year, 1, 1));
        var meetingSlotState = UseState(DateTime.Today.AddHours(10));
        var lunchTimeState = UseState(new TimeOnly(12, 0));

        var constraintsGrid = Layout.Grid().Columns(3)
            | Text.Monospaced("Constraint Type")
            | Text.Monospaced("Configuration")
            | Text.Monospaced("Input")

            | Text.Monospaced("Min/Max Date")
            | Text.Block("Future dates only (today to +90 days)")
            | constrainedDateState
                .ToDateInput()
                .Min(DateTime.Today)
                .Max(DateTime.Today.AddDays(90))
                .Placeholder("Select date")
                .TestId("datetime-input-minmax")

            | Text.Monospaced("Business Hours")
            | Text.Block("9 AM to 5 PM only")
            | appointmentState
                .ToDateTimeInput()
                .Min(DateTime.Today.AddHours(9))
                .Max(DateTime.Today.AddHours(17))
                .Placeholder("Select appointment")
                .TestId("datetime-input-business-hours")

            | Text.Monospaced("Time Slots")
            | Text.Block("15-minute intervals")
            | timeSlotState
                .ToTimeInput()
                .Step(TimeSpan.FromMinutes(15))
                .Placeholder("Select time slot")
                .TestId("datetime-input-time-slots")

            | Text.Monospaced("Month in year")
            | Text.Block("Min Jan / max Dec of current calendar year")
            | constrainedMonthState
                .ToMonthInput()
                .Min(new DateTime(DateTime.Now.Year, 1, 1))
                .Max(new DateTime(DateTime.Now.Year, 12, 1))
                .Placeholder("Select month")
                .TestId("datetime-input-constraints-month")

            | Text.Monospaced("Week window")
            | Text.Block("ISO weeks from 6 months ago to 6 months ahead")
            | constrainedWeekState
                .ToWeekInput()
                .Min(DateTime.Today.AddMonths(-6))
                .Max(DateTime.Today.AddMonths(6))
                .Placeholder("Select week")
                .TestId("datetime-input-constraints-week")

            | Text.Monospaced("Year range")
            | Text.Block("Years 2020 through 2035")
            | constrainedYearState
                .ToYearInput()
                .Min(new DateTime(2020, 1, 1))
                .Max(new DateTime(2035, 12, 31))
                .Placeholder("Select year")
                .TestId("datetime-input-constraints-year")

            | Text.Monospaced("Meeting slots")
            | Text.Block("Same calendar day, 9 AM-5 PM, 30-minute steps")
            | meetingSlotState
                .ToDateTimeInput()
                .Min(DateTime.Today.AddHours(9))
                .Max(DateTime.Today.AddHours(17))
                .Step(TimeSpan.FromMinutes(30))
                .Placeholder("Book a slot")
                .TestId("datetime-input-meeting-slots")

            | Text.Monospaced("Lunch reservation")
            | Text.Block("11:30 AM-1:30 PM, 15-minute steps")
            | lunchTimeState
                .ToTimeInput()
                .Min(DateTime.Today.AddHours(11).AddMinutes(30))
                .Max(DateTime.Today.AddHours(13).AddMinutes(30))
                .Step(TimeSpan.FromMinutes(15))
                .Placeholder("Pick lunch time")
                .TestId("datetime-input-lunch-time");

        var sampleValues = Layout.Vertical()
            | Text.H3("Current values (this tab)")
            | Text.Block($"Date (today..+90d): {constrainedDateState.Value:yyyy-MM-dd}")
            | Text.Block($"Business hours DateTime: {appointmentState.Value:yyyy-MM-dd HH:mm}")
            | Text.Block($"Time slot (15m step): {timeSlotState.Value:HH:mm}")
            | Text.Block($"Month (in calendar year): {constrainedMonthState.Value:yyyy-MM}")
            | Text.Block($"Week (±6 months): {constrainedWeekState.Value:yyyy-'W'ww}")
            | Text.Block($"Year (2020-2035): {constrainedYearState.Value:yyyy}")
            | Text.Block($"Meeting (30m step, 9-5): {meetingSlotState.Value:yyyy-MM-dd HH:mm}")
            | Text.Block($"Lunch time (11:30-13:30, 15m): {lunchTimeState.Value:HH:mm}");

        return Layout.Vertical()
            | Text.H2("Min / max / step")
            | Text.P("Range and stepping constraints across date, time, and calendar variants.")
            | constraintsGrid
            | sampleValues;
    }
}
