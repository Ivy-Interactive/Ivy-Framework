using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views.Blades;

/// <summary>
/// A view that renders the blade navigation interface by managing blade items from the blade controller.
/// </summary>
/// <remarks>
/// This view is typically created automatically by the UseBlades extension and manages the rendering
/// of all blades in the navigation stack. It connects to the IBladeController to get blade state
/// and handles close/refresh events for each blade in the stack.
/// </remarks>
public class BladesView : ViewBase
{
    /// <summary>
    /// Builds the blade navigation interface by rendering all blades from the controller.
    /// </summary>
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

/// <summary>
/// A memoized wrapper for individual blade instances that optimizes rendering performance.
/// </summary>
/// <remarks>
/// This view wraps each blade with memoization based on index and refresh token,
/// preventing unnecessary re-renders when blade content hasn't changed.
/// </remarks>
public class BladeView(IView bladeView, int index, long refreshToken, string? title, Size? width, Action<Event<Blade>>? onClose, Action<Event<Blade>>? onRefresh) : ViewBase, IMemoized
{
    /// <summary>
    /// Builds the individual blade with the specified properties and event handlers.
    /// </summary>
    /// <returns>A Blade widget configured with the view content and event handlers.</returns>
    public override object? Build()
    {
        return new Blade(bladeView, index, title, width, onClose, onRefresh).Key($"{index}:{refreshToken}");
    }

    /// <summary>
    /// Gets the values used for memoization to determine when the blade should re-render.
    /// </summary>
    /// <returns>An array containing the index and refresh token for memoization comparison.</returns>
    public object[] GetMemoValues()
    {
        return [index, refreshToken];
    }
}
