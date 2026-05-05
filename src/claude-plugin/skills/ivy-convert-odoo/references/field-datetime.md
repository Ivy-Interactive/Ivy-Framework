# Field: Date / Datetime

Odoo's date and datetime fields for temporal values. `fields.Date` stores date only, `fields.Datetime` includes time. Supports min/max constraints, date ranges, and various picker modes.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    date_order = fields.Datetime(string='Order Date', required=True,
                                  default=fields.Datetime.now, copy=False)
    validity_date = fields.Date(string='Expiration Date')
    commitment_date = fields.Datetime(string='Delivery Date')
    create_date = fields.Datetime(string='Creation Date', readonly=True)
    expected_date = fields.Datetime(string='Expected Date', compute='_compute_expected')

class HrLeave(models.Model):
    _name = 'hr.leave'

    date_from = fields.Datetime(string='Start Date', required=True)
    date_to = fields.Datetime(string='End Date', required=True)
    request_date_from = fields.Date(string='Request Start Date')
    request_date_to = fields.Date(string='Request End Date')
```

```xml
<!-- Date field -->
<field name="validity_date"/>

<!-- Datetime field -->
<field name="date_order"/>

<!-- With min/max date constraints -->
<field name="validity_date" options="{'min_date': 'today'}"/>
<field name="commitment_date" options="{'min_date': '2024-01-01', 'max_date': '2025-12-31'}"/>

<!-- Date range widget -->
<field name="date_from" widget="daterange" options="{'end_date_field': 'date_to'}"/>

<!-- Warn on future dates -->
<field name="date_order" options="{'warn_future': true}"/>

<!-- Remaining days display -->
<field name="validity_date" widget="remaining_days"/>

<!-- In list view -->
<field name="date_order" widget="date" optional="show"/>
```

## Ivy

```csharp
// Date → DateTimeInput with Date variant
var validityDate = UseState<DateOnly?>(null);
validityDate.ToDateInput()
    .Placeholder("Select expiration date...")
    .WithField()
    .Label("Expiration Date");

// Datetime → DateTimeInput with DateTime variant
var dateOrder = UseState(DateTime.Now);
dateOrder.ToDateTimeInput()
    .WithField()
    .Label("Order Date")
    .Required();

// Date range → DateRangeInput
var dateRange = UseState<(DateOnly?, DateOnly?)>((null, null));
dateRange.ToDateRangeInput()
    .StartPlaceholder("Start Date")
    .EndPlaceholder("End Date")
    .WithField()
    .Label("Leave Period");

// Time only → DateTimeInput with Time variant
var meetingTime = UseState<TimeOnly?>(null);
meetingTime.ToTimeInput()
    .WithField()
    .Label("Meeting Time");

// Month/Year picker variants
var reportMonth = UseState<DateOnly?>(null);
reportMonth.ToMonthInput()
    .WithField()
    .Label("Report Month");

// Read-only date display
new TextBlock(dateOrder.Value.ToString("MMM dd, yyyy HH:mm"));

// In form state class
public class OrderFormState
{
    [Required]
    [Display(Name = "Order Date")]
    public DateTime DateOrder { get; set; } = DateTime.Now;

    [Display(Name = "Expiration Date")]
    public DateOnly? ValidityDate { get; set; }

    [Display(Name = "Delivery Date")]
    public DateTime? CommitmentDate { get; set; }
}
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `fields.Date` | Date only field | `DateTimeInput` with Date variant or `.ToDateInput()` |
| `fields.Datetime` | Date and time field | `DateTimeInput` with DateTime variant or `.ToDateTimeInput()` |
| `string="..."` | Field label | `.WithField().Label("...")` |
| `required=True` | Mandatory field | `.Required()` or `[Required]` |
| `default=fields.Date.today` | Default to today | Initial `UseState(DateOnly.FromDateTime(DateTime.Today))` |
| `default=fields.Datetime.now` | Default to now | Initial `UseState(DateTime.Now)` |
| `options="{'min_date': 'today'}"` | Minimum date constraint | Validation in handler or form |
| `options="{'max_date': '...'}"` | Maximum date constraint | Validation in handler or form |
| `options="{'warn_future': true}"` | Warn on future dates | Custom validation warning |
| `widget="daterange"` | Date range picker | `DateRangeInput` |
| `options="{'end_date_field': '...'}"` | End date for range | Second field in DateRangeInput tuple |
| `options="{'rounding': 15}"` | Time rounding (minutes) | Time step configuration |
| `options="{'show_time': false}"` | Hide time component | Use Date variant instead of DateTime |
| `options="{'show_seconds': true}"` | Show seconds | DateTimeInput seconds display |
| `widget="remaining_days"` | Days until/since display | Custom computed display |
| `readonly=True` | Read-only display | `.Disabled(true)` or `TextBlock` with date format |
| `attrs="{'invisible':...}"` | Conditional visibility | `.Visible(condition)` |
