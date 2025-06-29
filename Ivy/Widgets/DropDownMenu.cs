using Ivy.Shared;
using Ivy.Core;
using Ivy.Core.Docs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record DropDownMenu : WidgetBase<DropDownMenu>
{
    public enum SideOptions
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum AlignOptions
    {
        Start,
        Center,
        End
    }

    public DropDownMenu(Action<Event<DropDownMenu, object>> onSelect, object trigger, params IEnumerable<MenuItem> items) : base([new Slot("Trigger", trigger)])
    {
        OnSelect = onSelect;
        Items = items.ToArray();
    }

    [Prop] public MenuItem[] Items { get; set; }
    [Prop] public SideOptions Side { get; set; } = SideOptions.Bottom;
    [Prop] public int SideOffset { get; set; } = 8;
    [Prop] public AlignOptions Align { get; set; } = AlignOptions.Start;
    [Prop] public int AlignOffset { get; set; } = 0;
    [Event] public Action<Event<DropDownMenu, object>> OnSelect { get; set; }

    public static DropDownMenu operator |(DropDownMenu widget, object child)
    {
        if (child is MenuItem menuItem)
        {
            return widget with { Items = widget.Items.Append(menuItem).ToArray() };
        }

        throw new NotSupportedException("DropDownMenu does not support children other then MenuItem.");
    }

    public static Action<Event<DropDownMenu, object>> DefaultSelectHandler()
    {
        return (@evt) => @evt.Sender.Items.GetSelectHandler(@evt.Value)?.Invoke();
    }
}

public static class DropDownMenuExtensions
{
    public static DropDownMenu WithDropDown(this Button button, params MenuItem[] items)
    {
        return new DropDownMenu(DropDownMenu.DefaultSelectHandler(), button, items);
    }

    public static DropDownMenu Header(this DropDownMenu dropDownMenu, object header)
    {
        return dropDownMenu with { Children = [.. dropDownMenu.Children, new Slot("Header", header)] };
    }

    public static DropDownMenu Align(this DropDownMenu dropDownMenu, DropDownMenu.AlignOptions align)
    {
        return dropDownMenu with { Align = align };
    }

    public static DropDownMenu AlignOffset(this DropDownMenu dropDownMenu, int offset)
    {
        return dropDownMenu with { AlignOffset = offset };
    }

    public static DropDownMenu Side(this DropDownMenu dropDownMenu, DropDownMenu.SideOptions side)
    {
        return dropDownMenu with { Side = side };
    }

    public static DropDownMenu SideOffset(this DropDownMenu dropDownMenu, int offset)
    {
        return dropDownMenu with { SideOffset = offset };
    }

    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Top(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Top };
    }

    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Right(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Right };
    }

    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Bottom(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Bottom };
    }

    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Left(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Left };
    }

    public static DropDownMenu Items(this DropDownMenu dropDownMenu, MenuItem[] items)
    {
        return dropDownMenu with { Items = items };
    }

    public static DropDownMenu HandleSelect(this DropDownMenu dropDownMenu, Action<Event<DropDownMenu, object>> onSelect)
    {
        return dropDownMenu with { OnSelect = onSelect };
    }
}