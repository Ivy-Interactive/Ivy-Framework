using System.Reactive;
using System.Runtime.CompilerServices;
using Ivy.Apps;
using Ivy.Charts;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual style variants available for button widgets, controlling their
/// appearance, color scheme, and visual emphasis to suit different use cases and
/// design requirements. Each variant provides distinct visual styling appropriate
/// for different button purposes and contexts.
/// </summary>
public enum ButtonVariant
{
    /// <summary>Default primary button style with prominent visual emphasis, typically used for main actions and primary user interactions.</summary>
    Primary,
    /// <summary>Destructive button style with warning or error colors, typically used for critical actions like delete or remove operations.</summary>
    Destructive,
    /// <summary>Outline button style with bordered appearance, typically used for secondary actions or alternative choices.</summary>
    Outline,
    /// <summary>Secondary button style with reduced visual emphasis, typically used for supporting actions or less prominent interactions.</summary>
    Secondary,
    /// <summary>Ghost button style with minimal visual styling, typically used for subtle actions or tertiary interactions.</summary>
    Ghost,
    /// <summary>Link button style that appears as a clickable link, typically used for navigation or external references.</summary>
    Link,
    /// <summary>Inline button style that integrates seamlessly with text content, typically used for embedded actions within text.</summary>
    Inline
}

/// <summary>
/// Represents a button widget that provides interactive elements for user actions,
/// navigation, and form submissions. 
/// 
/// The Button widget supports multiple visual variants, sizes, icon integration,
/// and interactive states to create engaging and accessible user interfaces.
/// Buttons can be configured for various purposes including form submissions,
/// navigation, action triggers, and user feedback.
/// </summary>
public record Button : WidgetBase<Button>
{
    /// <summary>
    /// Initializes a new instance of the Button class with the specified title,
    /// click handler, variant, and icon. The button will display the provided
    /// information and respond to user interactions based on the configuration.
    /// </summary>
    /// <param name="title">The text content to display on the button. This can be
    /// a label, action description, or navigation text. When null, creates an
    /// icon-only button.</param>
    /// <param name="onClick">Optional event handler that is called when the button
    /// is clicked by the user. This handler receives the button event context
    /// and can perform any desired action.</param>
    /// <param name="variant">The visual style variant for the button, controlling
    /// its appearance and color scheme. Default is <see cref="ButtonVariant.Primary"/>.</param>
    /// <param name="icon">The icon to display alongside or instead of the title text.
    /// This enhances visual meaning and can be positioned on either side of the text.
    /// Default is <see cref="Icons.None"/>.</param>
    [OverloadResolutionPriority(1)]
    public Button(string? title = null, Func<Event<Button>, ValueTask>? onClick = null, ButtonVariant variant = ButtonVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick;
    }

    public Button(string? title = null, Action<Event<Button>>? onClick = null, ButtonVariant variant = ButtonVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick?.ToValueTask();
    }

    public Button(string? title = null, Action? onClick = null, ButtonVariant variant = ButtonVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick == null ? null : (_ => { onClick(); return ValueTask.CompletedTask; });
    }

    /// <summary>
    /// Gets or sets the text content displayed on the button.
    /// This property contains the primary information that the button communicates,
    /// such as action labels, navigation text, or descriptive content that guides
    /// users on what will happen when they click the button.
    /// 
    /// When null, the button becomes icon-only, displaying only the icon without
    /// any text content. This is useful for creating compact visual buttons where
    /// the icon alone conveys the necessary information.
    /// Default is null (icon-only button).
    /// </summary>
    [Prop] public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the visual style variant for the button.
    /// This property controls the button's appearance, color scheme, and visual
    /// emphasis, allowing you to choose the most appropriate style for your
    /// use case and design requirements.
    /// 
    /// Different variants provide different levels of visual emphasis and are
    /// suited for different purposes, from primary actions to subtle interactions.
    /// Default is <see cref="ButtonVariant.Primary"/>.
    /// </summary>
    [Prop] public ButtonVariant Variant { get; set; }

