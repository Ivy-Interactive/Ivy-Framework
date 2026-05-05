# Date Picker

A date selection input. Lovable apps typically use a custom DatePicker built on shadcn/ui Popover + Calendar components.

## Lovable

```tsx
import { format } from "date-fns";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { CalendarIcon } from "lucide-react";

const [date, setDate] = useState<Date>();

<Popover>
  <PopoverTrigger asChild>
    <Button variant="outline">
      <CalendarIcon className="mr-2 h-4 w-4" />
      {date ? format(date, "PPP") : "Pick a date"}
    </Button>
  </PopoverTrigger>
  <PopoverContent>
    <Calendar mode="single" selected={date} onSelect={setDate} />
  </PopoverContent>
</Popover>
```

## Ivy

```csharp
var date = UseState<DateTime?>(null);

new DatePicker("Date", date, placeholder: "Pick a date");

// Date range
var dateRange = UseState<DateRange?>(null);
new DateRangePicker("Period", dateRange);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `selected` | Selected date | Bound state parameter |
| `onSelect` | Selection callback | Automatic via state binding |
| `mode` | `"single" \| "range" \| "multiple"` | `DatePicker` (single) or `DateRangePicker` (range) |
| `disabled` | Disable date selection | `Disabled` (bool) |
| Placeholder text | Button label when no selection | `Placeholder` (string) |
