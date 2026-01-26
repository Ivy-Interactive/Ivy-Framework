using Ivy.Shared;
using Ivy.Views;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class SingleAnswerQuestion(string question, Action<bool>? onAnswer = null, string yesText = "Yes", string noText = "No") : ViewBase
{
    public override object? Build()
    {
        return new Box(
            Layout.Vertical().Gap(4)
            | (Layout.Horizontal().Align(Align.TopLeft)
                | Text.Label(question))
            | (Layout.Horizontal().Gap(2)
                | new Button(yesText, () => onAnswer?.Invoke(true)).Small().Icon(Icons.Waypoints)
                | new Button(noText, () => onAnswer?.Invoke(false)).Outline().Small())
        ).Padding(4).BorderThickness(1).BorderColor("#3c3c3c").Color(Colors.Secondary);
    }
}
