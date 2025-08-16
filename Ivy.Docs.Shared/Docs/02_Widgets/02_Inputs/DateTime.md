# DateTimeInput

<Ingress>
Capture dates and times with intuitive picker interfaces supporting calendar selection, time input, and combined date-time entry.
</Ingress>

The `DateTimeInput` widget provides a comprehensive date and time picker interface with support for different variants. It allows users to select dates from a calendar, times from a time selector, or both date and time together, making it ideal for scheduling, event creation, and form inputs.

## Basic Usage

Here's a simple example of a DateTimeInput that allows users to select a date and time:

```csharp demo-below 
public class BasicDateTimeUsageDemo : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Today);
        var daysBetween = dateState.Value.Subtract(DateTime.Today).Days;
        return Layout.Vertical() 
                | Text.Large("When is your birthday?")
                | dateState.ToDateTimeInput()
                           .Variant(DateTimeInputs.Date)
                | Text.Html($"<i>That's <b>{daysBetween}</b> days from now!");
    }
}    
```

## Variants

`DateTimeInput` supports three variants to suit different use cases:

### Date Variant

The Date variant provides a calendar picker for selecting dates only.

```csharp
dateState.ToDateTimeInput().Variant(DateTimeInputs.Date)
```

Instead of using `DateTimeInputs.Date` the function `ToDateInput` should generally be used.

### DateTime Variant

The DateTime variant combines a calendar picker with a time input field.

```csharp
dateState.ToDateTimeInput().Variant(DateTimeInputs.DateTime)
```

Instead of using `DateTimeInputs.DateTime` the function `ToDateTimeInput` should generally be used.

### Time Variant

The Time variant provides a time picker for selecting time only.

```csharp
dateState.ToDateTimeInput().Variant(DateTimeInputs.Time)
```

Instead of using `DateTimeInputs.Time` the function `ToTimeInput` should generally be used.  

The following demo shows all of these in action.

```csharp demo-below 
public class DateTimeVariantsDemo : ViewBase
{    
    public override object? Build()
    {    
        var dateState = UseState(DateTime.Today.Date);
        var timeState = UseState(DateTime.Now);
        var dateTimeState = UseState(DateTime.Today);
        
        return Layout.Vertical()
                | (Layout.Horizontal()
                    | Text.Small("Date")
                          .Width(35)
                    | dateState.ToDateInput()
                           .Format("dd/MM/yyyy"))
                | (Layout.Horizontal()
                    | Text.Small("DateTime")
                          .Width(35)
                    | dateTimeState.ToDateTimeInput()
                           .Format("dd/MM/yyyy HH:mm:ss"))
                | (Layout.Horizontal()
                    | Text.Small("Time")
                          .Width(35)
                    | timeState.ToTimeInput());
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

## Event Handling

`DateTimeInput` can handle change and blur events:

```csharp
var dateState = this.UseState(DateTime.Now);
var onChangeHandler = (Event<IInput<DateTime>, DateTime> e) =>
{
    dateState.Set(e.Value);
};
return dateState.ToDateTimeInput().OnChange(onChangeHandler);
```

## Styling

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
                 | (Layout.Horizontal()
                     | Text.Small("MM/dd/yyyy")
                           .Width(25) 
                     | monthDateYear.ToDateInput()
                                    .Format("MM/dd/yyyy"))
                | (Layout.Horizontal()
                    | Text.Small("yyyy/MMM/dd")
                          .Width(25)
                    | yearMonthDate.ToDateInput()
                                   .Placeholder("yyyy/MMM/dd")
                                   .Format("yyyy/MMM/dd"));
    }
}    
```

### Invalid

To represent that something might be wrong with a date input the function `Invalid`
should be used. The following code shows a demonstration.

```csharp demo-below 
public class InvalidDateTimeDemo : ViewBase
{
    public override object? Build()
    {
        var thisDate = UseState(DateTime.Today.Date.AddDays(8));
        return Layout.Vertical()
                | Text.Large("Return date")
                | thisDate.ToDateInput()
                          .Invalid("Date is beyond the last approved date!");
    }
}

```

### Disabled

To disable a `DateTimeInput` the `Disabled` function should be used.

```csharp demo-below 
public class DisabledDateTimeDemo : ViewBase
{
    public override object? Build()
    {
        var disabledDate = UseState(DateTime.Today.Date);
        return Layout.Vertical()
                | disabledDate.ToDateInput()
                              .Disabled();
    }
}
```

<WidgetDocs Type="Ivy.DateTimeInput" ExtensionTypes="Ivy.DateTimeInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/DateTimeInput.cs"/>

## Examples

```csharp demo-tabs 
public class LibraryBookReturnDemo : ViewBase
{
   
    public override object? Build()
    {
        var issueDate = UseState(DateTime.Today.Date);
        // Library book returns must be within a week 
        var returnDate = UseState(DateTime.Today.AddDays(7).Date);
        var actualReturnDate = UseState(DateTime.Today.Date);
        var fineDays = actualReturnDate.Value.Subtract(returnDate.Value).Days;
        var invalidMessage = UseState(String.Empty);
        if(fineDays > 0)
        {
            invalidMessage.Set($"Book is <b>{fineDays}<b> days overdue!");
        }
        else
        {
            invalidMessage.Set(String.Empty);
        }
        return Layout.Vertical()
                | Icons.Book    
                | H3("Library Book Return")
                | Text.Small("Library book returns must be within a week")
                | Text.Large("Issue Date")
                | issueDate.ToDateInput()
                           .Disabled()
                | Text.Large("Return Date")
                | returnDate.ToDateInput()
                            .Disabled()
                | actualReturnDate.ToDateInput()
                                    .Invalid(invalidMessage.Value);
    }    
}

```
