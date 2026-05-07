# Separator / Divider

A visual divider between content sections. In Lovable apps, this is the shadcn/ui Separator component.

## Lovable

```tsx
import { Separator } from "@/components/ui/separator";

<Separator />
<Separator orientation="vertical" />
```

## Ivy

```csharp
new Divider();
new Divider(DividerOrientation.Vertical);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `orientation` | `"horizontal" \| "vertical"`. Default: `"horizontal"` | `DividerOrientation` enum |
| `className` | Tailwind CSS classes | Not supported |
