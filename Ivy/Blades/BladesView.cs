using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Blades;

public class BladesView : ViewBase
{
    public override object? Build()
    {
        var controller = UseContext<IBladeController>();

        var blades = controller.Blades.Value
            .Select(e => new BladeView(
                e.View,
                e.Index,
                e.RefreshToken,
                e.Title,
                e.Width,
                onClose: _ =>
                {
                    controller.Pop(e.Index - 1);
                },
                onRefresh: _ =>
                {
                    controller.Pop(e.Index, true);
                }).Key(e.Key)
            )
            .ToArray();
        return new BladeContainer(blades);
    }
}

public class BladeView(IView bladeView, int index, long refreshToken, string? title, Size? width, Action<Event<Blade>>? onClose, Action<Event<Blade>>? onRefresh) : ViewBase, IMemoized
{
    public override object? Build()
    {
        return new Blade(bladeView, index, title, width, onClose, onRefresh).Key($"{index}:{refreshToken}");
    }

    public object[] GetMemoValues()
    {
        return [index, refreshToken];
    }
}
