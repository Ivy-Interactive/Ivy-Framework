using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Blade : WidgetBase<Blade>
{
    public Blade(IView bladeView, int index, string? title, Size? width, Action<Event<Blade>>? onClose, Action<Event<Blade>>? onRefresh) : base([bladeView])
    {
        Index = index;
        Title = title;
        OnClose = onClose;
        OnRefresh = onRefresh;
        Width = width ?? Size.Auto().Min(Size.Units(80));
    }

    [Prop] public int Index { get; set; }

    [Prop] public string? Title { get; set; }

    [Event] public Action<Event<Blade>>? OnClose { get; set; }

    [Event] public Action<Event<Blade>>? OnRefresh { get; set; }
}