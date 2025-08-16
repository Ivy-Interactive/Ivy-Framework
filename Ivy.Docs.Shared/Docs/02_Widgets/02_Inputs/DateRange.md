# DateRangeInput

<Ingress>
Select date ranges with an intuitive calendar interface for start and end dates, perfect for filtering and event scheduling.
</Ingress>

The `DateRangeInput` widget allows users to select a range of dates. It provides a calendar interface for both start and end date selection, making it ideal for filtering data by date ranges or scheduling events.

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
                | Text.Large(span);
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

The `DateRangeInput`can be customized with various states:

### Disabled State

To render a date time range in the disabled state the `Disabled` function
should be used.

```csharp demo-below 
public class DisabledDateRange : ViewBase
{   
    public override object? Build()
    {    
        var dateRangeState = this.UseState(() => (from: DateTime.Today.AddDays(-7), to: DateTime.Today));
        return Layout.Vertical()
                | dateRangeState.ToDateRangeInput().Disabled();
    }
}    
```

### Invalid State

To render a `DateTimeRange` in the invalid state the `Invalid` function
should be used.

```csharp demo-below 
public class InvalidDateRangeDemo : ViewBase 
{    
    public override object? Build()
    {    
        var dateRangeState = this.UseState(() => (from: DateTime.Today.AddDays(-7), to: DateTime.Today));
        return Layout.Vertical()
                | dateRangeState.ToDateRangeInput().Invalid("Invalid date range");
    }
}    
```

### Nullable State

Sometimes it is necessary to render a date time with possible nullable from and to dates.
The following demo shows how this can be done.

```csharp demo-below 
public class NullableDateRangeDemo : ViewBase
{
    public override object? Build()
    {    
        var nullableRange = this.UseState<(DateOnly?, DateOnly?)>(() => 
            (DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), 
             DateOnly.FromDateTime(DateTime.Today)));
        return Layout.Vertical()
                |  nullableRange.ToDateRangeInput();
    }    
}
```

## Styling

`DateRangeInput` can be customized with various styling options:

### Placeholder

A friendly placeholder text can be used to give users a clue about what the data range depicts.

```csharp demo-below 
public class DateRangePlaceHolderDemo : ViewBase 
{   
    public override object? Build()
    {    
        var dateRangeState = this.UseState(() => 
            (from: DateTime.Today.AddDays(-7), to: DateTime.Today));
        return Layout.Vertical()
                | dateRangeState.ToDateRangeInput().Placeholder("Select a date range");
    }
}    
```

### Format

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

### Complete Example with All Features

```csharp demo-tabs 
public class DateRangeRealisticDemo : ViewBase
{
    public override object? Build()
    {
      var leaveRangeState = this.UseState(() =>
            (from: DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), 
            to: DateOnly.FromDateTime(DateTime.Today)));

        var toDateTime = new DateTime(leaveRangeState.Value.Item2.Year,
            leaveRangeState.Value.Item2.Month, leaveRangeState.Value.Item2.Day);
        
        var fromDateTime = new DateTime(leaveRangeState.Value.Item1.Year,
            leaveRangeState.Value.Item1.Month, leaveRangeState.Value.Item1.Day);

        
        
        int saturdays = 0;
        int sundays = 0;
        for(var d = fromDateTime ; d <= toDateTime; d=d.AddDays(1))
        {
            if(d.DayOfWeek == DayOfWeek.Saturday)
                saturdays++;
            if(d.DayOfWeek == DayOfWeek.Sunday)
                sundays++;
        }
        var moreThanTwoWeeks = (toDateTime - fromDateTime).Days - (saturdays + sundays) > 10;
        var invalidLeave = UseState("");
        if (moreThanTwoWeeks)
            invalidLeave.Set("Only two consecutive weeks allowed!");    
        else
            invalidLeave.Set(string.Empty);
                            
        return Layout.Vertical()
                | H3("Select Leave Range")
                | leaveRangeState.ToDateRangeInput().Invalid(invalidLeave.Value);
    }    
}    
```
