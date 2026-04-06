// ReSharper disable once CheckNamespace
namespace Ivy;

public record IvyLogo : WidgetBase<IvyLogo>
{
    public IvyLogo(Colors color = Colors.IvyGreen) : this()
    {
        Color = color;
    }

    internal IvyLogo()
    {
        Width = Size.Units(25);
        Height = Size.Auto();
    }

    [Prop] public Colors Color { get; set; } = Colors.IvyGreen;
}