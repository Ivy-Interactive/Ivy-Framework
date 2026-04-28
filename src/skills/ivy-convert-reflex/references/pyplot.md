# Pyplot

A graphing component that wraps Matplotlib, allowing any Matplotlib plot (contour, scatter, bar, etc.) to be rendered directly inside a web application. Requires installing the `reflex-pyplot` package. The figure must be closed after creation.

## Reflex

```python
import matplotlib.pyplot as plt
import reflex as rx
from reflex_pyplot import pyplot
import numpy as np

def create_contour_plot():
    X, Y = np.meshgrid(np.linspace(-3, 3, 256), np.linspace(-3, 3, 256))
    Z = (1 - X / 2 + X**5 + Y**3) * np.exp(-(X**2) - Y**2)
    levels = np.linspace(Z.min(), Z.max(), 7)

    fig, ax = plt.subplots()
    ax.contourf(X, Y, Z, levels=levels)
    plt.close(fig)
    return fig

def pyplot_simple_example():
    return rx.card(
        pyplot(create_contour_plot(), width="100%", height="400px"),
        width="100%",
    )
```

## Ivy

Ivy does not have a direct Matplotlib/pyplot wrapper. For standard chart types, Ivy provides built-in chart widgets via the Chart Builders API. For arbitrary Matplotlib output, a workaround is to save the figure as an image and display it with the `Image` widget.

**Built-in chart (closest equivalent for simple plots):**

```csharp
public class LineChartExample : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = "Jan", Y = 186 },
            new { X = "Feb", Y = 305 },
            new { X = "Mar", Y = 237 },
            new { X = "Apr", Y = 289 },
        };

        return data.ToLineChart()
            .Dimension("X", e => e.X)
            .Measure("Y", e => e.Sum(f => f.Y))
            .CartesianGrid()
            .Tooltip()
            .Toolbox();
    }
}
```

**Displaying a pre-rendered image (workaround for arbitrary plots):**

```csharp
// Save matplotlib figure to a file externally, then display it:
new Image("/ivy/images/contour_plot.png")
    .Width(Size.Percent(100))
    .Height(400);
```

## Parameters

| Parameter               | Documentation                                                                 | Ivy                                                                                          |
|-------------------------|-------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| Matplotlib figure       | Pass any `matplotlib.figure.Figure` as a child to render it in the browser    | Not supported (no Matplotlib wrapper; use built-in chart widgets or `Image` as a workaround) |
| `width`                 | CSS width of the rendered chart container                                     | `.Width(Size)` on chart or `Image` widget                                                    |
| `height`                | CSS height of the rendered chart container                                    | `.Height(Size)` on chart or `Image` widget                                                   |
| Contour / 3D plots      | Supported via Matplotlib (contourf, plot_surface, etc.)                      | Not supported                                                                                |
| Scatter plots           | Supported via Matplotlib (`ax.scatter(...)`)                                 | Not supported (no scatter chart type)                                                        |
| Line charts             | Supported via Matplotlib (`ax.plot(...)`)                                    | `data.ToLineChart()` with `.Dimension()` / `.Measure()`                                     |
| Bar charts              | Supported via Matplotlib (`ax.bar(...)`)                                     | `data.ToBarChart()` with `.Dimension()` / `.Measure()`                                      |
| Area charts             | Supported via Matplotlib (`ax.fill_between(...)`)                            | `data.ToAreaChart()` with `.Dimension()` / `.Measure()`                                     |
| Pie charts              | Supported via Matplotlib (`ax.pie(...)`)                                     | `data.ToPieChart()` with `.Dimension()` / `.Measure()`                                      |
| Dark/light theme        | Manual color management required (`color_mode_cond` + custom figure colors)  | `.ColorScheme(ColorScheme.Default)` built-in; theme handled automatically                    |
| Grid lines              | `ax.grid(True, ...)`                                                         | `.CartesianGrid()` with `.Horizontal()` / `.Vertical()`                                     |
| Legend                  | `ax.legend(...)`                                                              | `.Legend()` with `.Layout()` / `.VerticalAlign()`                                            |
| Tooltip                 | Not built-in (Matplotlib is static)                                          | `.Tooltip()` on chart widgets                                                                |
| Toolbox                 | Not built-in                                                                  | `.Toolbox()` adds interactive controls (zoom, export, etc.)                                  |
| Axis labels             | `ax.set_xlabel(...)` / `ax.set_ylabel(...)`                                  | `.XAxis().Label<XAxis>("text")` / `.YAxis().Label<YAxis>("text")`                            |
| Sorting                 | Manual via data preparation                                                   | `.SortBy(SortOrder)` or `.SortBy(keySelector, SortOrder)`                                   |
| Color scheme            | Manual per-element color assignment                                           | `.ColorScheme(ColorScheme.Rainbow)` and other presets                                        |
| Event triggers          | All default Reflex event triggers                                             | Not documented for charts                                                                    |
