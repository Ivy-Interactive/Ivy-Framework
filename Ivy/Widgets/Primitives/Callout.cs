using Ivy.Core;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Callout visual variants.</summary>
public enum CalloutVariant
{
    /// <summary>General information.</summary>
    Info,
    /// <summary>Cautionary information.</summary>
    Warning,
    /// <summary>Critical issues/errors.</summary>
    Error,
    /// <summary>Success/confirmations.</summary>
    Success
}

/// <summary>Prominent message widget for info, warnings, errors, and success notifications. Strings auto-convert to Markdown.</summary>
public record Callout : WidgetBase<Callout>
{
    /// <summary>Initializes callout.</summary>
    /// <param name="description">Content (string converts to Markdown).</param>
    /// <param name="title">Optional title for the callout.</param>
    /// <param name="variant">Visual variant. Default: Info.</param>
    /// <param name="icon">Optional icon to display.</param>
    public Callout(object? description = null, string? title = null, CalloutVariant variant = CalloutVariant.Info, Icons? icon = null)
    {
        var child = description switch
        {
            string str => new Markdown(str),
            _ => description
        };

        if (child != null)
            Children = [child!];

        Title = title;
        Variant = variant;
        Icon = icon;
    }

    [Prop] public string? Title { get; set; }

    /// <summary>Callout variant (Info, Warning, Error, Success).</summary>
    [Prop] public CalloutVariant Variant { get; set; }

    [Prop] public Icons? Icon { get; set; }

    /// <summary>Event handler called when links in markdown content are clicked.</summary>
    [Event] public Func<Event<Callout, string>, ValueTask>? OnLinkClick { get; set; }

    /// <summary>Creates Info callout.</summary>
    public static Callout Info(string? description = null, string? title = null) => new(description, title);

    /// <summary>Creates Warning callout.</summary>
    public static Callout Warning(string? description = null, string? title = null) => new(description, title, CalloutVariant.Warning);

    /// <summary>Creates Error callout.</summary>
    public static Callout Error(string? description = null, string? title = null) => new(description, title, CalloutVariant.Error);

    /// <summary>Creates Success callout.</summary>
    public static Callout Success(string? description = null, string? title = null) => new(description, title, CalloutVariant.Success);
}

public static class CalloutExtensions
{
    public static Callout Title(this Callout callout, string title)
    {
        return callout with { Title = title };
    }

    public static Callout Description(this Callout callout, string description)
    {
        return callout with { Children = [new Markdown(description)] };
    }

    public static Callout Variant(this Callout callout, CalloutVariant variant)
    {
        return callout with { Variant = variant };
    }

    public static Callout Icon(this Callout callout, Icons icon)
    {
        return callout with { Icon = icon };
    }

    /// <summary>Sets link click event handler for Callout widget.</summary>
    /// <param name="callout">Callout widget to configure.</param>
    /// <param name="onLinkClick">Event handler receiving full event context.</param>
    /// <returns>Callout widget with specified link click handler.</returns>
    [OverloadResolutionPriority(1)]
    public static Callout HandleLinkClick(this Callout callout, Func<Event<Callout, string>, ValueTask> onLinkClick)
    {
        var updatedCallout = callout with { OnLinkClick = onLinkClick };

        // If the first child is a Markdown widget, also set its link click handler
        if (updatedCallout.Children.Length > 0 && updatedCallout.Children[0] is Markdown markdown)
        {
            // Ensure Content is preserved - use Content property or fallback to empty string
            var content = markdown.Content ?? string.Empty;

            // Create a new Markdown widget with the link click handler that forwards to the Callout's handler
            var markdownWithHandler = new Markdown(content, (Event<Markdown, string> @event) =>
                onLinkClick(new Event<Callout, string>(@event.EventName, updatedCallout, @event.Value)));

            updatedCallout = updatedCallout with
            {
                Children = new object[] { markdownWithHandler }
            };
        }

        return updatedCallout;
    }

    // Overload for Action<Event<Callout, string>>
    public static Callout HandleLinkClick(this Callout callout, Action<Event<Callout, string>> onLinkClick)
    {
        return callout.HandleLinkClick(onLinkClick.ToValueTask());
    }

    /// <summary>Sets link click event handler for Callout widget with simplified callback.</summary>
    /// <param name="callout">Callout widget to configure.</param>
    /// <param name="onLinkClick">Simplified event handler receiving only clicked link URL.</param>
    /// <returns>Callout widget with specified link click handler.</returns>
    public static Callout HandleLinkClick(this Callout callout, Action<string> onLinkClick)
    {
        return callout.HandleLinkClick(@event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; });
    }
}