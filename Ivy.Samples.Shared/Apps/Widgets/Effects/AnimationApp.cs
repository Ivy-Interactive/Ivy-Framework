using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Effects;

[App(icon: Icons.Play, searchHints: ["motion", "transition", "effects", "animated", "movement", "visual"])]
public class AnimationApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | CreateHeader()
               | CreateAnimationType()
            ;
    }

    private object CreateHeader()
    {
        return Layout.Vertical()
               | Text.H1("Animations")
            ;
    }

    private object CreateAnimationType()
    {
        object Create(AnimationType a)
        {
            return (Layout.Vertical().Width(50)
                    | Text.InlineCode($"AnimationType.{a.ToString()}")
                    | Icons.Star.ToIcon().Color(Colors.Primary).Large().WithAnimation(a).Duration(5));
        }

        return Layout.Vertical()
               | Text.H2("Animation Types")
               | (Layout.Wrap()
                  | Enum.GetValues<AnimationType>().Select(Create)
               );
    }
}