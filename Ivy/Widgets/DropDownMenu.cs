using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Shared;
using Ivy.Core;
using Ivy.Core.Docs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a dropdown menu widget that provides interactive menu options
/// with customizable positioning, alignment, and selection handling. This widget
/// creates collapsible menus that appear when triggered, offering users a
/// clean and organized way to access various actions and navigation options.
/// 
/// The DropDownMenu widget supports flexible positioning relative to its trigger
/// element, customizable alignment options, and comprehensive event handling for
/// user selections. It can contain various menu item types including regular
/// items, separators, checkboxes, and nested submenus for complex navigation
/// structures.
/// </summary>
public record DropDownMenu : WidgetBase<DropDownMenu>
{
    /// <summary>
    /// Defines the available positions where the dropdown menu can appear
    /// relative to its trigger element, controlling the spatial relationship
    /// between the trigger and the displayed menu.
    /// </summary>
    public enum SideOptions
    {
        /// <summary>Position the dropdown menu above the trigger element.</summary>
        Top,
        /// <summary>Position the dropdown menu to the right of the trigger element.</summary>
        Right,
        /// <summary>Position the dropdown menu below the trigger element. This is the default behavior.</summary>
        Bottom,
        /// <summary>Position the dropdown menu to the left of the trigger element.</summary>
        Left
    }

    /// <summary>
    /// Defines the alignment options for positioning the dropdown menu
    /// relative to its trigger element, controlling how the menu is
    /// horizontally or vertically aligned with the trigger.
    /// </summary>
    public enum AlignOptions
    {
        /// <summary>Align the dropdown menu to the start of the trigger element (left for horizontal, top for vertical).</summary>
        Start,
        /// <summary>Center the dropdown menu relative to the trigger element.</summary>
        Center,
        /// <summary>Align the dropdown menu to the end of the trigger element (right for horizontal, bottom for vertical).</summary>
        End
    }

    /// <summary>
    /// Initializes a new instance of the DropDownMenu class with the specified
    /// selection handler, trigger element, and menu items. The dropdown menu
    /// will be positioned relative to the trigger and display the provided
    /// menu options for user interaction.
    /// </summary>
    /// <param name="onSelect">Event handler that is called when a menu item
    /// is selected by the user. This handler receives the dropdown event context
    /// and the selected item value, allowing you to process the selection
    /// and perform appropriate actions.</param>
    /// <param name="trigger">The element that triggers the dropdown menu to
    /// appear when clicked or activated. This can be any widget including
    /// buttons, text, or custom elements.</param>
    /// <param name="items">Variable number of MenuItem collections that define
    /// the menu structure, content, and behavior for the dropdown interface.</param>
    [OverloadResolutionPriority(1)]
    public DropDownMenu(Func<Event<DropDownMenu, object>, ValueTask> onSelect, object trigger, params IEnumerable<MenuItem> items) : base([new Slot("Trigger", trigger)])
    {
        OnSelect = onSelect;
        Items = items.ToArray();
    }

    /// <summary>
    /// Provides a default selection handler that automatically processes menu item
    /// selections using their built-in select handlers. This method creates a
    /// standard event handler that delegates selection processing to individual
    /// menu items, simplifying common dropdown menu implementations.
    /// 
    /// The default handler automatically invokes the select handler of the chosen
    /// menu item, enabling menu items to manage their own selection behavior
    /// without requiring custom event handling logic.
    /// </summary>
    /// <returns>A default event handler that processes menu item selections automatically.</returns>
    public static Func<Event<DropDownMenu, object>, ValueTask> DefaultSelectHandler()
    {
        return (@evt) =>
        {
            @evt.Sender.Items.GetSelectHandler(@evt.Value)?.Invoke();
            return ValueTask.CompletedTask;
        };
    }

