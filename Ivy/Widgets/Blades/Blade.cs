using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Blades provide a master-detail navigation pattern where new views slide in from the right, creating an intuitive drill-down experience for hierarchical content.</summary>
public record Blade : WidgetBase<Blade>
{
    /// <param name="index">The zero-based index position of this blade in the blade stack.</param>
    /// <param name="title">If null, no title is shown.</param>
    /// <param name="width">If null, defaults to fit-content with a minimum of 120 units and maximum of 300 units.</param>
    [OverloadResolutionPriority(1)]
    public Blade(IView bladeView, int index, string? title, Size? width, Func<Event<Blade>, ValueTask>? onClose, Func<Event<Blade>, ValueTask>? onRefresh) : base([bladeView])
    {
        Index = index;
        Title = title;
        OnClose = onClose;
        OnRefresh = onRefresh;
        Width = width ?? Size.Fit().Min(Size.Units(90)).Max(Size.Units(300));
    }

    /// <summary>The index determines the blade's position and is used for navigation and ordering.</summary>
    [Prop] public int Index { get; set; }

    /// <summary>When null, no title is shown in the blade header.</summary>
    [Prop] public string? Title { get; set; }

    [Event] public Func<Event<Blade>, ValueTask>? OnClose { get; set; }

    [Event] public Func<Event<Blade>, ValueTask>? OnRefresh { get; set; }

    /// <summary>Compatibility constructor for Action-based event handlers.</summary>
    public Blade(IView bladeView, int index, string? title, Size? width, Action<Event<Blade>>? onClose, Action<Event<Blade>>? onRefresh)
        : this(bladeView, index, title, width,
               onClose != null ? e => { onClose(e); return ValueTask.CompletedTask; }
    : null,
               onRefresh != null ? e => { onRefresh(e); return ValueTask.CompletedTask; }
    : null)
    {
    }
}
