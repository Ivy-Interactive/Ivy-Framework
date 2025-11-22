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
