using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a sidebar layout widget that provides a flexible sidebar navigation layout with a main content area and collapsible sidebar.
/// </summary>
public record SidebarLayout : WidgetBase<SidebarLayout>
{
    /// <summary>
    /// Initializes a new instance of the SidebarLayout class with the specified main content and sidebar content, along with optional sidebar header and footer sections.
    /// The layout automatically manages the space distribution between the sidebar and main content areas.
    /// </summary>
    /// <param name="mainContent">The primary content to display in the main area of the layout. This content takes up the remaining space after the sidebar and can contain any combination of widgets or content elements.</param>
    /// <param name="sidebarContent">The content to display in the sidebar area. This typically contains navigation menus, tools, or supplementary information that users need quick access to.</param>
    /// <param name="sidebarHeader">Optional header content for the sidebar. This can include branding, search functionality, or section titles that appear at the top of the sidebar.</param>
    /// <param name="sidebarFooter">Optional footer content for the sidebar. This can include user information, version details, or additional controls that appear at the bottom of the sidebar.</param>
    public SidebarLayout(object mainContent, object sidebarContent, object? sidebarHeader = null, object? sidebarFooter = null)
        : base([new Slot("MainContent", mainContent), new Slot("SidebarContent", sidebarContent), new Slot("SidebarHeader", sidebarHeader), new Slot("SidebarFooter", sidebarFooter)])
    {
    }

    /// <summary>
    /// Gets or sets whether the sidebar will be the main application's sidebar.
    /// When true, this controls the sidebar's behavior when the app is embedded in a page, creating a toggle button in the top right corner of the sidebar for responsive behavior.
    /// Main app sidebars typically include additional features like automatic collapsing on smaller screens and integration with the application's overall navigation system.
    /// </summary>
    [Prop] public bool MainAppSidebar { get; set; } = false;

    /// <summary>
    /// Controls the padding for the main content area in units.
    /// This property sets the internal spacing around the main content, providing visual separation and breathing room for the primary content elements.
    /// </summary>
    [Prop] public int MainContentPadding { get; set; } = 2;

    /// <summary>
    /// Operator overload that prevents adding children to the SidebarLayout using the pipe operator.
    /// SidebarLayout uses a predefined four-slot system (MainContent, SidebarContent, SidebarHeader, and SidebarFooter) and does not support additional children beyond the initial parameters.
    /// This restriction ensures that the layout maintains its intended structure with proper sidebar and main content areas, preventing accidental modification of the layout structure.
    /// </summary>
    /// <param name="widget">The SidebarLayout widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <exception cref="NotSupportedException">Always thrown, as SidebarLayout does not support additional children.</exception>
    public static SidebarLayout operator |(SidebarLayout widget, object child)
    {
        throw new NotSupportedException("SidebarLayout does not support children.");
    }
}

/// <summary>
/// Provides extension methods for the SidebarLayout widget that enable a fluent API for configuring sidebar behavior and styling.
/// </summary>
public static class SidebarLayoutExtensions
{
    /// <summary>
    /// Sets whether the sidebar should behave as the main application sidebar.
    /// This method allows you to configure the sidebar's behavior after creation, enabling or disabling main app sidebar features like toggle buttons and responsive behavior.
    /// </summary>
    /// <param name="sidebar">The SidebarLayout to configure.</param>
    /// <param name="isMainApp">Whether the sidebar should behave as the main application sidebar. Default is true.</param>
    public static SidebarLayout MainAppSidebar(this SidebarLayout sidebar, bool isMainApp = true)
    {
        return sidebar with { MainAppSidebar = isMainApp };
    }

    /// <summary>
    /// Sets the padding for the main content area in units.
    /// This method allows you to adjust the internal spacing around the main content, providing visual separation and breathing room for the primary content elements.
    /// </summary>
    /// <param name="sidebar">The SidebarLayout to configure.</param>
    /// <param name="padding">The padding value in units to apply to the main content area.</param>
    public static SidebarLayout Padding(this SidebarLayout sidebar, int padding)
    {
        return sidebar with { MainContentPadding = padding };
    }
}

