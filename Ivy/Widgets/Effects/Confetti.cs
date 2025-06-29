using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Confetti : WidgetBase<Confetti>
{
    public Confetti(object? child = null) : base(child != null ? [child] : [])
    {
    }

    [Prop] public AnimationTrigger Trigger { get; init; } = AnimationTrigger.Auto;

    public static Confetti operator |(Confetti widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Confetti does not support multiple children.");
        }

        return widget with { Children = [child] };
    }
}

public static class ConfettiExtensions
{
    public static Confetti WithConfetti(this IWidget widget, AnimationTrigger trigger = AnimationTrigger.Auto)
    {
        return new Confetti(widget).Trigger(trigger);
    }

    public static Confetti WithConfetti(this IView view, AnimationTrigger trigger = AnimationTrigger.Auto)
    {
        return new Confetti(view).Trigger(trigger);
    }

    public static Confetti Trigger(this Confetti confetti, AnimationTrigger trigger)
    {
        return confetti with { Trigger = trigger };
    }
}