    /// <summary>
    /// Compatibility constructor for Action-based event handlers.
    /// Automatically wraps Action delegates in ValueTask-returning functions for backward compatibility.
    /// </summary>
    /// <param name="onSelect">Action-based event handler that is called when a menu item is selected.</param>
    /// <param name="trigger">The element that triggers the dropdown menu to appear when clicked or activated.</param>
    /// <param name="items">Variable number of MenuItem collections that define the menu structure.</param>
    public DropDownMenu(Action<Event<DropDownMenu, object>> onSelect, object trigger, params IEnumerable<MenuItem> items)
        : this(e => { onSelect(e); return ValueTask.CompletedTask; }, trigger, items)
    {
    }

    /// <summary>
    /// Gets or sets the array of menu items that make up the dropdown menu content.
    /// This property contains all the menu options, separators, and nested submenus
    /// that users can interact with when the dropdown is displayed.
    /// 
    /// Menu items can include regular options, separators, checkboxes, and nested
    /// submenus, allowing for complex navigation structures and user interactions.
    /// </summary>
    [Prop] public MenuItem[] Items { get; set; }

    /// <summary>
    /// Gets or sets the side where the dropdown menu appears relative to its trigger.
    /// This property controls the spatial positioning of the menu, allowing you to
    /// choose the most appropriate location based on available space and design
    /// requirements.
    /// 
    /// The side option determines whether the menu appears above, below, to the left,
    /// or to the right of the trigger element, with automatic adjustments for
    /// available screen space.
    /// Default is <see cref="SideOptions.Bottom"/>.
    /// </summary>
    [Prop] public SideOptions Side { get; set; } = SideOptions.Bottom;

    /// <summary>
    /// Gets or sets the offset distance from the trigger element in the side direction.
    /// This property controls the spacing between the trigger and the dropdown menu,
    /// allowing you to create visually appealing layouts with appropriate separation.
    /// 
    /// The side offset is applied in the direction specified by the Side property,
    /// creating consistent spacing regardless of the chosen positioning.
    /// Default is 8 pixels.
    /// </summary>
    [Prop] public int SideOffset { get; set; } = 8;

    /// <summary>
    /// Gets or sets the alignment of the dropdown menu relative to its trigger.
    /// This property controls how the menu is positioned horizontally or vertically
    /// relative to the trigger element, ensuring proper visual alignment.
    /// 
    /// Alignment options allow you to position the menu at the start, center, or
    /// end of the trigger element, creating balanced and visually appealing layouts.
    /// Default is <see cref="AlignOptions.Start"/>.
    /// </summary>
    [Prop] public AlignOptions Align { get; set; } = AlignOptions.Start;

    /// <summary>
    /// Gets or sets the offset distance from the trigger element in the alignment direction.
    /// This property controls the fine-tuning of menu positioning, allowing you to
    /// create precise layouts with custom alignment adjustments.
    /// 
    /// The align offset is applied in the direction specified by the Align property,
    /// enabling precise control over menu positioning for optimal visual balance.
    /// Default is 0 pixels (no additional offset).
    /// </summary>
    [Prop] public int AlignOffset { get; set; } = 0;

    /// <summary>
    /// Gets or sets the event handler that is called when a menu item is selected.
    /// This event handler receives the dropdown event context and the selected item
    /// value, enabling you to process user selections and perform appropriate actions.
    /// </summary>
    [Event] public Func<Event<DropDownMenu, object>, ValueTask> OnSelect { get; set; }

    /// <summary>
    /// Operator overload that allows adding MenuItem objects to the dropdown menu
    /// using the pipe operator. This operator enables convenient menu construction
    /// by allowing you to chain menu items together for better readability.
    /// 
    /// The operator automatically appends the new menu item to the existing items
    /// array, maintaining the menu structure while enabling fluent menu building.
    /// Only MenuItem objects are supported to maintain menu integrity.
    /// </summary>
    /// <param name="widget">The DropDownMenu to add the menu item to.</param>
    /// <param name="child">The MenuItem to add to the dropdown menu.</param>
    /// <returns>A new DropDownMenu instance with the additional menu item appended.</returns>
    /// <exception cref="NotSupportedException">Thrown when attempting to add non-MenuItem children.</exception>
    public static DropDownMenu operator |(DropDownMenu widget, object child)
    {
        if (child is MenuItem menuItem)
        {
            return widget with { Items = widget.Items.Append(menuItem).ToArray() };
        }

        throw new NotSupportedException("DropDownMenu does not support children other then MenuItem.");
    }
}

