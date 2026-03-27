using System.Linq;
using Ivy;

namespace MyBenchmark;

[App("massive-benchmark-engine", group: ["Benchmarks"])]
public class MassiveFormsApp : ViewBase
{
    public override object? Build()
    {
        var runTrigger = UseState(0);

        return Layout.Vertical(
            Layout.Horizontal(
                Text.H1("Gigantic Ivy Benchmark Engine"),
                new Button("Simulate Massive 10,000 Node Mutation", onClick: _ => runTrigger.Set(t => t + 1))
            ),
            Layout.Vertical(
                Enumerable.Range(0, 1000).Select(formIndex =>
                    new FormComponent(formIndex, runTrigger.Value)
                ).ToArray()
            )
        );
    }
}

public class FormComponent(int index, int globalTrigger) : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            Text.H3($"Synthetic Form Prototype - Block {index}"),
            Layout.Vertical(
                Enumerable.Range(0, 10).Select(fieldIndex =>
                    Text.Muted($"Field Value {fieldIndex} [Mutated {globalTrigger} Times!]")
                ).ToArray()
            )
        );
    }
}