    /// <summary>
    /// Gets or sets the icon displayed on the button.
    /// This property allows you to add visual context and meaning to the button,
    /// enhancing its communicative value and visual appeal.
    /// 
    /// Icons can be positioned on either side of the text (or used alone for
    /// icon-only buttons) and should be chosen to complement the button's purpose
    /// and content.
    /// Default is <see cref="Icons.None"/>.
    /// </summary>
    [Prop] public Icons? Icon { get; set; }

    /// <summary>
    /// Gets or sets the position of the icon relative to the title text.
    /// This property controls whether the icon appears to the left or right
    /// of the text content, allowing you to create visually balanced and
    /// contextually appropriate button layouts.
    /// 
    /// Icon positioning can affect the visual flow and readability of the
    /// button, with left positioning being more common for most use cases.
    /// Default is <see cref="Align.Left"/>.
    /// </summary>
    [Prop] public Align IconPosition { get; set; } = Align.Left;

    /// <summary>
    /// Gets or sets the foreground color for the button text and icon.
    /// This property allows you to customize the color of the button's text
    /// and icon content, enabling brand-specific styling or visual emphasis
    /// that matches your application's design system.
    /// 
    /// When null, the button uses the default color scheme for its variant.
    /// Default is null (uses variant default colors).
    /// </summary>
    [Prop] public Colors? Foreground { get; set; }

    /// <summary>
    /// Gets or sets the URL for navigation when the button is clicked.
    /// This property enables the button to function as a navigation element,
    /// allowing users to navigate to specific URLs or internal routes when
    /// they interact with the button.
    /// 
    /// When set, the button can be used for both action handling (via OnClick)
    /// and navigation (via URL), providing flexible interaction options.
    /// Default is null (no navigation URL).
    /// </summary>
    [Prop] public string? Url { get; set; }

    /// <summary>
    /// Gets or sets whether the button is disabled and cannot be interacted with.
    /// This property controls the button's interactive state, preventing user
    /// interactions when set to true and providing visual feedback about
    /// the button's availability.
    /// 
    /// Disabled buttons typically appear with reduced opacity and cannot
    /// trigger click events or navigation actions.
    /// Default is false (button is enabled).
    /// </summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the tooltip text displayed when hovering over the button.
    /// This property provides additional context and information about the
    /// button's purpose or action, enhancing user understanding and
    /// accessibility.
    /// 
    /// Tooltips are displayed when users hover over the button and provide
    /// helpful information without cluttering the interface.
    /// Default is null (no tooltip).
    /// </summary>
    [Prop] public string? Tooltip { get; set; }

    /// <summary>
    /// Gets or sets whether the button is in a loading state.
    /// This property controls the button's loading indicator, showing users
    /// that an action is in progress and preventing multiple submissions
    /// during processing.
    /// 
    /// Loading buttons typically display a spinner or progress indicator
    /// and may be temporarily disabled to prevent multiple interactions.
    /// Default is false (button is not loading).
    /// </summary>
    [Prop] public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets the border radius for the button's corners.
    /// This property controls the visual styling of the button's edges,
    /// allowing you to create buttons with different levels of corner
    /// rounding to match your design requirements.
    /// 
    /// Different border radius values provide different visual styles,
    /// from sharp corners to fully rounded pill-shaped buttons.
    /// Default is <see cref="BorderRadius.Rounded"/>.
    /// </summary>
    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    /// <summary>
    /// Gets or sets the size of the button.
    /// This property controls the button's dimensions and visual prominence,
    /// allowing you to match it to your design requirements and the
    /// importance of the action being performed.
    /// 
    /// Different sizes provide different levels of visual emphasis and are
    /// suited for different contexts, from compact interfaces to prominent
    /// call-to-action buttons.
    /// Default is <see cref="Sizes.Medium"/>.
    /// </summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>
    /// Gets or sets the event handler that is called when the button is clicked.
    /// This event handler receives the button event context and can perform
    /// any desired action, such as form submissions, navigation, or custom
    /// business logic execution.
    /// 
    /// The event handler provides access to the button context and can be
    /// used to implement complex interaction logic or state management.
    /// Default is null (no click handler).
    /// </summary>
    [Event] public Func<Event<Button>, ValueTask>? OnClick { get; set; }

