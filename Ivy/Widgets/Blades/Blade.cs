using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Blade : WidgetBase<Blade>
{
    [OverloadResolutionPriority(1)]
    public Blade(IView bladeView, int index, string? title, Size? width, Func<Event<Blade>, ValueTask>? onClose, Func<Event<Blade>, ValueTask>? onRefresh) : base([bladeView])
    {
        Index = index;
        Title = title;
        OnClose = onClose;
        OnRefresh = onRefresh;
        Width = width ?? Size.Fit().Min(Size.Units(90)).Max(Size.Units(300));
    }

    [Prop] public int Index { get; set; }

    [Prop] public string? Title { get; set; }

    [Event] public Func<Event<Blade>, ValueTask>? OnClose { get; set; }

    [Event] public Func<Event<Blade>, ValueTask>? OnRefresh { get; set; }

    public Blade(IView bladeView, int index, string? title, Size? width, Action<Event<Blade>>? onClose, Action<Event<Blade>>? onRefresh)
    : this(bladeView, index, title, width,
           onClose != null ? e => { onClose(e); return ValueTask.CompletedTask; }
    : null,
           onRefresh != null ? e => { onRefresh(e); return ValueTask.CompletedTask; }
    : null)
    {
    }
}
