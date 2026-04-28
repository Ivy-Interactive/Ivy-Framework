# Layout (Tailwind flex/grid)

Layout patterns using Tailwind CSS utility classes. Lovable apps use Tailwind for layout via `flex`, `grid`, `space-*`, and `gap-*` classes.

## Lovable

```tsx
// Horizontal layout (row)
<div className="flex gap-4 items-center">
  <Button>Action 1</Button>
  <Button>Action 2</Button>
</div>

// Vertical layout (column)
<div className="flex flex-col gap-4">
  <Input placeholder="Name" />
  <Input placeholder="Email" />
</div>

// Grid layout
<div className="grid grid-cols-3 gap-4">
  <Card>Item 1</Card>
  <Card>Item 2</Card>
  <Card>Item 3</Card>
</div>

// Responsive grid
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  {items.map(item => <Card key={item.id}>{item.name}</Card>)}
</div>

// Spacing and alignment
<div className="flex justify-between items-center p-4">
  <h1>Title</h1>
  <Button>Action</Button>
</div>

// Container with max-width
<div className="container mx-auto px-4">
  <div className="max-w-2xl mx-auto">Content</div>
</div>
```

## Ivy

```csharp
// Horizontal layout (row)
new Row(
    new Button("Action 1"),
    new Button("Action 2")
).Gap(4).AlignCenter();

// Vertical layout (column)
var name = UseState("");
var email = UseState("");
new Column(
    name.ToTextInput().Placeholder("Name"),
    email.ToTextInput().Placeholder("Email")
).Gap(4);

// Grid layout
new Grid(3,
    new Card("Item 1"),
    new Card("Item 2"),
    new Card("Item 3")
).Gap(4);

// Spacing between title and action
new Row(
    new Heading("Title"),
    new Spacer(),
    new Button("Action")
).AlignCenter();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `flex` | Flexbox container | `Row` (horizontal) or `Column` (vertical) |
| `flex-col` | Vertical flex direction | `Column` |
| `grid` | CSS Grid container | `Grid` |
| `grid-cols-N` | Grid column count | `Grid(columns)` constructor parameter |
| `gap-N` | Gap between items | `.Gap(int)` |
| `items-center` | Vertical center alignment | `.AlignCenter()` |
| `justify-between` | Space between items | Use `Spacer` between items |
| `justify-center` | Center items horizontally | `.JustifyCenter()` |
| `p-N` | Padding | `.Padding(int)` |
| `space-y-N` | Vertical spacing between children | `.Gap(int)` on `Column` |
| `space-x-N` | Horizontal spacing between children | `.Gap(int)` on `Row` |
| `container` | Max-width container | Not needed (Ivy handles page layout) |
| `w-full` | Full width | Not needed (default behavior) |
