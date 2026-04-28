# DropdownMenu

A contextual menu triggered by a button click. In Lovable apps, this is the shadcn/ui DropdownMenu component.

## Lovable

```tsx
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem,
  DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal } from "lucide-react";

<DropdownMenu>
  <DropdownMenuTrigger asChild>
    <Button variant="ghost" size="icon">
      <MoreHorizontal className="h-4 w-4" />
    </Button>
  </DropdownMenuTrigger>
  <DropdownMenuContent align="end">
    <DropdownMenuLabel>Actions</DropdownMenuLabel>
    <DropdownMenuSeparator />
    <DropdownMenuItem onClick={() => handleEdit(row)}>Edit</DropdownMenuItem>
    <DropdownMenuItem onClick={() => handleDelete(row)} className="text-destructive">
      Delete
    </DropdownMenuItem>
  </DropdownMenuContent>
</DropdownMenu>
```

## Ivy

In Ivy, dropdown menus are typically handled as row actions on DataTable or as standalone menu buttons.

```csharp
// As DataTable row actions (most common use case)
new DataTable<Item>(items)
    .RowAction("Edit", (e, row) => HandleEdit(row), Icons.Edit)
    .RowAction("Delete", (e, row) => HandleDelete(row), Icons.Trash);

// As a standalone menu button
new MenuButton(Icons.MoreHorizontal,
    new MenuItem("Edit", e => HandleEdit(), Icons.Edit),
    new MenuItem("Delete", e => HandleDelete(), Icons.Trash).Destructive()
);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `DropdownMenuTrigger` | Element that opens the menu | `MenuButton` or DataTable row action trigger |
| `DropdownMenuItem` | Menu item with click handler | `MenuItem` or `.RowAction()` |
| `DropdownMenuLabel` | Section label | Not supported (group by separator) |
| `DropdownMenuSeparator` | Visual separator | Automatic between groups |
| `align` | Menu alignment. Default: `"center"` | Automatic |
| `onClick` | Item click handler | Event handler parameter |
