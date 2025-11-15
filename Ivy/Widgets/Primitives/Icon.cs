using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Scalable vector icon widget with theming support.</summary>
public record Icon : WidgetBase<Icon>
{
    /// <param name="name">Icon to display.</param>
    /// <param name="color">Optional color override. Default: theme color.</param>
    public Icon(Icons name, Colors? color = null)
    {
        Name = name;
        Color = color;
    }

    [Prop] public Icons Name { get; set; }

    [Prop] public Colors? Color { get; set; }
}

public static class IconExtensions
{
    public static Icon ToIcon(this Icons icon)
    {
        return new Icon(icon);
    }

    public static Icon Color(this Icon icon, Colors? color = null)
    {
        return icon with { Color = color };
    }

    public static Icon Small(this Icon icon)
    {
        return icon with { Width = Size.Units(4), Height = Size.Units(4) };
    }

    public static Icon Large(this Icon icon)
    {
        return icon with { Width = Size.Units(12), Height = Size.Units(12) };
    }
}