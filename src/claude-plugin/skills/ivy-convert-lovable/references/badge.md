# Badge

A small status indicator label. In Lovable apps, this is the shadcn/ui Badge component.

## Lovable

```tsx
import { Badge } from "@/components/ui/badge";

<Badge>Default</Badge>
<Badge variant="secondary">Secondary</Badge>
<Badge variant="destructive">Error</Badge>
<Badge variant="outline">Outline</Badge>
```

## Ivy

```csharp
new Badge("Default");
new Badge("Secondary").Secondary();
new Badge("Error").Destructive();
new Badge("Outline").Outline();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `children` | Badge label text | First parameter (string) |
| `variant` | `"default" \| "secondary" \| "destructive" \| "outline"` | `Variant` (`BadgeVariant`) |
| `className` | Tailwind CSS classes | Not supported |
