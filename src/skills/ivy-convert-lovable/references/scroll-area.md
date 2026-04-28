# ScrollArea

A scrollable container with custom scrollbars. In Lovable apps, this is the shadcn/ui ScrollArea component.

## Lovable

```tsx
import { ScrollArea } from "@/components/ui/scroll-area";

<ScrollArea className="h-72 w-full rounded-md border">
  <div className="p-4">
    {items.map(item => <div key={item.id}>{item.name}</div>)}
  </div>
</ScrollArea>
```

## Ivy

Ivy handles scrolling automatically within layout containers. No explicit scroll area widget is needed.

```csharp
// Content automatically scrolls within its container
new Column(
    items.Select(item => new Text(item.Name))
).Height(300);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `className` height | Fixed height container | `.Height()` on parent container |
| Scroll behavior | Custom scrollbars | Automatic |
