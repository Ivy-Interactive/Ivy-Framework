# Toast (sonner)

Toast notifications for feedback messages. Lovable apps use the `sonner` library (often via shadcn/ui's toast setup).

## Lovable

```tsx
import { toast } from "sonner";
// or
import { useToast } from "@/hooks/use-toast";

// sonner (most common in Lovable)
toast("Event has been created");
toast.success("Successfully saved!");
toast.error("Something went wrong");
toast.info("New update available");
toast.warning("Please review before submitting");

// With description
toast.success("Profile updated", {
  description: "Your changes have been saved.",
});

// shadcn useToast hook (less common)
const { toast } = useToast();
toast({
  title: "Success",
  description: "Your profile has been updated.",
  variant: "default", // or "destructive"
});
```

## Ivy

```csharp
client.Toast("Event has been created");
client.Toast("Successfully saved!", "Success");
client.Toast("Something went wrong", variant: ToastVariant.Error);
client.Toast("New update available", variant: ToastVariant.Info);
client.Toast("Please review before submitting", variant: ToastVariant.Warning);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `message` | Toast message text | First parameter of `client.Toast()` |
| `description` | Additional description text | Second parameter (title) |
| `toast.success()` | Success variant | `ToastVariant.Success` |
| `toast.error()` | Error variant | `ToastVariant.Error` |
| `toast.info()` | Info variant | `ToastVariant.Info` |
| `toast.warning()` | Warning variant | `ToastVariant.Warning` |
| `duration` | Auto-dismiss duration in ms | Not supported |
| `action` | Action button on toast | Not supported |
