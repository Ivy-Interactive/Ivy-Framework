using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A visual indicator of an operation's progress.
/// </summary>
public record Progress : WidgetBase<Progress>
{
    public Progress(IState<int> state) : this(state.Value)
    {
    }

    public Progress(int? value = 0) : this()
    {
        Value = value;
    }

    internal Progress()
    {
        Width = Size.Full();
    }

    [Prop] public int? Value { get; set; }

    [Prop] public string? Goal { get; set; }

    [Prop] public Colors? Color { get; set; }

    [Prop] public bool Indeterminate { get; set; }

    public static Progress operator |(Progress widget, object child)
    {
        throw new NotSupportedException("Progress does not support children.");
    }
}

public static class ProgressExtensions
{
    public static Progress Value(this Progress progress, IState<int> value)
    {
        return progress with { Value = value.Value };
    }

    public static Progress Goal(this Progress progress, string? goal)
    {
        return progress with { Goal = goal };
    }

    public static Progress Color(this Progress progress, Colors? color)
    {
        return progress with { Color = color };
    }

    public static Progress Indeterminate(this Progress progress, bool indeterminate = true)
    {
        return progress with { Indeterminate = indeterminate };
    }
}