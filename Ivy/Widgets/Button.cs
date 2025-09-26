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

/// <summary>Visual style variants for button widgets controlling appearance, color scheme, and emphasis.</summary>
public enum ButtonVariant
{
    /// <summary>Primary button style with prominent emphasis for main actions.</summary>
    Primary,
    /// <summary>Destructive button style for critical actions like delete operations.</summary>
    Destructive,
    /// <summary>Outline button style with borders for secondary actions.</summary>
    Outline,
    /// <summary>Secondary button style with reduced emphasis for supporting actions.</summary>
    Secondary,
    /// <summary>Success button style for confirming actions or indicating success.</summary>
    Success,
    /// <summary>Warning button style for alerting users to potential issues.</summary>
    Warning,
    /// <summary>Info button style for providing additional context or guidance.</summary>
    Info,
    /// <summary>Ghost button style with minimal styling for subtle actions.</summary>
    Ghost,
    /// <summary>Link button style appearing as clickable link for navigation.</summary>
    Link,
    /// <summary>Inline button style integrating seamlessly with text content.</summary>
    Inline,
}

/// <summary>Interactive button widget supporting multiple variants, sizes, icons, and states for user actions and navigation.</summary>
public record Button : WidgetBase<Button>
{
    /// <summary>Initializes a Button with specified title, click handler, variant, and icon.</summary>
    /// <param name="title">Text content to display. When null, creates icon-only button.</param>
    /// <param name="onClick">Optional click event handler.</param>
    /// <param name="variant">Visual style variant. Default is <see cref="ButtonVariant.Primary"/>.</param>
    /// <param name="icon">Icon to display. Default is <see cref="Icons.None"/>.</param>
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

    /// <summary>Text content displayed on the button. When null, creates icon-only button. Default is null.</summary>
    [Prop] public string? Title { get; set; }

    /// <summary>Visual style variant controlling appearance and emphasis. Default is <see cref="ButtonVariant.Primary"/>.</summary>
    [Prop] public ButtonVariant Variant { get; set; }

    /// <summary>Icon displayed on the button. Can be positioned on either side of text. Default is <see cref="Icons.None"/>.</summary>
    [Prop] public Icons? Icon { get; set; }

    /// <summary>Position of icon relative to title text. Default is <see cref="Align.Left"/>.</summary>
    [Prop] public Align IconPosition { get; set; } = Align.Left;

    /// <summary>Foreground color for button text and icon. When null, uses variant default colors. Default is null.</summary>
    [Prop] public Colors? Foreground { get; set; }

    /// <summary>URL for navigation when button is clicked. Default is null.</summary>
    [Prop] public string? Url { get; set; }

    /// <summary>Whether the button is disabled and cannot be interacted with. Default is false.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Tooltip text displayed when hovering over the button. Default is null.</summary>
    [Prop] public string? Tooltip { get; set; }

    /// <summary>Whether the button is in a loading state with progress indicator. Default is false.</summary>
    [Prop] public bool Loading { get; set; }

    /// <summary>Border radius for button corners. Default is <see cref="BorderRadius.Rounded"/>.</summary>
    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    /// <summary>Size of the button controlling dimensions and visual prominence. Default is <see cref="Sizes.Medium"/>.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Event handler called when button is clicked. Default is null.</summary>
    [Event] public Func<Event<Button>, ValueTask>? OnClick { get; set; }

    /// <summary>Custom tag object associated with the button for arbitrary data. Default is null.</summary>
    public object? Tag { get; set; } //not a prop!

    /// <summary>Prevents adding children to Button using pipe operator.</summary>
    /// <param name="widget">The Button widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <returns>Always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Button does not support children.</exception>
    public static Button operator |(Button widget, object child)
    {
        throw new NotSupportedException("Button does not support children.");
    }
}

/// <summary>Extension methods for Button widget providing fluent API for configuring appearance, behavior, and interactions.</summary>
public static class ButtonExtensions
{
    /// <summary>Converts an icon to a button with specified click handler and variant.</summary>
    /// <param name="icon">Icon to display on the button.</param>
    /// <param name="onClick">Optional click event handler.</param>
    /// <param name="variant">Visual style variant. Default is <see cref="ButtonVariant.Primary"/>.</param>
    /// <returns>New Button instance with specified icon and settings.</returns>
    [OverloadResolutionPriority(1)]
    public static Button ToButton(this Icons icon, Func<Event<Button>, ValueTask>? onClick = null, ButtonVariant variant = ButtonVariant.Primary)
    {
        return new Button(null, onClick, icon: icon, variant: variant);
    }

    public static Button ToButton(this Icons icon, Action<Event<Button>>? onClick = null, ButtonVariant variant = ButtonVariant.Primary)
    {
        return new Button(null, onClick?.ToValueTask(), icon: icon, variant: variant);
    }

    /// <summary>Converts button into trigger controlling dynamic content display for dropdowns, modals, or expandable sections.</summary>
    /// <param name="trigger">Button that triggers content display.</param>
    /// <param name="action">Function creating content to display when triggered.</param>
    /// <returns>IView managing trigger button and conditional content display.</returns>
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

    /// <summary>Sets the title text for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="title">Text content to display on the button.</param>
    /// <returns>New Button instance with updated title.</returns>
    public static Button Title(this Button button, string title)
    {
        return button with { Title = title };
    }

    /// <summary>Sets the navigation URL for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="url">URL to navigate to when clicked.</param>
    /// <returns>New Button instance with updated URL.</returns>
    public static Button Url(this Button button, string url)
    {
        return button with { Url = url };
    }

