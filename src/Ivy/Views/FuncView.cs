using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

public delegate object? FuncViewBuilder(IViewContext context);

/// <summary>
/// A view constructed from a delegate function.
/// Allows defining view logic inline without creating a dedicated class, useful for simple or higher-order components.
/// </summary>
public class FuncView(FuncViewBuilder viewFactory) : ViewBase
{
    public override object? Build()
    {
        return viewFactory(Context);
    }
}
/// <summary>
/// A memoized version of `FuncView` that only rebuilds when specific dependency values change.
/// Optimizes performance for functional views by preventing unnecessary re-renders.
/// </summary>
public class MemoizedFuncView(FuncViewBuilder viewFactory, object[] memoValues) : ViewBase, IMemoized
{
    public override object? Build()
    {
        return viewFactory(Context);
    }

    public object[] GetMemoValues() => memoValues;
}