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

    public static Callout HandleLinkClick(this Callout callout, Action<string> onLinkClick)
    {
        if (callout.Children.Length > 0 && callout.Children[0] is Markdown markdown)
        {
            var markdownWithHandler = new Markdown(markdown.Content ?? string.Empty,
                (Event<Markdown, string> @event) => { onLinkClick(@event.Value); return ValueTask.CompletedTask; });

            return callout with { Children = new object[] { markdownWithHandler } };
        }
        return callout;
    }
}