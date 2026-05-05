# Sheet

A slide-out panel from the edge of the screen, used for forms, details, or secondary content. In Lovable apps, this is the shadcn/ui Sheet component.

## Lovable

```tsx
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";

const [open, setOpen] = useState(false);

<Sheet open={open} onOpenChange={setOpen}>
  <SheetTrigger asChild>
    <Button>Open Sheet</Button>
  </SheetTrigger>
  <SheetContent side="right">
    <SheetHeader>
      <SheetTitle>Edit Item</SheetTitle>
      <SheetDescription>Update the item details.</SheetDescription>
    </SheetHeader>
    <Input placeholder="Name" />
    <Button onClick={handleSave}>Save</Button>
  </SheetContent>
</Sheet>
```

## Ivy

```csharp
var (sheet, openSheet) = UseSheet("Edit Item", SheetContent);

new Button("Open Sheet", onClick: e => openSheet());
sheet; // render the sheet view

IEnumerable<object> SheetContent()
{
    yield return new Text("Update the item details.");
    yield return name.ToTextInput().Placeholder("Name");
    yield return new Button("Save", onClick: e => HandleSave());
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `open` | Controlled open state | Managed by `UseSheet` hook |
| `onOpenChange` | Callback when open state changes | Managed by `UseSheet` hook |
| `side` | `"top" \| "right" \| "bottom" \| "left"`. Default: `"right"` | Not supported (always right) |
| `SheetTitle` | Panel header title | First parameter of `UseSheet` |
| `SheetDescription` | Description text below title | Rendered as `Text` in sheet content |
| `SheetContent` | Panel body content | Content callback function |
| `SheetTrigger` | Element that opens the sheet | Call `openSheet()` from any event handler |