    /// <summary>
    /// Gets or sets a custom tag object associated with the button.
    /// This property allows you to attach arbitrary data to the button
    /// for custom logic, state management, or integration with external
    /// systems.
    /// 
    /// Default is null (no tag).
    /// </summary>
    public object? Tag { get; set; } //not a prop!

    /// <summary>
    /// Operator overload that prevents adding children to the Button using the pipe operator.
    /// Button widgets are self-contained components that don't support additional
    /// child content beyond their configured title, icon, and styling properties.
    /// 
    /// This restriction ensures that buttons maintain their intended focused
    /// design and prevent accidental modification of their structure.
    /// </summary>
    /// <param name="widget">The Button widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <returns>This method always throws a NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Always thrown, as Button does not support additional children.</exception>
    public static Button operator |(Button widget, object child)
    {
        throw new NotSupportedException("Button does not support children.");
    }
}

/// <summary>
/// Provides extension methods for the Button widget that enable a fluent API for
/// configuring button appearance, behavior, and interaction handling. These methods
/// allow you to easily set variants, sizes, icons, states, and event handlers for
/// optimal button presentation and functionality.
/// </summary>
public static class ButtonExtensions
{
    /// <summary>
    /// Converts an icon to a button with the specified click handler and variant.
    /// </summary>
    /// <param name="icon">The icon to display on the button.</param>
    /// <param name="onClick">Optional event handler for button click events.</param>
    /// <param name="variant">The visual style variant for the button. Default is <see cref="ButtonVariant.Primary"/>.</param>
    /// <returns>A new Button instance configured with the specified icon and settings.</returns>
    [OverloadResolutionPriority(1)]
    public static Button ToButton(this Icons icon, Func<Event<Button>, ValueTask>? onClick = null, ButtonVariant variant = ButtonVariant.Primary)
    {
        return new Button(null, onClick, icon: icon, variant: variant);
    }

    public static Button ToButton(this Icons icon, Action<Event<Button>>? onClick = null, ButtonVariant variant = ButtonVariant.Primary)
    {
        return new Button(null, onClick?.ToValueTask(), icon: icon, variant: variant);
    }

    /// <summary>
    /// Converts a button into a trigger that controls the display of dynamic content.
    /// This method creates a view that shows or hides content based on the button's
    /// click state, enabling interactive UI patterns like dropdowns, modals, or
    /// expandable sections.
    /// </summary>
    /// <param name="trigger">The button that will trigger the content display.</param>
    /// <param name="action">A function that creates the content to display when triggered,
    /// receiving the trigger state for controlling visibility.</param>
    /// <returns>An IView that manages the trigger button and conditional content display.</returns>
    public static IView ToTrigger(this Button trigger, Func<IState<bool>, object> action)
    {
        return new FuncView((context) =>
            {
                var isOpen = context.UseState(false);
                var clonedTrigger = trigger with
                {
                    OnClick = async @event =>
                    {
                        if (trigger.OnClick != null)
                        {
                            await trigger.OnClick(@event);
                        }
                        isOpen.Value = true;
                    }
                };
                return new Fragment(
                    clonedTrigger,
                    isOpen.Value ? action(isOpen) : null
                );
            }
        );
    }

    /// <summary>
    /// Sets the title text for the button.
    /// This method allows you to set or change the button's text content
    /// after creation, enabling dynamic button labeling based on context
    /// or user preferences.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="title">The text content to display on the button.</param>
    /// <returns>A new Button instance with the updated title.</returns>
    public static Button Title(this Button button, string title)
    {
        return button with { Title = title };
    }

    /// <summary>
    /// Sets the navigation URL for the button.
    /// This method allows you to configure the button for navigation purposes,
    /// enabling it to function as a link while maintaining button styling
    /// and interaction capabilities.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="url">The URL to navigate to when the button is clicked.</param>
    /// <returns>A new Button instance with the updated URL.</returns>
    public static Button Url(this Button button, string url)
    {
        return button with { Url = url };
    }

    /// <summary>
    /// Sets the disabled state of the button.
    /// This method allows you to control whether the button can be interacted
    /// with, enabling dynamic state management based on application logic
    /// or user permissions.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="disabled">Whether the button should be disabled. Default is true.</param>
    /// <returns>A new Button instance with the updated disabled state.</returns>
    public static Button Disabled(this Button button, bool disabled = true)
    {
        return button with { Disabled = disabled };
    }

