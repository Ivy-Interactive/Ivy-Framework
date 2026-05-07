# React State & Hooks

React state management and hooks patterns. Lovable apps use React hooks for state, effects, and custom data fetching.

## Lovable

```tsx
import { useState, useEffect, useMemo, useCallback } from "react";

// useState - local component state
const [count, setCount] = useState(0);
const [items, setItems] = useState<Item[]>([]);
const [isOpen, setIsOpen] = useState(false);

// useEffect - side effects
useEffect(() => {
  fetchData();
}, []);

useEffect(() => {
  if (id) loadItem(id);
}, [id]);

// useMemo - computed values
const total = useMemo(() => items.reduce((sum, i) => sum + i.amount, 0), [items]);

// useCallback - memoized callbacks
const handleClick = useCallback(() => {
  setCount(c => c + 1);
}, []);

// Custom hook for data fetching (common pattern)
function useCustomers() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["customers"],
    queryFn: async () => {
      const { data, error } = await supabase.from("customers").select("*");
      if (error) throw error;
      return data;
    },
  });
  return { customers: data ?? [], isLoading, error };
}
```

## Ivy

```csharp
// UseState - local component state
var count = UseState(0);
var items = UseState(ImmutableList<Item>.Empty);
var isOpen = UseState(false);

// Reading state
var currentCount = count.Value;

// Setting state
count.Set(5);
count.Set(c => c + 1);
items.Set(items.Value.Add(newItem));
isOpen.Set(true);

// UseQuery - data fetching (replaces useEffect + useState + react-query)
var customers = UseQuery(() => connection.Query<Customer>().ToListAsync());

// Computed values - just use C# expressions
var total = items.Value.Sum(i => i.Amount);

// No need for useCallback/useMemo - Ivy handles rendering optimization
```

## Parameters

| Pattern | Lovable | Ivy |
|---------|---------|-----|
| Local state | `useState(initialValue)` | `UseState(initialValue)` |
| Read state | `value` | `state.Value` |
| Set state | `setValue(newValue)` | `state.Set(newValue)` |
| Functional update | `setValue(prev => prev + 1)` | `state.Set(prev => prev + 1)` |
| Boolean toggle | `setOpen(!open)` | `isOpen.Set(!isOpen.Value)` |
| Array state | `setItems([...items, newItem])` | `items.Set(items.Value.Add(newItem))` |
| Data fetching | `useQuery({ queryKey, queryFn })` | `UseQuery(() => asyncFn())` |
| Side effect on mount | `useEffect(() => {}, [])` | Logic in App constructor or `UseQuery` |
| Side effect on change | `useEffect(() => {}, [dep])` | Reactive via state bindings |
| Computed value | `useMemo(() => expr, [deps])` | Direct C# expression |
| Memoized callback | `useCallback(() => {}, [deps])` | Not needed |
| Loading state | `isLoading` from `useQuery` | `UseQuery` returns loading state |
| Error state | `error` from `useQuery` | `UseQuery` handles errors |
