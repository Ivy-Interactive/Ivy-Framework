using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views.Blades;

public class BladesView : ViewBase
{
    /// <returns>A BladeContainer containing all active blades with their event handlers.</returns>
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
    /// <returns>A Blade widget configured with the view content and event handlers.</returns>
    public override object? Build()
    {
        return new Blade(bladeView, index, title, width, onClose, onRefresh).Key($"{index}:{refreshToken}");
    }

    /// <returns>An array containing the index and refresh token for memoization comparison.</returns>
    public object[] GetMemoValues()
    {
        return [index, refreshToken];
    }
}
