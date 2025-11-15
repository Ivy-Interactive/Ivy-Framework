using System.Collections.Immutable;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Blades;

public interface IBladeController
{
    IState<ImmutableArray<BladeItem>> Blades { get; }

    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="toIndex">Optional index to insert the blade at. Defaults to the end of the stack.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    void Push(IView bladeView, string? title = null, int? toIndex = null, Size? width = null);

    /// <param name="currentView">The current view to push after.</param>
    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    void Push(IView currentView, IView bladeView, string? title = null, Size? width = null);

    /// <param name="toIndex">Optional index to pop back to. Defaults to removing the last blade.</param>
    /// <param name="refresh">Whether to refresh the target blade after popping.</param>
    void Pop(int? toIndex = null, bool refresh = false);

    /// <param name="currentView">The current view to pop back to.</param>
    /// <param name="refresh">Whether to refresh the target blade after popping.</param>
    void Pop(IView currentView, bool refresh = false) => Pop(GetIndex(currentView), refresh);

    /// <returns>The zero-based index of the blade view in the stack.</returns>
    /// <param name="bladeView">The blade view to find the index for.</param>
    int GetIndex(IView bladeView);
}

public class BladeController : IBladeController
{
    public BladeController()
    {
        Blades = new State<ImmutableArray<BladeItem>>([]);
    }

    /// <param name="blades">The reactive state containing the blade items.</param>
    public BladeController(IState<ImmutableArray<BladeItem>> blades)
    {
        Blades = blades;
    }

    public IState<ImmutableArray<BladeItem>> Blades { get; }

    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="toIndex">Optional index to insert the blade at. Defaults to the end of the stack.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    public void Push(IView bladeView, string? title = null, int? toIndex = null, Size? width = null)
    {
        toIndex ??= Blades.Value.Length - 1;
        //make sure toIndex is within bounds or do nothing if it is not
        if (toIndex < 0 || toIndex >= Blades.Value.Length) return;
        var blade = new BladeItem(bladeView, toIndex.Value + 1, title, width);
        ImmutableArray<BladeItem> immutableArray = [.. Blades.Value.Take(toIndex.Value + 1).Append(blade)];
        Blades.Set(immutableArray);
    }

    /// <param name="currentView">The current view to push after.</param>
    /// <param name="bladeView">The view to display in the new blade.</param>
    /// <param name="title">Optional title for the blade header.</param>
    /// <param name="width">Optional width constraint for the blade.</param>
    public void Push(IView currentView, IView bladeView, string? title = null, Size? width = null)
    {
        var index = GetIndex(currentView);
        Push(bladeView, title, index, width);
    }

    /// <param name="toIndex">Optional index to pop back to. Defaults to removing the last blade.</param>
    /// <param name="refresh">Whether to refresh the target blade after popping.</param>
    public void Pop(int? toIndex = null, bool refresh = false)
    {
        toIndex ??= Blades.Value.Length - 2;
        //make sure toIndex is within bounds or do nothing if it is not
        if (toIndex < 0 || toIndex >= Blades.Value.Length) return;
        Blades.Set([.. Blades.Value.Take(toIndex.Value + 1)]);
        if (refresh)
        {
            Blades.Value[toIndex.Value].RefreshToken = DateTime.UtcNow.Ticks;
        }
    }

    /// <returns>The zero-based index of the blade view in the stack.</returns>
    /// <param name="bladeView">The blade view to find the index for.</param>
    public int GetIndex(IView bladeView)
    {
        return Blades.Value.First(e => e.View == bladeView).Index;
    }
}

public class BladeItem(IView view, int index, string? title, Size? width = null)
{
    public string Key { get; } = Guid.NewGuid().ToString();

    public IView View { get; set; } = view;

    public int Index { get; set; } = index;

    public long RefreshToken { get; set; } = DateTime.UtcNow.Ticks;

    public string? Title { get; set; } = title;

    public Size? Width { get; set; } = width;
}

public static class UseBladesExtensions
{
    /// <typeparam name="TView">The type of view that will use the blade system.</typeparam>
    /// <param name="view">The view that will host the blade navigation.</param>
    /// <param name="rootBlade">A factory function that creates the root blade view.</param>
    /// <param name="title">Optional title for the root blade.</param>
    /// <param name="width">Optional width constraint for the root blade.</param>
    /// <returns>A BladesView that manages the blade navigation interface.</returns>
    public static IView UseBlades<TView>(this TView view, Func<IView> rootBlade, string? title = null, Size? width = null) where TView : ViewBase =>
        view.Context.UseBlades(rootBlade, title, width);

    /// <param name="context">The view context that will host the blade navigation.</param>
    /// <param name="rootBlade">A factory function that creates the root blade view.</param>
    /// <param name="title">Optional title for the root blade.</param>
    /// <param name="width">Optional width constraint for the root blade.</param>
    /// <returns>A BladesView that manages the blade navigation interface.</returns>
    public static IView UseBlades(this IViewContext context, Func<IView> rootBlade, string? title = null, Size? width = null)
    {
        var blades = context.UseState<ImmutableArray<BladeItem>>(() => [new BladeItem(rootBlade(), 0, title, width)]);
        context.CreateContext<IBladeController>(() => new BladeController(blades));
        IView bladeView = new BladesView();
        return bladeView;
    }
}