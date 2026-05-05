# Button

A clickable button that triggers actions. In Lovable apps, this is the shadcn/ui Button component.

## Lovable

```tsx
import { Button } from "@/components/ui/button";

<Button onClick={() => console.log("clicked")}>Click me</Button>
<Button variant="destructive">Delete</Button>
<Button variant="outline">Cancel</Button>
<Button variant="ghost">Settings</Button>
<Button variant="link">Learn more</Button>
<Button disabled>Disabled</Button>
<Button size="sm">Small</Button>
<Button size="lg">Large</Button>
<Button size="icon"><Trash className="h-4 w-4" /></Button>
```

## Ivy

```csharp
new Button("Click me", onClick: e => Console.WriteLine("clicked"));
new Button("Delete").Destructive();
new Button("Cancel").Outline();
new Button("Settings").Ghost();
new Button("Learn more").Link();
new Button("Disabled", disabled: true);
new Button().Icon(Icons.Trash).Ghost(); // icon-only button
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `children` | Button label text or elements | `Title` (string) |
| `onClick` | Click event handler | `OnClick` (`Func<Event<Button>, ValueTask>`) |
| `variant` | `"default" \| "destructive" \| "outline" \| "secondary" \| "ghost" \| "link"` | `Variant` (`ButtonVariant`) — Primary, Secondary, Destructive, Outline, Ghost, Link |
| `size` | `"default" \| "sm" \| "lg" \| "icon"` | `Scale` (`Scale?`) |
| `disabled` | Disables interaction. Default: `false` | `Disabled` (bool) |
| `asChild` | Merges props onto child element | Not supported |
| `className` | Tailwind CSS classes | Not supported (use Ivy styling methods) |