    /// <summary>
    /// Sets the icon and its position for the button.
    /// This method allows you to configure both the icon display and its positioning
    /// relative to the title text, creating visually balanced and contextually
    /// appropriate button layouts.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="icon">The icon to display on the button, or null to remove the icon.</param>
    /// <param name="position">The position of the icon relative to the title text. Default is <see cref="Align.Left"/>.</param>
    /// <returns>A new Button instance with the updated icon and position settings.</returns>
    public static Button Icon(this Button button, Icons? icon, Align position = Align.Left)
    {
        return button with { Icon = icon, IconPosition = position };
    }

    /// <summary>
    /// Sets the visual style variant for the button.
    /// This method allows you to change the button's appearance and color scheme
    /// after creation, enabling dynamic styling based on context or user preferences.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="variant">The visual style variant to apply to the button.</param>
    /// <returns>A new Button instance with the updated variant setting.</returns>
    public static Button Variant(this Button button, ButtonVariant variant)
    {
        return button with { Variant = variant };
    }

    /// <summary>
    /// Sets the foreground color for the button text and icon.
    /// This method allows you to customize the button's text and icon colors,
    /// enabling brand-specific styling or visual emphasis that matches
    /// your application's design system.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="color">The color to apply to the button's text and icon.</param>
    /// <returns>A new Button instance with the updated foreground color.</returns>
    public static Button Foreground(this Button button, Colors color)
    {
        return button with { Foreground = color };
    }

    /// <summary>
    /// Sets the tooltip text for the button.
    /// This method allows you to provide additional context and information
    /// about the button's purpose or action, enhancing user understanding
    /// and accessibility.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="tooltip">The tooltip text to display when hovering over the button.</param>
    /// <returns>A new Button instance with the updated tooltip.</returns>
    public static Button Tooltip(this Button button, string tooltip)
    {
        return button with { Tooltip = tooltip };
    }

    /// <summary>
    /// Sets the loading state of the button.
    /// This method allows you to show users that an action is in progress,
    /// preventing multiple submissions and providing visual feedback about
    /// the button's current state.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="loading">Whether the button should be in a loading state. Default is true.</param>
    /// <returns>A new Button instance with the updated loading state.</returns>
    public static Button Loading(this Button button, bool loading = true)
    {
        return button with { Loading = loading };
    }

    /// <summary>
    /// Sets the click event handler for the button.
    /// This method allows you to configure the button's click behavior,
    /// enabling it to perform custom actions when users interact with it.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="onClick">The event handler to call when the button is clicked.</param>
    /// <returns>A new Button instance with the updated click handler.</returns>
    public static Button HandleClick(this Button button, Func<Event<Button>, ValueTask> onClick)
    {
        return button with { OnClick = onClick };
    }

    public static Button HandleClick(this Button button, Action<Event<Button>> onClick)
    {
        return button with { OnClick = onClick.ToValueTask() };
    }

    /// <summary>
    /// Sets a simple click event handler for the button.
    /// This method allows you to configure the button's click behavior with
    /// a simple action that doesn't require the button event context.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="onClick">The simple action to perform when the button is clicked.</param>
    /// <returns>A new Button instance with the updated click handler.</returns>
    public static Button HandleClick(this Button button, Action onClick)
    {
        return button with { OnClick = _ => { onClick(); return ValueTask.CompletedTask; } };
    }

    public static Button HandleClick(this Button button, Func<ValueTask> onClick)
    {
        return button with { OnClick = _ => onClick() };
    }

    /// <summary>
    /// Sets a custom tag object for the button.
    /// This method allows you to attach arbitrary data to the button
    /// for custom logic, state management, or integration with external
    /// systems.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="tag">The custom tag object to associate with the button.</param>
    /// <returns>A new Button instance with the updated tag.</returns>
    public static Button Tag(this Button button, object tag)
    {
        return button with { Tag = tag };
    }

