using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Widget that creates a celebratory confetti effect around its child content.
/// </summary>
public record Confetti : WidgetBase<Confetti>
{
    /// <summary>
    /// Initializes a new instance of the Confetti class.
    /// </summary>
    /// <param name="child">
    /// The child widget or content.
    /// </param>
    public Confetti(object? child = null) : base(child != null ? [child] : [])
    {
    }

    /// <summary>
    /// Gets or sets when the confetti effect should be triggered.
    /// </summary>
    [Prop] public AnimationTrigger Trigger { get; init; } = AnimationTrigger.Auto;

    /// <summary>
    /// Overrides the | operator to add a single child.
    /// </summary>
    /// <param name="widget">The Confetti widget.</param>
    /// <param name="child">The single child widget or content to add.</param>
    /// <returns>A new Confetti widget with the specified child.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when attempting to add multiple children.
    /// </exception>
    public static Confetti operator |(Confetti widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Confetti does not support multiple children.");
        }

        return widget with { Children = [child] };
    }
}

/// <summary>
/// Provides extension methods for creating and configuring Confetti widgets.
/// </summary>
public static class ConfettiExtensions
{
    /// <summary>
    /// Wraps a widget with a confetti effect.
    /// </summary>
    /// <param name="widget">The widget to wrap with confetti effect.</param>
    /// <param name="trigger">The trigger condition for the confetti effect.</param>
    /// <returns>A Confetti widget.</returns>
    public static Confetti WithConfetti(this IWidget widget, AnimationTrigger trigger = AnimationTrigger.Auto)
    {
        return new Confetti(widget).Trigger(trigger);
    }

    /// <summary>
    /// Wraps a view with a confetti effect.
    /// </summary>
    /// <param name="view">The view to wrap with confetti effect.</param>
    /// <param name="trigger">The trigger condition for the confetti effect.</param>
    /// <returns>A Confetti widget.</returns>
    public static Confetti WithConfetti(this IView view, AnimationTrigger trigger = AnimationTrigger.Auto)
    {
        return new Confetti(view).Trigger(trigger);
    }

    /// <summary>
    /// Sets the trigger condition for when the confetti effect should activate.
    /// </summary>
    /// <param name="confetti">The Confetti widget.</param>
    /// <param name="trigger">The trigger condition.</param>
    /// <returns>The confetti widget with the specified trigger condition.</returns>
    public static Confetti Trigger(this Confetti confetti, AnimationTrigger trigger)
    {
        return confetti with { Trigger = trigger };
    }
}


