---
searchHints:
  - calendar
  - date
  - picker
  - range
  - period
  - dates
---

# DateRangeInput

<Ingress>
Select date ranges with an intuitive calendar [interface](../../01_Onboarding/02_Concepts/02_Views.md) for start and end dates, perfect for filtering and event scheduling.
</Ingress>

The `DateRangeInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) allows users to select a range of dates. It provides a calendar interface for both start and end date selection, making it ideal for filtering data by date ranges or scheduling events.

## Basic Usage

Here's a simple example of a `DateRangeInput` that allows users to select a date range:

```csharp demo-below
public class BasicDateRangeDemo : ViewBase
{
    public override object? Build()
    {    
        var dateRangeState = this.UseState(() => (from: DateTime.Today.AddDays(-7), to: DateTime.Today));
        var start = dateRangeState.Value.Item1;
        var end = dateRangeState.Value.Item2;
        var span = $"That's {(end-start).Days} days";
        return Layout.Vertical()
                | dateRangeState.ToDateRangeInput()
                | Text.P(span).Large();
    }    
}        
```

As can be seen, the starting and ending date of the date range can be extracted using the
`DateTimeRange.Value.Item1` and `DateTimeRange.Value.Item2`

## Supported Types

DateRangeInput supports DateOnly tuple types:

- `(DateOnly, DateOnly)` - Date-only range
- `(DateOnly?, DateOnly?)` - Nullable date-only range

## Variants

The `DateRangeInput` can be customized with various states. The following example demonstrates **Disabled**, **Invalid**, and **Nullable** states:

```csharp demo-below
public class DateRangeVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var dateRange = this.UseState(() => 
            (from: DateTime.Today.AddDays(-7), to: DateTime.Today));
        
        var nullableRange = this.UseState<(DateOnly?, DateOnly?)>(() => 
            (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), 
             DateOnly.FromDateTime(DateTime.Today)));

        return Layout.Vertical().Gap(4)
            | Text.P("Disabled State").Small()
            | dateRange.ToDateRangeInput().Disabled()
            | Text.P("Invalid State").Small()
            | dateRange.ToDateRangeInput().Invalid("Invalid date range")
            | Text.P("Nullable State").Small()
            | nullableRange.ToDateRangeInput();
    }
}
```

## Format

To change the format of selected dates the `Format` function needs to be used.

```csharp demo-below
public class FormatDateRangeDemo : ViewBase
{
    public override object? Build()
    {   
         var dateRangeState = this.UseState(() => 
            (from: DateTime.Today.AddDays(-7), to: DateTime.Today));
         return Layout.Vertical()
                 | dateRangeState.ToDateRangeInput()
                                  .Format("yyyy-MM-dd");
    }    
}        
```

<WidgetDocs Type="Ivy.DateRangeInput" ExtensionTypes="Ivy.DateRangeInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/DateRangeInput.cs"/>

## Examples

<Details>
<Summary>
Hotel Booking Example
</Summary>
<Body>

A realistic example demonstrating a hotel booking form with date range selection, validation, and price calculation:

```csharp demo-tabs
public class HotelBookingDemo : ViewBase
{
    private const decimal PricePerNight = 120m;
    
    public override object? Build()
    {
        var bookingRange = this.UseState<(DateOnly?, DateOnly?)>(() => (null, null));
        
        var from = bookingRange.Value.Item1;
        var to = bookingRange.Value.Item2;
        
        var nights = from.HasValue && to.HasValue 
            ? (to.Value.ToDateTime(TimeOnly.MinValue) - from.Value.ToDateTime(TimeOnly.MinValue)).Days 
            : 0;
        
        var isValid = nights >= 1;
        var errorMessage = !isValid && from.HasValue ? "Minimum stay is 1 night" : "";
        var totalPrice = nights * PricePerNight;

        return Layout.Vertical().Gap(4)
            | Text.P("Book Your Stay").Large().Bold()
            | bookingRange.ToDateRangeInput()
                .Placeholder("Select check-in and check-out dates")
                .Format("MMM dd, yyyy")
                .Invalid(errorMessage)
            | (nights > 0 
                ? Layout.Horizontal().Gap(2)
                    | Text.P($"{nights} night(s)").Small().Muted()
                    | Text.P($"Total: ${totalPrice}").Small().Bold()
                : null);
    }
}
```

</Body>
</Details>
