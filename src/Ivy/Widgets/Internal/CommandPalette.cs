using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Widgets.Internal;

public record CommandPalette : WidgetBase<CommandPalette>
{
    [Prop] public MenuItem[] Items { get; set; } = [];
    [Event] public Func<Event<CommandPalette, string>, ValueTask>? OnSelect { get; set; }

    public CommandPalette(params object[] children) : base(children)
    {
    }

    public CommandPalette(MenuItem[] items, Func<Event<CommandPalette, string>, ValueTask> onSelect)
    {
        Items = items;
        OnSelect = onSelect;
    }
}