    /// <summary>
    /// Sets the content for the button.
    /// This method allows you to add child content to the button,
    /// enabling complex button layouts with multiple elements.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="child">The child content to display within the button.</param>
    /// <returns>A new Button instance with the updated content.</returns>
    public static Button Content(this Button button, object child)
    {
        return button with { Children = [child] };
    }

    /// <summary>
    /// Sets the button variant to primary for maximum visual emphasis.
    /// This convenience method creates a primary button that provides
    /// prominent visual styling suitable for main actions and primary
    /// user interactions.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with primary variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Primary(this Button button)
    {
        return button.Variant(ButtonVariant.Primary);
    }

    /// <summary>
    /// Sets the button variant to secondary for reduced visual emphasis.
    /// This convenience method creates a secondary button that provides
    /// subtle visual styling suitable for supporting actions or less
    /// prominent interactions.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with secondary variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Secondary(this Button button)
    {
        return button.Variant(ButtonVariant.Secondary);
    }

    /// <summary>
    /// Sets the button variant to outline for bordered appearance.
    /// This convenience method creates an outline button that provides
    /// subtle visual styling with borders, suitable for secondary actions
    /// or alternative choices.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with outline variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Outline(this Button button)
    {
        return button.Variant(ButtonVariant.Outline);
    }

    /// <summary>
    /// Sets the button variant to destructive for warning or error styling.
    /// This convenience method creates a destructive button that provides
    /// prominent visual styling suitable for critical actions like delete
    /// or remove operations.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with destructive variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Destructive(this Button button)
    {
        return button.Variant(ButtonVariant.Destructive);
    }

    /// <summary>
    /// Sets the button variant to ghost for minimal visual styling.
    /// This convenience method creates a ghost button that provides
    /// subtle visual styling suitable for tertiary interactions or
    /// actions that should not compete with primary content.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with ghost variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Ghost(this Button button)
    {
        return button.Variant(ButtonVariant.Ghost);
    }

    /// <summary>
    /// Sets the button variant to link for clickable link appearance.
    /// This convenience method creates a link button that appears as
    /// a clickable link, suitable for navigation or external references
    /// while maintaining button interaction capabilities.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with link variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Link(this Button button)
    {
        return button.Variant(ButtonVariant.Link);
    }

    /// <summary>
    /// Sets the button variant to inline for seamless text integration.
    /// This convenience method creates an inline button that integrates
    /// seamlessly with text content, suitable for embedded actions
    /// within text blocks or paragraphs.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with inline variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Inline(this Button button)
    {
        return button.Variant(ButtonVariant.Inline);
    }

    /// <summary>
    /// Sets the border radius for the button's corners.
    /// This method allows you to control the visual styling of the button's
    /// edges, enabling you to create buttons with different levels of corner
    /// rounding to match your design requirements.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="radius">The border radius to apply to the button's corners.</param>
    /// <returns>A new Button instance with the updated border radius.</returns>
    public static Button BorderRadius(this Button button, BorderRadius radius)
    {
        return button with { BorderRadius = radius };
    }

    /// <summary>
    /// Sets the size of the button.
    /// This method allows you to control the button's dimensions and visual
    /// prominence after creation, enabling dynamic sizing based on context
    /// or design requirements.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <param name="size">The size to apply to the button.</param>
    /// <returns>A new Button instance with the updated size setting.</returns>
    public static Button Size(this Button button, Sizes size)
    {
        return button with { Size = size };
    }

    /// <summary>
    /// Sets the button size to large for prominent display.
    /// This convenience method creates a large button that provides maximum
    /// visual emphasis and is ideal for important call-to-action buttons
    /// or prominent user interactions.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with large size applied.</returns>
    [RelatedTo(nameof(Button.Size))]
    public static Button Large(this Button button)
    {
        return button.Size(Sizes.Large);
    }

    /// <summary>
    /// Sets the button size to small for compact display.
    /// This convenience method creates a small button that provides minimal
    /// visual footprint and is ideal for subtle interactions or space-constrained
    /// interfaces.
    /// </summary>
    /// <param name="button">The Button to configure.</param>
    /// <returns>A new Button instance with small size applied.</returns>
    [RelatedTo(nameof(Button.Size))]
    public static Button Small(this Button button)
    {
        return button.Size(Sizes.Small);
    }
}
