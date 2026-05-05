# Input

A text input field for user data entry. In Lovable apps, this is the shadcn/ui Input component, often used with react-hook-form and zod validation.

## Lovable

```tsx
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

<div>
  <Label htmlFor="email">Email</Label>
  <Input id="email" type="email" placeholder="Enter your email" />
</div>

// With react-hook-form
import { useForm } from "react-hook-form";
const { register } = useForm();
<Input {...register("email")} placeholder="Email" />

// Controlled
const [value, setValue] = useState("");
<Input value={value} onChange={(e) => setValue(e.target.value)} />
```

## Ivy

```csharp
var email = UseState("");
email.ToTextInput().Placeholder("Enter your email");

// With validation
email.ToTextInput().Placeholder("Enter your email")
    .Required()
    .Email();

// Password input
var password = UseState("");
password.ToTextInput(type: TextInputType.Password);

// Number input
var age = UseState(0);
age.ToNumberInput();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `value` | Current input value | Bound state parameter |
| `onChange` | Change event handler | Automatic via state binding |
| `placeholder` | Placeholder text | `Placeholder` (string) |
| `type` | `"text" \| "email" \| "password" \| "number" \| "tel" \| "url"` | `TextInputType` or use `NumberInput` for numbers |
| `disabled` | Disables input. Default: `false` | `Disabled` (bool) |
| `required` | HTML required attribute | `.Required()` validation |
| `className` | Tailwind CSS classes | Not supported |
| `id` | HTML id attribute | Not applicable |
