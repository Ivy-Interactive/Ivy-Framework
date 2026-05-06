# Select

A dropdown selection control. In Lovable apps, this is the shadcn/ui Select component.

## Lovable

```tsx
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

<Select value={status} onValueChange={setStatus}>
  <SelectTrigger>
    <SelectValue placeholder="Select status" />
  </SelectTrigger>
  <SelectContent>
    <SelectItem value="active">Active</SelectItem>
    <SelectItem value="inactive">Inactive</SelectItem>
    <SelectItem value="pending">Pending</SelectItem>
  </SelectContent>
</Select>
```

## Ivy

```csharp
var status = UseState("");

new Select("Status", status, new[]
{
    new SelectOption("active", "Active"),
    new SelectOption("inactive", "Inactive"),
    new SelectOption("pending", "Pending"),
}).Placeholder("Select status");

// With enum
new Select<StatusEnum>("Status", statusState);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `value` | Selected value | Bound state parameter |
| `onValueChange` | Change callback | Automatic via state binding |
| `placeholder` | Placeholder text when no selection | `Placeholder` (string) |
| `disabled` | Disables selection. Default: `false` | `Disabled` (bool) |
| `SelectItem.value` | Option value | `SelectOption` value parameter |
| `SelectItem.children` | Option display text | `SelectOption` label parameter |