    /// <summary>Sets the disabled state of the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="disabled">Whether button should be disabled. Default is true.</param>
    /// <returns>New Button instance with updated disabled state.</returns>
    public static Button Disabled(this Button button, bool disabled = true)
    {
        return button with { Disabled = disabled };
    }

    /// <summary>Sets the icon and its position for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="icon">Icon to display, or null to remove icon.</param>
    /// <param name="position">Icon position relative to title text. Default is <see cref="Align.Left"/>.</param>
    /// <returns>New Button instance with updated icon and position.</returns>
    public static Button Icon(this Button button, Icons? icon, Align position = Align.Left)
    {
        return button with { Icon = icon, IconPosition = position };
    }

    /// <summary>Sets the visual style variant for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="variant">Visual style variant to apply.</param>
    /// <returns>New Button instance with updated variant.</returns>
    public static Button Variant(this Button button, ButtonVariant variant)
    {
        return button with { Variant = variant };
    }

    /// <summary>Sets the foreground color for button text and icon.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="color">Color to apply to text and icon.</param>
    /// <returns>New Button instance with updated foreground color.</returns>
    public static Button Foreground(this Button button, Colors color)
    {
        return button with { Foreground = color };
    }

    /// <summary>Sets the tooltip text for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="tooltip">Tooltip text to display when hovering.</param>
    /// <returns>New Button instance with updated tooltip.</returns>
    public static Button Tooltip(this Button button, string tooltip)
    {
        return button with { Tooltip = tooltip };
    }

    /// <summary>Sets the loading state of the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="loading">Whether button should be in loading state. Default is true.</param>
    /// <returns>New Button instance with updated loading state.</returns>
    public static Button Loading(this Button button, bool loading = true)
    {
        return button with { Loading = loading };
    }

    /// <summary>Sets the click event handler for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="onClick">Event handler to call when clicked.</param>
    /// <returns>New Button instance with updated click handler.</returns>
    public static Button HandleClick(this Button button, Func<Event<Button>, ValueTask> onClick)
    {
        return button with { OnClick = onClick };
    }

    public static Button HandleClick(this Button button, Action<Event<Button>> onClick)
    {
        return button with { OnClick = onClick.ToValueTask() };
    }

    /// <summary>Sets a simple click event handler for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="onClick">Simple action to perform when clicked.</param>
    /// <returns>New Button instance with updated click handler.</returns>
    public static Button HandleClick(this Button button, Action onClick)
    {
        return button with { OnClick = _ => { onClick(); return ValueTask.CompletedTask; } };
    }

    public static Button HandleClick(this Button button, Func<ValueTask> onClick)
    {
        return button with { OnClick = _ => onClick() };
    }

    /// <summary>Sets a custom tag object for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="tag">Custom tag object to associate with button.</param>
    /// <returns>New Button instance with updated tag.</returns>
    public static Button Tag(this Button button, object tag)
    {
        return button with { Tag = tag };
    }

    /// <summary>Sets the content for the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="child">Child content to display within button.</param>
    /// <returns>New Button instance with updated content.</returns>
    public static Button Content(this Button button, object child)
    {
        return button with { Children = [child] };
    }

    /// <summary>Sets button variant to primary for maximum visual emphasis.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with primary variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Primary(this Button button)
    {
        return button.Variant(ButtonVariant.Primary);
    }

    /// <summary>Sets button variant to secondary for reduced visual emphasis.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with secondary variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Secondary(this Button button)
    {
        return button.Variant(ButtonVariant.Secondary);
    }

    /// <summary>Sets button variant to outline for bordered appearance.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with outline variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Outline(this Button button)
    {
        return button.Variant(ButtonVariant.Outline);
    }

    /// <summary>Sets button variant to destructive for warning or error styling.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with destructive variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Destructive(this Button button)
    {
        return button.Variant(ButtonVariant.Destructive);
    }

    /// <summary>Sets button variant to ghost for minimal visual styling.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with ghost variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Ghost(this Button button)
    {
        return button.Variant(ButtonVariant.Ghost);
    }

    /// <summary>Sets button variant to link for clickable link appearance.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with link variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Link(this Button button)
    {
        return button.Variant(ButtonVariant.Link);
    }

    /// <summary>Sets button variant to inline for seamless text integration.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with inline variant applied.</returns>
    [RelatedTo(nameof(Button.Variant))]
    public static Button Inline(this Button button)
    {
        return button.Variant(ButtonVariant.Inline);
    }

    /// <summary>Sets the border radius for button corners.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="radius">Border radius to apply to corners.</param>
    /// <returns>New Button instance with updated border radius.</returns>
    public static Button BorderRadius(this Button button, BorderRadius radius)
    {
        return button with { BorderRadius = radius };
    }

    /// <summary>Sets the size of the button.</summary>
    /// <param name="button">Button to configure.</param>
    /// <param name="size">Size to apply to the button.</param>
    /// <returns>New Button instance with updated size.</returns>
    public static Button Size(this Button button, Sizes size)
    {
        return button with { Size = size };
    }

    /// <summary>Sets button size to large for prominent display.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with large size applied.</returns>
    [RelatedTo(nameof(Button.Size))]
    public static Button Large(this Button button)
    {
        return button.Size(Sizes.Large);
    }

    /// <summary>Sets button size to small for compact display.</summary>
    /// <param name="button">Button to configure.</param>
    /// <returns>New Button instance with small size applied.</returns>
    [RelatedTo(nameof(Button.Size))]
    public static Button Small(this Button button)
    {
        return button.Size(Sizes.Small);
    }
}
