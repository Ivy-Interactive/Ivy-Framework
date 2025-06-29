using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum CalloutVariant
{
    Info,
    Warning,
    Error,
    Success
}

public record Callout : WidgetBase<Callout>
{
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

    [Prop] public CalloutVariant Variant { get; set; }

    [Prop] public Icons? Icon { get; set; }

    public static Callout Info(string? description = null, string? title = null) => new(description, title);
    public static Callout Warning(string? description = null, string? title = null) => new(description, title, CalloutVariant.Warning);
    public static Callout Error(string? description = null, string? title = null) => new(description, title, CalloutVariant.Error);
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
}