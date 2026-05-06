# Skeleton / Loading

A placeholder animation shown while content is loading. In Lovable apps, this is the shadcn/ui Skeleton component.

## Lovable

```tsx
import { Skeleton } from "@/components/ui/skeleton";

// Loading state
if (isLoading) {
  return (
    <div className="space-y-4">
      <Skeleton className="h-12 w-full" />
      <Skeleton className="h-8 w-3/4" />
      <Skeleton className="h-8 w-1/2" />
    </div>
  );
}

// Common pattern with useQuery
const { data, isLoading } = useQuery({ ... });
if (isLoading) return <Skeleton className="h-64 w-full" />;
```

## Ivy

In Ivy, loading states are handled automatically by `UseQuery`. No manual skeleton components are needed.

```csharp
// UseQuery handles loading, error, and data states automatically
var customers = UseQuery(() => connection.Query<Customer>().ToListAsync());

// Ivy shows a loading indicator while data is being fetched
// No need for manual Skeleton components
```

## Parameters

| Pattern | Lovable | Ivy |
|---------|---------|-----|
| Loading state | `if (isLoading) return <Skeleton />` | Automatic via `UseQuery` |
| Manual loading | `<Skeleton className="h-12 w-full" />` | Not needed |
