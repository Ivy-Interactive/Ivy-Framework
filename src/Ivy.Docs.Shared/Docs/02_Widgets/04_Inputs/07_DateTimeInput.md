---
searchHints:
  - calendar
  - date
  - time
  - picker
  - datetime
  - timestamp
  - month
  - week
  - year
---

# DateTimeInput

<Ingress>
Capture dates and times with intuitive picker [interfaces](../../01_Onboarding/02_Concepts/02_Views.md) supporting calendar selection, time input, and combined date-time entry.
</Ingress>

The `DateTimeInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a comprehensive date and time picker interface with support for different variants. It allows users to select dates, months, weeks, years from a calendar, times from a time selector, or both date and time together, making it ideal for scheduling, event creation, reporting periods, and [form](../../01_Onboarding/02_Concepts/08_Forms.md) inputs.

## Basic Usage

Here's a simple example of a DateTimeInput that allows users to select a date:

```csharp demo-below
public class BasicDateUsageDemo : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Today);
        var daysBetween = dateState.Value.Subtract(DateTime.Today).Days;
        return Layout.Vertical()
                | dateState.ToDateInput()
                           .WithField()
                           .Label("When is your birthday?")
                | Text.Html($"<i>That's <b>{daysBetween}</b> days from now!");
    }
}
```

## Variants

`DateTimeInput` supports six variants: Date, DateTime, Time, Month, Week, and Year. The following extension methods are available:

- `ToDateInput()`: Calendar picker for dates only.
- `ToDateTimeInput()`: Calendar picker with time input.
- `ToTimeInput()`: Time picker only.
- `ToMonthInput()`: Month picker with year navigation; selects the 1st of the chosen month.
- `ToWeekInput()`: Calendar with week numbers; selects the Monday of the chosen week.
- `ToYearInput()`: Year picker with decade navigation; selects January 1st of the chosen year.

The following demo shows the core variants in action:

```csharp demo-below
public class DateTimeVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Today.Date);
        var timeState = UseState(DateTime.Now);
        var dateTimeState = UseState(DateTime.Today);
        var monthState = UseState(DateTime.Today);
        var weekState = UseState(DateTime.Today);
        var yearState = UseState(DateTime.Today);

        return Layout.Vertical()
                | dateState.ToDateInput()
                       .Format("dd/MM/yyyy")
                       .WithField()
                       .Label("Date")
                | dateTimeState.ToDateTimeInput()
                       .Format("dd/MM/yyyy HH:mm:ss")
                       .WithField()
                       .Label("DateTime")
                | timeState.ToTimeInput()
                       .WithField()
                       .Label("Time")
                | monthState.ToMonthInput()
                       .WithField()
                       .Label("Month")
                | weekState.ToWeekInput()
                       .WithField()
                       .Label("Week")
                | yearState.ToYearInput()
                       .WithField()
                       .Label("Year");
    }
}
```

## Supported State Types

`DateTimeInput` supports various date and time types:

- `DateTime` and `DateTime?`
- `DateTimeOffset` and `DateTimeOffset?`
- `DateOnly` and `DateOnly?`
- `TimeOnly` and `TimeOnly?`
- `string` (for ISO format)

## Format

`DateTimeInput` can be customized with various formats. So the captured value can be
expressed in any format as supported by .NET.

```csharp demo-below
public class FormatDemo : ViewBase
{
     public override object? Build()
     {
         var monthDateYear = UseState(DateTime.Today.Date);
         var yearMonthDate = UseState(DateTime.Today.Date);

         return Layout.Vertical()
                | monthDateYear.ToDateInput()
                                .Format("MM/dd/yyyy")
                                .WithField()
                                .Label("MM/dd/yyyy")
                | yearMonthDate.ToDateInput()
                                .Placeholder("yyyy/MMM/dd")
                                .Format("yyyy/MMM/dd")
                                .WithField()
                                .Label("yyyy/MMM/dd");
    }
}
```

## Event Handling

Date and time inputs support focus, blur, and manual `AutoFocus` behavior.

```csharp demo-tabs
public class DateTimeInputEventsDemo : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var state = UseState(DateTime.Now);

        return Layout.Tabs(
            new Tab("OnFocus", Layout.Vertical()
                | Text.P("The OnFocus event fires when the date picker gains focus.")
                | state.ToDateInput().Placeholder("Focus me...")
                    .OnFocus(() => focusCount.Set(focusCount.Value + 1))
                | Text.Literal($"Focus Count {focusCount.Value}")
            ),
            new Tab("OnBlur", Layout.Vertical()
                | Text.P("The OnBlur event fires when the date picker loses focus.")
                | state.ToDateInput().Placeholder("Blur me...")
                    .OnBlur(() => blurCount.Set(blurCount.Value + 1))
                | Text.Literal($"Blur Count {blurCount.Value}")
            ),
            new Tab("AutoFocus", Layout.Vertical()
                | Text.P("The AutoFocus property automatically focuses the date input and opens the calendar popover upon mounting.")
                | state.ToDateInput().Placeholder("AutoFocused DatePicker")
                    .AutoFocus()
                | Text.Lead("Focused & Open!")
            )
        ).Variant(TabsVariant.Tabs);
    }
}
```

<WidgetDocs Type="Ivy.DateTimeInput" ExtensionTypes="Ivy.DateTimeInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/DateTimeInput.cs"/>
