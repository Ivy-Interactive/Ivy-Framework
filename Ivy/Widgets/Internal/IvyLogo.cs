using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// This widget renders the Ivy logo with customizable colors and consistent sizing.
/// </summary>
public record IvyLogo : WidgetBase<IvyLogo>
{
    /// <summary>
    /// Initializes a new instance of the IvyLogo class with the specified color.
    /// </summary>
    /// <param name="color">The color to use for the Ivy logo.</param>
    public IvyLogo(Colors color = Colors.Primary) : base([])
    {
        Color = color;
        Width = Size.Units(25);
        Height = Size.Auto();
    }

    /// <summary>
    /// Gets or sets the color used to render the Ivy logo.
    /// The logo color can be set to any predefined color from the <see cref="Colors"/> enum.
    /// </summary>
    [Prop] public Colors Color { get; set; } = Colors.Primary;
}