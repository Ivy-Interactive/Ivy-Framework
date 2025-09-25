using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Shared;
using Ivy.Core;
using Ivy.Core.Docs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Dropdown menu widget with customizable positioning and selection handling.</summary>
public record DropDownMenu : WidgetBase<DropDownMenu>
{
    /// <summary>Dropdown positioning options relative to trigger.</summary>
    public enum SideOptions
    {
        /// <summary>Position above trigger.</summary>
        Top,
        /// <summary>Position to the right of trigger.</summary>
        Right,
        /// <summary>Position below trigger (default).</summary>
        Bottom,
        /// <summary>Position to the left of trigger.</summary>
        Left
    }

    /// <summary>Alignment options relative to trigger.</summary>
    public enum AlignOptions
    {
        /// <summary>Align to start of trigger.</summary>
        Start,
        /// <summary>Center relative to trigger.</summary>
        Center,
        /// <summary>Align to end of trigger.</summary>
        End
    }

    /// <summary>Initializes DropDownMenu with selection handler, trigger, and menu items.</summary>
    /// <param name="onSelect">Event handler for menu selection.</param>
    /// <param name="trigger">Element that triggers the dropdown.</param>
    /// <param name="items">Menu items defining structure and content.</param>
    [OverloadResolutionPriority(1)]
    public DropDownMenu(Func<Event<DropDownMenu, object>, ValueTask> onSelect, object trigger, params IEnumerable<MenuItem> items) : base([new Slot("Trigger", trigger)])
    {
        OnSelect = onSelect;
        Items = items.ToArray();
    }

    /// <summary>Provides default selection handler that processes menu selections automatically.</summary>
    public static Func<Event<DropDownMenu, object>, ValueTask> DefaultSelectHandler()
    {
        return (@evt) =>
        {
            @evt.Sender.Items.GetSelectHandler(@evt.Value)?.Invoke();
            return ValueTask.CompletedTask;
        };
    }

    /// <summary>Constructor for Action-based event handlers.</summary>
    /// <param name="onSelect">Action-based event handler for menu selection.</param>
    /// <param name="trigger">Element that triggers the dropdown.</param>
    /// <param name="items">Menu items defining structure.</param>
    public DropDownMenu(Action<Event<DropDownMenu, object>> onSelect, object trigger, params IEnumerable<MenuItem> items)
        : this(e => { onSelect(e); return ValueTask.CompletedTask; }, trigger, items)
    {
    }

    /// <summary>Array of menu items including options, separators, and submenus.</summary>
    [Prop] public MenuItem[] Items { get; set; }

    /// <summary>Side where dropdown appears relative to trigger. Default is <see cref="SideOptions.Bottom"/>.</summary>
    [Prop] public SideOptions Side { get; set; } = SideOptions.Bottom;

    /// <summary>Alignment relative to trigger. Default is <see cref="AlignOptions.Start"/>.</summary>
    [Prop] public AlignOptions Align { get; set; } = AlignOptions.Start;

    /// <summary>Offset distance from trigger in alignment direction. Default is 0 pixels.</summary>
    [Prop] public int AlignOffset { get; set; } = 0;

    /// <summary>Event handler for menu item selection.</summary>
    [Event] public Func<Event<DropDownMenu, object>, ValueTask> OnSelect { get; set; }

    /// <summary>Adds MenuItem using pipe operator. Throws NotSupportedException for non-MenuItem children.</summary>
    public static DropDownMenu operator |(DropDownMenu widget, object child)
    {
        if (child is MenuItem menuItem)
        {
            return widget with { Items = widget.Items.Append(menuItem).ToArray() };
        }

        throw new NotSupportedException("DropDownMenu does not support children other then MenuItem.");
    }
}

/// <summary>Extension methods for DropDownMenu providing fluent API for configuration.</summary>
public static class DropDownMenuExtensions
{
    /// <summary>Creates dropdown menu from button with menu items using default selection handler.</summary>
    public static DropDownMenu WithDropDown(this Button button, params MenuItem[] items)
    {
        return new DropDownMenu(DropDownMenu.DefaultSelectHandler(), button, items);
    }

    /// <summary>Adds header section for informational content.</summary>
    public static DropDownMenu Header(this DropDownMenu dropDownMenu, object header)
    {
        return dropDownMenu with { Children = [.. dropDownMenu.Children, new Slot("Header", header)] };
    }

    /// <summary>Sets alignment relative to trigger.</summary>
    public static DropDownMenu Align(this DropDownMenu dropDownMenu, DropDownMenu.AlignOptions align)
    {
        return dropDownMenu with { Align = align };
    }

    /// <summary>Sets alignment offset for positioning.</summary>
    public static DropDownMenu AlignOffset(this DropDownMenu dropDownMenu, int offset)
    {
        return dropDownMenu with { AlignOffset = offset };
    }

    /// <summary>Sets side where dropdown appears relative to trigger.</summary>
    public static DropDownMenu Side(this DropDownMenu dropDownMenu, DropDownMenu.SideOptions side)
    {
        return dropDownMenu with { Side = side };
    }

    /// <summary>Sets dropdown to appear above trigger.</summary>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Top(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Top };
    }

    /// <summary>Sets dropdown to appear to the right of trigger.</summary>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Right(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Right };
    }

    /// <summary>Sets dropdown to appear below trigger (default).</summary>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Bottom(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Bottom };
    }

    /// <summary>Sets dropdown to appear to the left of trigger.</summary>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Left(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Left };
    }

    /// <summary>Sets menu items enabling dynamic menu content.</summary>
    public static DropDownMenu Items(this DropDownMenu dropDownMenu, MenuItem[] items)
    {
        return dropDownMenu with { Items = items };
    }

    /// <summary>Sets selection event handler.</summary>
    [OverloadResolutionPriority(1)]
    public static DropDownMenu HandleSelect(this DropDownMenu dropDownMenu, Func<Event<DropDownMenu, object>, ValueTask> onSelect)
    {
        return dropDownMenu with { OnSelect = onSelect };
    }

    /// <summary>Sets Action-based event handler for menu selection.</summary>
    public static DropDownMenu HandleSelect(this DropDownMenu dropDownMenu, Action<Event<DropDownMenu, object>> onSelect)
    {
        return dropDownMenu with { OnSelect = onSelect.ToValueTask() };
    }

    /// <summary>Sets simplified event handler receiving only selected value.</summary>
    public static DropDownMenu HandleSelect(this DropDownMenu dropDownMenu, Action<object> onSelect)
    {
        return dropDownMenu with { OnSelect = @event => { onSelect(@event.Value); return ValueTask.CompletedTask; } };
    }
}