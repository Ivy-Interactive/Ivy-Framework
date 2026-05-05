# Popover

A floating content panel triggered by a click. In Lovable apps, this is the shadcn/ui Popover component.

## Lovable

```tsx
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

<Popover>
  <PopoverTrigger asChild>
    <Button variant="outline">Open</Button>
  </PopoverTrigger>
  <PopoverContent className="w-80">
    <div className="space-y-2">
      <h4 className="font-medium">Settings</h4>
      <p className="text-sm text-muted-foreground">Configure preferences.</p>
    </div>
  </PopoverContent>
</Popover>
```

## Ivy

```csharp
new Popover(
    new Button("Open").Outline(),
    new Column(
        new Heading("Settings", HeadingLevel.H4),
        new Text("Configure preferences.").Muted()
    ).Gap(2)
);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `PopoverTrigger` | Element that opens the popover | First child of `Popover` |
| `PopoverContent` | Floating content panel | Remaining children of `Popover` |
| `side` | `"top" \| "right" \| "bottom" \| "left"` | Not supported (automatic) |
| `align` | `"start" \| "center" \| "end"` | Not supported (automatic) |