/// <summary>
/// Represents a specialized sidebar menu widget designed specifically for sidebar navigation within SidebarLayout components.
/// This widget provides advanced navigation features including search highlighting, keyboard navigation, and hierarchical menu structures.
/// The SidebarMenu widget is optimized for sidebar usage and integrates seamlessly with SidebarLayout to create comprehensive navigation systems with event handling for user interactions and menu selections.
/// </summary>
public record SidebarMenu : WidgetBase<SidebarLayout>
{
    /// <summary>
    /// Initializes a new instance of the SidebarMenu class with the specified selection event handler and menu items.
    /// The menu will handle user interactions and provide navigation functionality optimized for sidebar layouts.
    /// </summary>
    /// <param name="onSelect">The event handler that is called when a menu item is selected by the user. This handler receives the selected menu item and can perform navigation or other actions.</param>
    /// <param name="items">Variable number of MenuItem elements that define the menu structure,
    /// including navigation options, hierarchical menus, and interactive elements.</param>
    [OverloadResolutionPriority(1)]
    public SidebarMenu(Func<Event<SidebarMenu, object>, ValueTask> onSelect, params MenuItem[] items)
    {
        OnSelect = onSelect;
        Items = items;
    }

    /// <summary>
    /// Gets or sets whether the search functionality is currently active in the sidebar menu.
    /// When true, the menu displays search capabilities that allow users to quickly find specific menu items through text input and highlighting.
    /// </summary>
    [Prop] public bool SearchActive { get; set; } = false;

    /// <summary>
    /// Gets or sets the array of menu items that make up the sidebar menu structure.
    /// Each MenuItem can represent a navigation option, section header, or hierarchical menu with nested items, creating a comprehensive navigation system.
    /// </summary>
    [Prop] public MenuItem[] Items { get; set; }

    /// <summary>
    /// Gets or sets the event handler that is called when a menu item is selected by the user.
    /// This handler receives the selected menu item and can perform navigation, state updates, or other actions based on the user's selection.
    /// </summary>
    [Event] public Func<Event<SidebarMenu, object>, ValueTask> OnSelect { get; set; }

    /// <summary>
    /// Gets or sets the optional event handler for Ctrl+Right-click events on menu items.
    /// This handler provides additional interaction options for power users, allowing for context menus, secondary actions, or alternative navigation behaviors.
    /// </summary>
    [Event] public Func<Event<SidebarMenu, object>, ValueTask>? OnCtrlRightClickSelect { get; set; }

    /// <summary>
    /// Operator overload that prevents adding children to the SidebarMenu using the pipe operator.
    /// SidebarMenu uses a predefined structure with MenuItem arrays and event handlers, and does not support additional children beyond the initial menu configuration.
    /// This restriction ensures that the menu maintains its intended structure and functionality, preventing accidental modification of the menu system.
    /// </summary>
    /// <param name="widget">The SidebarMenu widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <exception cref="NotSupportedException">Always thrown, as SidebarMenu does not support additional children.</exception>
    public static SidebarMenu operator |(SidebarMenu widget, object child)
    {
        throw new NotSupportedException("SidebarMenu does not support children.");
    }

    /// <summary>
    /// Compatibility constructor for Action-based event handlers. Automatically wraps Action delegates in ValueTask-returning functions for backward compatibility.
    /// </summary>
    /// <param name="onSelect">Optional Action-based event handler for menu item selection events.</param>
    /// <param name="items">Variable number of MenuItem elements that define the menu structure.</param>
    public SidebarMenu(Action<Event<SidebarMenu, object>> onSelect, params MenuItem[] items)
        : this(e => { onSelect(e); return ValueTask.CompletedTask; }, items)
    {
    }
}

