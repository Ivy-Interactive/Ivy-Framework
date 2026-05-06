# Icons (lucide-react)

Icon components. Lovable apps use `lucide-react` for icons throughout the UI.

## Lovable

```tsx
import { Plus, Trash, Edit, Settings, Search, User, Home, ChevronRight } from "lucide-react";

<Button><Plus className="h-4 w-4 mr-2" /> Add Item</Button>
<Button size="icon"><Trash className="h-4 w-4" /></Button>
<Search className="h-4 w-4 text-muted-foreground" />
```

## Ivy

Ivy uses the `Icons` enum which maps to the same Material/Lucide icon set.

```csharp
new Button("Add Item").Icon(Icons.Plus);
new Button().Icon(Icons.Trash).Ghost();  // icon-only
new Icon(Icons.Search);
```

## Common Icon Mappings

| lucide-react | Ivy |
|---|---|
| `Plus` | `Icons.Plus` |
| `Trash` / `Trash2` | `Icons.Trash` |
| `Edit` / `Pencil` | `Icons.Edit` |
| `Settings` | `Icons.Settings` |
| `Search` | `Icons.Search` |
| `User` | `Icons.User` |
| `Home` | `Icons.Home` |
| `ChevronRight` | `Icons.ChevronRight` |
| `ChevronDown` | `Icons.ChevronDown` |
| `X` / `Close` | `Icons.Close` |
| `Check` | `Icons.Check` |
| `AlertCircle` | `Icons.AlertCircle` |
| `Info` | `Icons.Info` |
| `Download` | `Icons.Download` |
| `Upload` | `Icons.Upload` |
| `Mail` | `Icons.Mail` |
| `Phone` | `Icons.Phone` |
| `Calendar` | `Icons.Calendar` |
| `Clock` | `Icons.Clock` |
| `Star` | `Icons.Star` |
| `Heart` | `Icons.Heart` |
| `Eye` | `Icons.Eye` |
| `EyeOff` | `Icons.EyeOff` |
| `Copy` | `Icons.Copy` |
| `ExternalLink` | `Icons.ExternalLink` |
| `MoreHorizontal` | `Icons.MoreHorizontal` |
| `MoreVertical` | `Icons.MoreVertical` |
| `ArrowLeft` | `Icons.ArrowLeft` |
| `ArrowRight` | `Icons.ArrowRight` |
| `Filter` | `Icons.Filter` |
| `RefreshCw` | `Icons.Refresh` |
| `LogOut` | `Icons.LogOut` |
| `BarChart` / `BarChart3` | `Icons.BarChart` |
| `LineChart` | `Icons.LineChart` |
| `PieChart` | `Icons.PieChart` |
| `DollarSign` | `Icons.DollarSign` |
| `FileText` | `Icons.FileText` |
| `Folder` | `Icons.Folder` |
