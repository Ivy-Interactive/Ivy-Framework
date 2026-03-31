using System;
using System.Linq;
using Ivy;

namespace Ivy.Benchmarks;

public class MassiveFormsApp : ViewBase
{
    private readonly int _formCount;
    private readonly int _triggerValue;

    public MassiveFormsApp(int formCount = 1000, int triggerValue = 0)
    {
        _formCount = formCount;
        _triggerValue = triggerValue;
    }

    public Action MutateState { get; private set; } = null!;

    public override object? Build()
    {
        var runTrigger = UseState(_triggerValue);
        MutateState = () => runTrigger.Set(x => x + 1);

        return Layout.Vertical(
            Text.H1("Gigantic Ivy Benchmark Engine"),
            Layout.Vertical(
                Enumerable.Range(0, _formCount).Select(formIndex => 
                    new FormComponent(formIndex, _triggerValue)
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
