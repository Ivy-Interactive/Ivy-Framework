# Textarea

A multi-line text input. In Lovable apps, this is the shadcn/ui Textarea component.

## Lovable

```tsx
import { Textarea } from "@/components/ui/textarea";

const [value, setValue] = useState("");

<Textarea
  placeholder="Type your message here."
  value={value}
  onChange={(e) => setValue(e.target.value)}
  rows={4}
/>
```

## Ivy

```csharp
var value = UseState("");

new TextArea("Message", value, placeholder: "Type your message here.")
    .Rows(4);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `value` | Current text value | Bound state parameter |
| `onChange` | Change handler | Automatic via state binding |
| `placeholder` | Placeholder text | `Placeholder` (string) |
| `rows` | Visible row count | `.Rows(int)` |
| `disabled` | Disables input. Default: `false` | `Disabled` (bool) |
