using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a blade in a stacked navigation interface. Blades provide a master-detail navigation pattern
/// where new views slide in from the right, creating an intuitive drill-down experience for hierarchical content.
/// </summary>
public record Blade : WidgetBase<Blade>
{
    /// <summary>
    /// Initializes a new instance of the Blade class with the specified view and configuration.
    /// </summary>
    /// <param name="bladeView">The view to be displayed within this blade. Must implement <see cref="IView"/>.</param>
    /// <param name="index">The zero-based index position of this blade in the blade stack.</param>
    /// <param name="title">The optional title to display in the blade header. If null, no title is shown.</param>
    /// <param name="width">The width of the blade. If null, defaults to fit-content with a minimum of 120 units and maximum of 300 units.</param>
    /// <param name="onClose">Optional event handler called when the blade is closed by the user.</param>
    /// <param name="onRefresh">Optional event handler called when the blade content should be refreshed.</param>
    [OverloadResolutionPriority(1)]
    public Blade(IView bladeView, int index, string? title, Size? width, Func<Event<Blade>, ValueTask>? onClose, Func<Event<Blade>, ValueTask>? onRefresh) : base([bladeView])
    {
        Index = index;
        Title = title;
        OnClose = onClose;
        OnRefresh = onRefresh;
        Width = width ?? Size.Fit().Min(Size.Units(90)).Max(Size.Units(300));
    }

    /// <summary>
    /// Gets or sets the zero-based index position of this blade in the blade stack.
    /// The index determines the blade's position and is used for navigation and ordering.
    /// </summary>
    /// <value>The index position of the blade, starting from 0 for the root blade.</value>
    [Prop] public int Index { get; set; }

    /// <summary>
    /// Gets or sets the title displayed in the blade header.
    /// When null, no title is shown in the blade header.
    /// </summary>
    /// <value>The title text, or null if no title should be displayed.</value>
    [Prop] public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the event handler called when the user closes this blade.
    /// This event is typically triggered by clicking a close button or using keyboard shortcuts.
    /// </summary>
    /// <value>An action that receives a <see cref="Event{T}"/> with this blade as the source.</value>
    [Event] public Func<Event<Blade>, ValueTask>? OnClose { get; set; }

    /// <summary>
    /// Gets or sets the event handler called when the blade content should be refreshed.
    /// This event is typically triggered by user action or programmatic refresh requests.
    /// </summary>
    /// <value>An action that receives a <see cref="Event{T}"/> with this blade as the source.</value>
    [Event] public Func<Event<Blade>, ValueTask>? OnRefresh { get; set; }

    /// <summary>
    /// Compatibility constructor for Action-based event handlers.
    /// Automatically wraps Action delegates in ValueTask-returning functions for backward compatibility.
    /// </summary>
    /// <param name="bladeView">The view to be displayed within this blade.</param>
    /// <param name="index">The zero-based index position of this blade in the blade stack.</param>
    /// <param name="title">The optional title to display in the blade header.</param>
    /// <param name="width">The width of the blade.</param>
    /// <param name="onClose">Optional Action-based event handler called when the blade is closed.</param>
    /// <param name="onRefresh">Optional Action-based event handler called when the blade content should be refreshed.</param>
    public Blade(IView bladeView, int index, string? title, Size? width, Action<Event<Blade>>? onClose, Action<Event<Blade>>? onRefresh)
        : this(bladeView, index, title, width,
               onClose != null ? e => { onClose(e); return ValueTask.CompletedTask; }
    : null,
               onRefresh != null ? e => { onRefresh(e); return ValueTask.CompletedTask; }
    : null)
    {
    }
}
