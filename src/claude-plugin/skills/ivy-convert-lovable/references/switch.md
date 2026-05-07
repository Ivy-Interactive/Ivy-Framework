# Switch

A toggle switch for boolean values. In Lovable apps, this is the shadcn/ui Switch component.

## Lovable

```tsx
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";

const [enabled, setEnabled] = useState(false);

<div className="flex items-center space-x-2">
  <Switch id="notifications" checked={enabled} onCheckedChange={setEnabled} />
  <Label htmlFor="notifications">Enable notifications</Label>
</div>
```

## Ivy

```csharp
var enabled = UseState(false);

new Switch("Enable notifications", enabled);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `checked` | Whether on. Default: `false` | Bound state parameter |
| `onCheckedChange` | Callback when toggled | Automatic via state binding |
| `disabled` | Disables interaction. Default: `false` | `Disabled` (bool) |
| `id` | HTML id for label association | Not needed |
| `Label` | Associated label text | First parameter of `Switch` |
