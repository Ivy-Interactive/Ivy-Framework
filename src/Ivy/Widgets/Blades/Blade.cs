using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A slide-out panel component for secondary content or actions.
/// </summary>
public record Blade : WidgetBase<Blade>
{
    [OverloadResolutionPriority(1)]
    public Blade(IView bladeView, int index, string? title, Size? width, Func<Event<Blade>, ValueTask>? onClose, Func<Event<Blade>, ValueTask>? onRefresh) : base([bladeView])
    {
        Index = index;
        Title = title;
        OnClose = onClose.ToEventHandler();
        OnRefresh = onRefresh.ToEventHandler();
        Width = width ?? Size.Fit().Min(Size.Units(90)).Max(Size.Units(300));
    }

    internal Blade()
    {
    }

    [Prop] public int Index { get; set; }

    [Prop] public string? Title { get; set; }

    [Event] public EventHandler<Event<Blade>>? OnClose { get; set; }

    [Event] public EventHandler<Event<Blade>>? OnRefresh { get; set; }

    public Blade(IView bladeView, int index, string? title, Size? width, Action<Event<Blade>>? onClose, Action<Event<Blade>>? onRefresh)
    : this(bladeView, index, title, width,
           onClose?.ToValueTask(),
           onRefresh?.ToValueTask())
    {
    }
}
