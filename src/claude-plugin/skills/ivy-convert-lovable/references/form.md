# Form (react-hook-form + zod)

Form handling with validation. Lovable apps use react-hook-form with zod schema validation, wrapped in shadcn/ui Form components.

## Lovable

```tsx
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import {
  Form, FormControl, FormDescription, FormField,
  FormItem, FormLabel, FormMessage,
} from "@/components/ui/form";

const formSchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters"),
  email: z.string().email("Invalid email address"),
  age: z.number().min(18, "Must be at least 18"),
});

type FormValues = z.infer<typeof formSchema>;

const form = useForm<FormValues>({
  resolver: zodResolver(formSchema),
  defaultValues: { name: "", email: "", age: 0 },
});

const onSubmit = (values: FormValues) => {
  console.log(values);
};

<Form {...form}>
  <form onSubmit={form.handleSubmit(onSubmit)}>
    <FormField
      control={form.control}
      name="name"
      render={({ field }) => (
        <FormItem>
          <FormLabel>Name</FormLabel>
          <FormControl>
            <Input {...field} />
          </FormControl>
          <FormDescription>Your display name.</FormDescription>
          <FormMessage />
        </FormItem>
      )}
    />
    <FormField
      control={form.control}
      name="email"
      render={({ field }) => (
        <FormItem>
          <FormLabel>Email</FormLabel>
          <FormControl>
            <Input {...field} />
          </FormControl>
          <FormMessage />
        </FormItem>
      )}
    />
    <Button type="submit">Submit</Button>
  </form>
</Form>
```

## Ivy

In Ivy, form fields have built-in validation. No separate form wrapper or schema library is needed.

```csharp
var name = UseState("");
var email = UseState("");
var age = UseState(0);

new Column(
    name.ToTextInput()
        .Required()
        .MinLength(2, "Name must be at least 2 characters")
        .Description("Your display name."),
    email.ToTextInput()
        .Required()
        .Email("Invalid email address"),
    age.ToNumberInput()
        .Min(18, "Must be at least 18"),
    new Button("Submit", onClick: e => HandleSubmit())
).Gap(4);

void HandleSubmit()
{
    // Ivy validates automatically before submission
    Console.WriteLine($"Name: {name.Value}, Email: {email.Value}");
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `z.string()` | String validation | `TextInput` |
| `z.number()` | Number validation | `NumberInput` |
| `z.string().min(n)` | Minimum length | `.MinLength(int, string?)` |
| `z.string().max(n)` | Maximum length | `.MaxLength(int, string?)` |
| `z.string().email()` | Email validation | `.Email(string?)` |
| `z.number().min(n)` | Minimum value | `.Min(number, string?)` |
| `z.number().max(n)` | Maximum value | `.Max(number, string?)` |
| `z.string().optional()` | Optional field | Field is optional by default |
| `.min(1)` or `.nonempty()` | Required field | `.Required()` |
| `FormLabel` | Field label | First parameter of input widget |
| `FormDescription` | Help text below field | `.Description(string)` |
| `FormMessage` | Validation error message | Built-in (automatic) |
| `defaultValues` | Initial form values | `UseState` initial values |
| `handleSubmit` | Form submission | Button `OnClick` handler |