/// <summary>
/// Provides extension methods for the DropDownMenu widget that enable a fluent API for
/// configuring dropdown appearance, positioning, and behavior. These methods allow you
/// to easily set positioning options, alignment settings, and event handlers for
/// optimal dropdown menu presentation and functionality.
/// </summary>
public static class DropDownMenuExtensions
{
    /// <summary>
    /// Creates a dropdown menu from a button with the specified menu items.
    /// This convenience method simplifies the creation of button-triggered dropdowns
    /// by automatically setting up the button as the trigger and using the default
    /// selection handler for automatic menu item processing.
    /// </summary>
    /// <param name="button">The button that will trigger the dropdown menu.</param>
    /// <param name="items">Array of MenuItem objects that define the dropdown menu structure.</param>
    /// <returns>A new DropDownMenu instance configured with the button as trigger and default selection handling.</returns>
    public static DropDownMenu WithDropDown(this Button button, params MenuItem[] items)
    {
        return new DropDownMenu(DropDownMenu.DefaultSelectHandler(), button, items);
    }

    /// <summary>
    /// Adds a header section to the dropdown menu.
    /// This method allows you to include informational content at the top of the
    /// dropdown menu, such as user information, context details, or descriptive text
    /// that helps users understand the menu's purpose or context.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to add the header to.</param>
    /// <param name="header">The header content to display at the top of the dropdown menu.</param>
    /// <returns>A new DropDownMenu instance with the header content added.</returns>
    public static DropDownMenu Header(this DropDownMenu dropDownMenu, object header)
    {
        return dropDownMenu with { Children = [.. dropDownMenu.Children, new Slot("Header", header)] };
    }

    /// <summary>
    /// Sets the alignment of the dropdown menu relative to its trigger.
    /// This method allows you to control how the menu is positioned horizontally
    /// or vertically relative to the trigger element, ensuring proper visual alignment.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="align">The alignment option to apply to the dropdown menu.</param>
    /// <returns>A new DropDownMenu instance with the updated alignment setting.</returns>
    public static DropDownMenu Align(this DropDownMenu dropDownMenu, DropDownMenu.AlignOptions align)
    {
        return dropDownMenu with { Align = align };
    }

    /// <summary>
    /// Sets the alignment offset for the dropdown menu positioning.
    /// This method allows you to fine-tune the menu's alignment positioning,
    /// creating precise layouts with custom alignment adjustments.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="offset">The offset distance in pixels to apply in the alignment direction.</param>
    /// <returns>A new DropDownMenu instance with the updated alignment offset.</returns>
    public static DropDownMenu AlignOffset(this DropDownMenu dropDownMenu, int offset)
    {
        return dropDownMenu with { AlignOffset = offset };
    }

    /// <summary>
    /// Sets the side where the dropdown menu appears relative to its trigger.
    /// This method allows you to control the spatial positioning of the menu,
    /// choosing the most appropriate location based on available space and design requirements.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="side">The side option to apply for menu positioning.</param>
    /// <returns>A new DropDownMenu instance with the updated side setting.</returns>
    public static DropDownMenu Side(this DropDownMenu dropDownMenu, DropDownMenu.SideOptions side)
    {
        return dropDownMenu with { Side = side };
    }

    /// <summary>
    /// Sets the side offset for the dropdown menu positioning.
    /// This method allows you to control the spacing between the trigger and
    /// the dropdown menu, creating visually appealing layouts with appropriate separation.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="offset">The offset distance in pixels to apply in the side direction.</param>
    /// <returns>A new DropDownMenu instance with the updated side offset.</returns>
    public static DropDownMenu SideOffset(this DropDownMenu dropDownMenu, int offset)
    {
        return dropDownMenu with { SideOffset = offset };
    }

