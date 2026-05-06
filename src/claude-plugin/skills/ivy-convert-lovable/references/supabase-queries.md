# Supabase Queries

Data fetching patterns using the Supabase client. Lovable apps use `@supabase/supabase-js` for database queries, often wrapped in `@tanstack/react-query` hooks.

## Lovable

```tsx
import { supabase } from "@/integrations/supabase/client";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

// Direct Supabase query
const { data, error } = await supabase
  .from("customers")
  .select("*")
  .order("created_at", { ascending: false });

// With filters
const { data } = await supabase
  .from("orders")
  .select("*, customers(name)")
  .eq("status", "active")
  .gte("amount", 100)
  .limit(50);

// Insert
const { data, error } = await supabase
  .from("customers")
  .insert({ name: "John", email: "john@example.com" })
  .select()
  .single();

// Update
const { error } = await supabase
  .from("customers")
  .update({ name: "Jane" })
  .eq("id", customerId);

// Delete
const { error } = await supabase
  .from("customers")
  .delete()
  .eq("id", customerId);

// React Query wrapper (common pattern)
const { data: customers, isLoading } = useQuery({
  queryKey: ["customers"],
  queryFn: async () => {
    const { data, error } = await supabase.from("customers").select("*");
    if (error) throw error;
    return data;
  },
});

// Mutation with cache invalidation
const queryClient = useQueryClient();
const createCustomer = useMutation({
  mutationFn: async (values) => {
    const { error } = await supabase.from("customers").insert(values);
    if (error) throw error;
  },
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["customers"] });
    toast.success("Customer created!");
  },
});

// Invoke edge function
const { data, error } = await supabase.functions.invoke("send-email", {
  body: { to: "user@example.com", subject: "Hello" },
});
```

## Ivy

In Ivy, database operations are handled through Connection classes with typed queries.

```csharp
// Query all records
var customers = await connection.Query<Customer>()
    .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();

// With filters
var orders = await connection.Query<Order>()
    .Where(o => o.Status == "active")
    .Where(o => o.Amount >= 100)
    .Take(50)
    .Include(o => o.Customer)
    .ToListAsync();

// Insert
var customer = new Customer { Name = "John", Email = "john@example.com" };
await connection.InsertAsync(customer);

// Update
customer.Name = "Jane";
await connection.UpdateAsync(customer);

// Delete
await connection.DeleteAsync(customer);

// In an App, data is typically loaded via UseQuery
var customers = UseQuery(() => connection.Query<Customer>().ToListAsync());

// Mutations trigger automatic refresh
async ValueTask CreateCustomer(Customer customer)
{
    await connection.InsertAsync(customer);
    client.Toast("Customer created!");
}
```

## Parameters

| Pattern | Lovable | Ivy |
|---------|---------|-----|
| Select all | `supabase.from("table").select("*")` | `connection.Query<T>().ToListAsync()` |
| Filter (equals) | `.eq("col", value)` | `.Where(x => x.Col == value)` |
| Filter (greater than) | `.gte("col", value)` | `.Where(x => x.Col >= value)` |
| Filter (like) | `.like("col", "%value%")` | `.Where(x => x.Col.Contains(value))` |
| Order | `.order("col", { ascending: false })` | `.OrderByDescending(x => x.Col)` |
| Limit | `.limit(n)` | `.Take(n)` |
| Join | `.select("*, related(col)")` | `.Include(x => x.Related)` |
| Insert | `.insert(obj)` | `connection.InsertAsync(obj)` |
| Update | `.update(obj).eq("id", id)` | `connection.UpdateAsync(obj)` |
| Delete | `.delete().eq("id", id)` | `connection.DeleteAsync(obj)` |
| Single record | `.select().single()` | `.FirstOrDefaultAsync()` |
| Count | `.select("*", { count: "exact" })` | `.CountAsync()` |
| React Query | `useQuery({ queryKey, queryFn })` | `UseQuery(() => ...)` |
| Mutation | `useMutation({ mutationFn })` | Direct async method call |
| Cache invalidation | `queryClient.invalidateQueries()` | Automatic |
| Edge function | `supabase.functions.invoke("name")` | Ivy server action or API call |
