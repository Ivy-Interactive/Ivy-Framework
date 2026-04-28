# Table / DataTable

A data table for displaying tabular data. In Lovable apps, this can be the shadcn/ui Table component or a @tanstack/react-table implementation.

## Lovable

```tsx
// Simple shadcn Table
import {
  Table, TableBody, TableCell, TableHead,
  TableHeader, TableRow,
} from "@/components/ui/table";

<Table>
  <TableHeader>
    <TableRow>
      <TableHead>Name</TableHead>
      <TableHead>Email</TableHead>
      <TableHead>Status</TableHead>
    </TableRow>
  </TableHeader>
  <TableBody>
    {data?.map((row) => (
      <TableRow key={row.id}>
        <TableCell>{row.name}</TableCell>
        <TableCell>{row.email}</TableCell>
        <TableCell>{row.status}</TableCell>
      </TableRow>
    ))}
  </TableBody>
</Table>

// With @tanstack/react-table
import { useReactTable, getCoreRowModel, flexRender } from "@tanstack/react-table";

const columns = [
  { accessorKey: "name", header: "Name" },
  { accessorKey: "email", header: "Email" },
  { accessorKey: "status", header: "Status" },
];
const table = useReactTable({ data, columns, getCoreRowModel: getCoreRowModel() });
```

## Ivy

```csharp
new DataTable<Customer>(customers)
    .Column("Name", c => c.Name)
    .Column("Email", c => c.Email)
    .Column("Status", c => c.Status);

// With row actions
new DataTable<Customer>(customers)
    .Column("Name", c => c.Name)
    .Column("Email", c => c.Email)
    .RowAction("Edit", (e, row) => EditCustomer(row), Icons.Edit)
    .RowAction("Delete", (e, row) => DeleteCustomer(row), Icons.Trash);

// With header actions (e.g. Add button)
new DataTable<Customer>(customers)
    .Column("Name", c => c.Name)
    .HeaderAction("Add Customer", e => openSheet(), Icons.Plus);

// With search and pagination
new DataTable<Customer>(customers)
    .Column("Name", c => c.Name)
    .Searchable()
    .PageSize(10);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `data` | Array of row data | Constructor parameter (collection) |
| `columns` | Column definitions | `.Column()` method chain |
| Column `accessorKey` | Data field to display | Lambda in `.Column()` |
| Column `header` | Column header text | First parameter of `.Column()` |
| Row click handler | `onClick` on `TableRow` | `.OnRowClick()` |
| Sorting | `getSortedRowModel()` | Built-in (automatic) |
| Filtering | `getFilteredRowModel()` | `.Searchable()` |
| Pagination | `getPaginationRowModel()` | `.PageSize(int)` |
