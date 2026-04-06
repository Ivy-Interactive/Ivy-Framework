// ReSharper disable once CheckNamespace
namespace Ivy;

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