    /// <summary>
    /// Sets the dropdown menu to appear above the trigger element.
    /// This convenience method positions the menu above the trigger, useful when
    /// there is limited space below or when the trigger is near the bottom of the viewport.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <returns>A new DropDownMenu instance positioned above the trigger.</returns>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Top(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Top };
    }

    /// <summary>
    /// Sets the dropdown menu to appear to the right of the trigger element.
    /// This convenience method positions the menu to the right of the trigger, useful when
    /// there is limited space to the left or when the trigger is near the left edge of the viewport.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <returns>A new DropDownMenu instance positioned to the right of the trigger.</returns>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Right(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Right };
    }

    /// <summary>
    /// Sets the dropdown menu to appear below the trigger element.
    /// This convenience method positions the menu below the trigger, which is the
    /// default behavior and typically provides the most natural user experience.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <returns>A new DropDownMenu instance positioned below the trigger.</returns>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Bottom(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Bottom };
    }

    /// <summary>
    /// Sets the dropdown menu to appear to the left of the trigger element.
    /// This convenience method positions the menu to the left of the trigger, useful when
    /// there is limited space to the right or when the trigger is near the right edge of the viewport.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <returns>A new DropDownMenu instance positioned to the left of the trigger.</returns>
    [RelatedTo(nameof(DropDownMenu.Side))]
    public static DropDownMenu Left(this DropDownMenu dropDownMenu)
    {
        return dropDownMenu with { Side = DropDownMenu.SideOptions.Left };
    }

    /// <summary>
    /// Sets the menu items for the dropdown menu.
    /// This method allows you to replace or update the entire menu structure
    /// after creation, enabling dynamic menu content based on context or user state.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="items">Array of MenuItem objects that define the new menu structure.</param>
    /// <returns>A new DropDownMenu instance with the updated menu items.</returns>
    public static DropDownMenu Items(this DropDownMenu dropDownMenu, MenuItem[] items)
    {
        return dropDownMenu with { Items = items };
    }

    /// <summary>
    /// Sets the selection event handler for the dropdown menu.
    /// This method allows you to configure custom selection handling logic,
    /// enabling you to process menu item selections according to your application's
    /// specific requirements and business logic.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="onSelect">The event handler to call when a menu item is selected.</param>
    /// <returns>A new DropDownMenu instance with the updated selection handler.</returns>
    [OverloadResolutionPriority(1)]
    public static DropDownMenu HandleSelect(this DropDownMenu dropDownMenu, Func<Event<DropDownMenu, object>, ValueTask> onSelect)
    {
        return dropDownMenu with { OnSelect = onSelect };
    }

    /// <summary>
    /// Sets the event handler for menu item selection with an Action-based handler.
    /// This compatibility overload allows using traditional synchronous Action delegates
    /// while automatically wrapping them for async compatibility.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="onSelect">The Action-based event handler to call when a menu item is selected.</param>
    /// <returns>A new DropDownMenu instance with the updated selection handler.</returns>
    public static DropDownMenu HandleSelect(this DropDownMenu dropDownMenu, Action<Event<DropDownMenu, object>> onSelect)
    {
        return dropDownMenu with { OnSelect = onSelect.ToValueTask() };
    }

    /// <summary>
    /// Sets the event handler for menu item selection with a simplified handler that receives only the selected value.
    /// This convenience overload allows handling selections without the full event context,
    /// useful when you only need to process the selected item value.
    /// </summary>
    /// <param name="dropDownMenu">The DropDownMenu to configure.</param>
    /// <param name="onSelect">The simplified handler that receives only the selected item value.</param>
    /// <returns>A new DropDownMenu instance with the updated selection handler.</returns>
    public static DropDownMenu HandleSelect(this DropDownMenu dropDownMenu, Action<object> onSelect)
    {
        return dropDownMenu with { OnSelect = @event => { onSelect(@event.Value); return ValueTask.CompletedTask; } };
    }
}