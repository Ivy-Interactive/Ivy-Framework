
// ReSharper disable once CheckNamespace
namespace Ivy;

public class FloatingBox(object? content = null) : ViewBase
{
    public override object? Build()
    {
        return new Box(content ?? Array.Empty<object>())
            .Padding(2)
            .BorderThickness(1)
            .Background(Colors.Neutral, 0.25f)
            .BorderColor(Colors.Neutral, 0.5f);
    }
}
