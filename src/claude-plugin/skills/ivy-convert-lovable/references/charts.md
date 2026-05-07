# Charts (Recharts)

Charts for data visualization. Lovable apps use Recharts (often wrapped via shadcn/ui chart components).

## Lovable

```tsx
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";
import { LineChart, Line } from "recharts";
import { PieChart, Pie, Cell } from "recharts";

const data = [
  { name: "Jan", revenue: 4000 },
  { name: "Feb", revenue: 3000 },
  { name: "Mar", revenue: 5000 },
];

// Bar Chart
<ResponsiveContainer width="100%" height={300}>
  <BarChart data={data}>
    <CartesianGrid strokeDasharray="3 3" />
    <XAxis dataKey="name" />
    <YAxis />
    <Tooltip />
    <Bar dataKey="revenue" fill="#8884d8" />
  </BarChart>
</ResponsiveContainer>

// Line Chart
<ResponsiveContainer width="100%" height={300}>
  <LineChart data={data}>
    <XAxis dataKey="name" />
    <YAxis />
    <Tooltip />
    <Line type="monotone" dataKey="revenue" stroke="#8884d8" />
  </LineChart>
</ResponsiveContainer>

// Pie Chart
<PieChart width={300} height={300}>
  <Pie data={data} dataKey="revenue" nameKey="name" cx="50%" cy="50%" outerRadius={80} fill="#8884d8" />
</PieChart>
```

## Ivy

```csharp
// Bar Chart
new BarChart<SalesData>(data)
    .XAxis(d => d.Name)
    .Series("Revenue", d => d.Revenue);

// Line Chart
new LineChart<SalesData>(data)
    .XAxis(d => d.Name)
    .Series("Revenue", d => d.Revenue);

// Pie Chart
new PieChart<SalesData>(data)
    .Value(d => d.Revenue)
    .Label(d => d.Name);

// Area Chart
new AreaChart<SalesData>(data)
    .XAxis(d => d.Name)
    .Series("Revenue", d => d.Revenue);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `data` | Array of data points | Constructor parameter (collection) |
| `XAxis dataKey` | X-axis field | `.XAxis()` lambda |
| `Bar/Line dataKey` | Y-axis field | `.Series()` lambda |
| `Pie dataKey` | Value field | `.Value()` lambda |
| `Pie nameKey` | Label field | `.Label()` lambda |
| `fill` / `stroke` | Color | Automatic (theme-based) |
| `ResponsiveContainer` | Responsive wrapper | Built-in (automatic) |
| `CartesianGrid` | Grid lines | Built-in (automatic) |
| `Tooltip` | Hover tooltip | Built-in (automatic) |
| `Legend` | Chart legend | Built-in (automatic) |
