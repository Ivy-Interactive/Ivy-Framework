using Ivy.Shared;
using Ivy.Views;

namespace Ivy;

public class FloatingBox(object? content = null) : ViewBase
{
    public override object? Build()
    {
        return new Box(content ?? Array.Empty<object>())
            .Padding(2)
            .BorderThickness(1)
            .Color(Colors.Neutral, 0.25f)
            .BorderColor(Colors.Neutral, 0.5f);
    }
}
