# Checkbox

A toggle control for boolean values. In Lovable apps, this is the shadcn/ui Checkbox component.

## Lovable

```tsx
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";

const [checked, setChecked] = useState(false);

<div className="flex items-center space-x-2">
  <Checkbox
    id="terms"
    checked={checked}
    onCheckedChange={(value) => setChecked(value as boolean)}
  />
  <Label htmlFor="terms">Accept terms and conditions</Label>
</div>
```

## Ivy

```csharp
var accepted = UseState(false);

new Checkbox("Accept terms and conditions", accepted);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `checked` | Whether checked. Default: `false` | Bound state parameter |
| `onCheckedChange` | Callback when state changes | Automatic via state binding |
| `disabled` | Disables interaction. Default: `false` | `Disabled` (bool) |
| `id` | HTML id for label association | Not needed (label is built-in) |
| `Label` | Associated label text | First parameter of `Checkbox` |
