using Ivy.Shared;
using Ivy.Views;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class QuestionBox(string? title = null, object? content = null) : ViewBase
{
    public override object? Build()
    {
        return new Box(
            Layout.Vertical().Gap(5)
            | (title != null ? Layout.Horizontal().Align(Align.TopLeft) | Text.Label(title) : null)
            | content
        ).Padding(4).BorderThickness(1).BorderColor("#3c3c3c").Color(Colors.Card);
    }
}
