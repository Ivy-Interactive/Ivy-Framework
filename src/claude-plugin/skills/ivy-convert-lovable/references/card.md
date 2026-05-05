# Card

A container with a border and padding for grouping related content. In Lovable apps, this is the shadcn/ui Card component.

## Lovable

```tsx
import {
  Card, CardContent, CardDescription,
  CardFooter, CardHeader, CardTitle,
} from "@/components/ui/card";

<Card>
  <CardHeader>
    <CardTitle>Revenue</CardTitle>
    <CardDescription>Monthly revenue overview</CardDescription>
  </CardHeader>
  <CardContent>
    <p className="text-2xl font-bold">$12,345</p>
  </CardContent>
  <CardFooter>
    <p className="text-sm text-muted-foreground">+12% from last month</p>
  </CardFooter>
</Card>
```

## Ivy

```csharp
new Card("Revenue", "Monthly revenue overview",
    new Text("$12,345").Bold(),
    new Text("+12% from last month").Muted()
);

// KPI card pattern (common in dashboards)
new KpiCard("Revenue", "$12,345", "+12%", KpiTrend.Up);

// Simple card with content
new Card(
    new Text("Some content here")
);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `CardTitle` | Header title text | `Title` parameter |
| `CardDescription` | Header description text | `Description` parameter |
| `CardContent` | Main body content | Content children |
| `CardFooter` | Footer content | Rendered as last children |
| `className` | Tailwind CSS classes | Not supported (use Ivy styling) |
