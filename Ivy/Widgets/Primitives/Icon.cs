using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Scalable vector icon widget with theming support.</summary>
public record Icon : WidgetBase<Icon>
{
    /// <summary>Initializes icon widget.</summary>
    /// <param name="name">Icon to display.</param>
    /// <param name="color">Optional color override. Default: theme color.</param>
    public Icon(Icons name, Colors? color = null)
    {
        Name = name;
        Color = color;
    }

    /// <summary>Icon to display.</summary>
    [Prop] public Icons Name { get; set; }

    /// <summary>Color override. Default: theme color.</summary>
    [Prop] public Colors? Color { get; set; }
}

/// <summary>Extension methods for Icon widget configuration.</summary>
public static class IconExtensions
{
    /// <summary>Converts Icons enum to Icon widget.</summary>
    public static Icon ToIcon(this Icons icon)
    {
        return new Icon(icon);
    }

    /// <summary>Sets icon color.</summary>
    public static Icon Color(this Icon icon, Colors? color = null)
    {
        return icon with { Color = color };
    }

    /// <summary>Sets small size (4x4 units).</summary>
    public static Icon Small(this Icon icon)
    {
        return icon with { Width = Size.Units(4), Height = Size.Units(4) };
    }

    /// <summary>Sets large size (12x12 units).</summary>
    public static Icon Large(this Icon icon)
    {
        return icon with { Width = Size.Units(12), Height = Size.Units(12) };
    }
}