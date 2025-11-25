using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record IvyLogo : WidgetBase<IvyLogo>
{
    public IvyLogo(Colors color = Colors.Primary) : base([])
    {
        Color = color;
        Width = Size.Units(25);
        Height = Size.Auto();
    }

    [Prop] public Colors Color { get; set; } = Colors.Primary;
}