# st.graphviz_chart

Display a graph using the dagre-d3 library. Accepts a Graphviz graph object or a DOT language string to render directed and undirected graphs.

## Streamlit

```python
import streamlit as st
import graphviz

graph = graphviz.Digraph()
graph.edge("run", "intr")
graph.edge("intr", "runbl")
graph.edge("runbl", "run")

st.graphviz_chart(graph)
```

## Ivy

Ivy does not have a built-in Graphviz widget. The closest equivalent is the `Svg` widget, which can render SVG markup produced by a Graphviz library such as [GiGraph](https://github.com/mariusz-schimke/GiGraph).

```csharp
using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Extensions;

public class GraphvizView : ViewBase
{
    public override object? Build()
    {
        var graph = new DotGraph(directed: true);
        graph.Edges.Add("run", "intr");
        graph.Edges.Add("intr", "runbl");
        graph.Edges.Add("runbl", "run");

        var dot = graph.Build();
        var svg = ConvertDotToSvg(dot); // requires a DOT-to-SVG renderer

        return new Svg(svg);
    }
}
```

## Parameters

| Parameter            | Documentation                                                                                     | Ivy           |
|----------------------|---------------------------------------------------------------------------------------------------|---------------|
| figure_or_dot        | The Graphviz graph object (`graphviz.dot.Graph`, `graphviz.dot.Digraph`, `graphviz.sources.Source`) or DOT string to display | Not supported (render to SVG first, then use `new Svg(content)`) |
| use_container_width  | Deprecated. Use `width="stretch"` instead of `True`, or `width="content"` instead of `False`      | Not supported |
