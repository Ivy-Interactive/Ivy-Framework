using Ivy.Core;
using Ivy.Shared;

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
    /// <param name="title">Optional title.</param>
    /// <param name="variant">Visual variant. Default: Info.</param>
    /// <param name="icon">Optional icon.</param>
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

    /// <summary>Callout title.</summary>
    [Prop] public string? Title { get; set; }

    /// <summary>Callout variant (Info, Warning, Error, Success).</summary>
    [Prop] public CalloutVariant Variant { get; set; }

    /// <summary>Optional icon.</summary>
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

/// <summary>Extension methods for Callout widget configuration.</summary>
public static class CalloutExtensions
{
    /// <summary>Sets callout title.</summary>
    public static Callout Title(this Callout callout, string title)
    {
        return callout with { Title = title };
    }

    /// <summary>Sets description (converts to Markdown).</summary>
    public static Callout Description(this Callout callout, string description)
    {
        return callout with { Children = [new Markdown(description)] };
    }

    /// <summary>Sets callout variant.</summary>
    public static Callout Variant(this Callout callout, CalloutVariant variant)
    {
        return callout with { Variant = variant };
    }

    /// <summary>Sets callout icon.</summary>
    public static Callout Icon(this Callout callout, Icons icon)
    {
        return callout with { Icon = icon };
    }
}