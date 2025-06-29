using System.Collections.Immutable;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Blades;

public interface IBladeController
{
    IState<ImmutableArray<BladeItem>> Blades { get; }
    void Push(IView bladeView, string? title = null, int? toIndex = null, Size? width = null);
    void Push(IView currentView, IView bladeView, string? title = null, Size? width = null);
    void Pop(int? toIndex = null, bool refresh = false);
    void Pop(IView currentView, bool refresh = false) => Pop(GetIndex(currentView), refresh);
    int GetIndex(IView bladeView);
}

public class BladeController : IBladeController
{
    internal BladeController(IState<ImmutableArray<BladeItem>> blades)
    {
        Blades = blades;
    }

    public IState<ImmutableArray<BladeItem>> Blades { get; }

    public void Push(IView bladeView, string? title = null, int? toIndex = null, Size? width = null)
    {
        toIndex ??= Blades.Value.Length - 1;
        //make sure toIndex is within bounds or do nothing if it is not
        if (toIndex < 0 || toIndex >= Blades.Value.Length) return;
        var blade = new BladeItem(bladeView, toIndex.Value + 1, title, width);
        ImmutableArray<BladeItem> immutableArray = [.. Blades.Value.Take(toIndex.Value + 1).Append(blade)];
        Blades.Set(immutableArray);
    }

    public void Push(IView currentView, IView bladeView, string? title = null, Size? width = null)
    {
        var index = GetIndex(currentView);
        Push(bladeView, title, index, width);
    }

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
    public static IView UseBlades<TView>(this TView view, Func<IView> rootBlade, string? title = null) where TView : ViewBase =>
        view.Context.UseBlades(rootBlade, title);

    public static IView UseBlades(this IViewContext context, Func<IView> rootBlade, string? title = null, Size? width = null)
    {
        var blades = context.UseState<ImmutableArray<BladeItem>>(() => [new BladeItem(rootBlade(), 0, title, width)]);
        context.CreateContext<IBladeController>(() => new BladeController(blades));
        IView bladeView = new BladesView();
        return bladeView;
    }
}