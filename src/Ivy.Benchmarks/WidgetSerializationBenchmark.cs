using BenchmarkDotNet.Attributes;
using Ivy;
using Ivy.Core;
using System.Text.Json.Nodes;

namespace Ivy.Benchmarks;

[MemoryDiagnoser]
public class WidgetSerializationBenchmark
{
    [Params(100, 500)]
    public int Iterations { get; set; }

    private IWidget[] _widgets = null!;

    [GlobalSetup]
    public void Setup()
    {
        var widgets = new List<IWidget>();
        for (int i = 0; i < Iterations; i++)
        {
            var buttons = new List<object>();
            for (int j = 0; j < 10; j++)
            {
                buttons.Add(new Button($"Btn {i}-{j}", () => {})
                    .Width(Size.Units(12))
                    .Height(Size.Units(6))
                    .Density(Density.Small));
            }

            var canvas = new CanvasLayout(buttons.ToArray())
            {
                Background = Colors.Slate
            };

            var card = new Card(canvas)
            {
                HoverVariant = HoverEffect.Shadow,
                Disabled = false
            };

            // Recursively assign IDs to the widget and all its children
            AssignIds(card, $"card-{i}");

            widgets.Add(card);
        }

        _widgets = widgets.ToArray();
    }

    private void AssignIds(IWidget widget, string id)
    {
        widget.Id = id;
        if (widget.Children != null)
        {
            for (int i = 0; i < widget.Children.Length; i++)
            {
                if (widget.Children[i] is IWidget child)
                {
                    AssignIds(child, $"{id}_{i}");
                }
            }
        }
    }

    [Benchmark(Baseline = true)]
    public int SerializeWidgets()
    {
        int totalLength = 0;
        foreach (var widget in _widgets)
        {
            var node = WidgetSerializer.Serialize(widget);
            totalLength += node["id"]?.GetValue<string>().Length ?? 0;
        }
        return totalLength;
    }
}
