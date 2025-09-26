using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Progress widget visually representing task completion status with customizable color variants and state binding for dynamic updates.</summary>
public record Progress : WidgetBase<Progress>
{
    /// <summary>Available color variants for progress bars controlling visual appearance and color scheme.</summary>
    public enum ColorVariants
    {
        /// <summary>Primary color variant with standard progress bar styling.</summary>
        Primary,
        /// <summary>Emerald gradient variant with enhanced visual appeal.</summary>
        EmeraldGradient
    }

    /// <summary>Initializes Progress with state-bound value for automatic updates.</summary>
    /// <param name="state">IState&lt;int&gt; object providing progress value for real-time tracking.</param>
    public Progress(IState<int> state) : this(state.Value)
    {
    }

    /// <summary>Initializes Progress with specified progress value (0-100 percentage).</summary>
    /// <param name="value">Progress value as percentage from 0 to 100, null for indeterminate state.</param>
    public Progress(int? value = 0)
    {
        Value = value;
        Width = Size.Full();
    }

    /// <summary>Current progress value as percentage (0-100). When null, displays indeterminate state. Default is 0.</summary>
    [Prop] public int? Value { get; set; }

    /// <summary>Goal or description text displayed alongside progress bar providing context. Default is null.</summary>
    [Prop] public string? Goal { get; set; }

    /// <summary>Color variant controlling visual styling and color scheme. Default is <see cref="ColorVariants.Primary"/>.</summary>
    [Prop] public ColorVariants ColorVariant { get; set; } = ColorVariants.Primary;

    /// <summary>Prevents adding children to Progress using pipe operator.</summary>
    /// <param name="widget">Progress widget.</param>
    /// <param name="child">Child content to add (not supported).</param>
    /// <returns>Always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Progress does not support children.</exception>
    public static Progress operator |(Progress widget, object child)
    {
        throw new NotSupportedException("Progress does not support children.");
    }
}

/// <summary>Extension methods for Progress widget providing fluent API for configuring appearance, behavior, and content.</summary>
public static class ProgressExtensions
{
    /// <summary>Sets progress value using state-bound integer for automatic updates.</summary>
    /// <param name="progress">Progress widget to configure.</param>
    /// <param name="value">IState&lt;int&gt; object providing progress value.</param>
    /// <returns>New Progress instance with updated value from state.</returns>
    public static Progress Value(this Progress progress, IState<int> value)
    {
        return progress with { Value = value.Value };
    }

    /// <summary>Sets goal or description text for progress bar providing context.</summary>
    /// <param name="progress">Progress widget to configure.</param>
    /// <param name="goal">Goal text to display alongside progress bar, or null to remove.</param>
    /// <returns>New Progress instance with updated goal text.</returns>
    public static Progress Goal(this Progress progress, string? goal)
    {
        return progress with { Goal = goal };
    }

    /// <summary>Sets color variant for progress bar changing visual styling and color scheme.</summary>
    /// <param name="progress">Progress widget to configure.</param>
    /// <param name="variant">Color variant to apply to progress bar.</param>
    /// <returns>New Progress instance with updated color variant.</returns>
    public static Progress ColorVariant(this Progress progress, Progress.ColorVariants variant)
    {
        return progress with { ColorVariant = variant };
    }
}