# Alert / Callout

An inline alert message for important information. In Lovable apps, this is the shadcn/ui Alert component.

## Lovable

```tsx
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { AlertCircle, CheckCircle, Info } from "lucide-react";

<Alert>
  <Info className="h-4 w-4" />
  <AlertTitle>Information</AlertTitle>
  <AlertDescription>This is an informational message.</AlertDescription>
</Alert>

<Alert variant="destructive">
  <AlertCircle className="h-4 w-4" />
  <AlertTitle>Error</AlertTitle>
  <AlertDescription>Something went wrong.</AlertDescription>
</Alert>
```

## Ivy

```csharp
Callout.Info("This is an informational message.", "Information");
Callout.Error("Something went wrong.", "Error");
Callout.Success("Operation completed successfully.", "Success");
Callout.Warning("Please review before continuing.", "Warning");
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `AlertTitle` | Alert heading text | `title` parameter |
| `AlertDescription` | Alert body text | `description` parameter |
| `variant` | `"default" \| "destructive"` | `CalloutVariant` (Info, Success, Warning, Error) |
| Icon | Leading icon component | Automatic based on variant, or `Icon` property |
