using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

public delegate object? FuncViewBuilder(IViewContext context);

public class FuncView(FuncViewBuilder viewFactory) : ViewBase
{
    public override object? Build()
    {
        return viewFactory(Context);
    }
}
public class MemoizedFuncView(FuncViewBuilder viewFactory, object[] memoValues) : ViewBase, IMemoized
{
    public override object? Build()
    {
        return viewFactory(Context);
    }

    public object[] GetMemoValues() => memoValues;
}