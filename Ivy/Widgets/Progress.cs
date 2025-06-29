using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Progress : WidgetBase<Progress>
{
    public enum ColorVariants { Primary, EmeraldGradient }

    public Progress(IState<int> state) : this(state.Value)
    {
    }

    public Progress(int? value = 0)
    {
        Value = value;
        Width = Size.Full();
    }

    [Prop] public int? Value { get; set; }
    [Prop] public string? Goal { get; set; }
    [Prop] public ColorVariants ColorVariant { get; set; } = ColorVariants.Primary;

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

    public static Progress ColorVariant(this Progress progress, Progress.ColorVariants variant)
    {
        return progress with { ColorVariant = variant };
    }
}