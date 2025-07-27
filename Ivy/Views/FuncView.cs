using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

public delegate object? FuncBuilder(IViewContext context);

public class FuncView(FuncBuilder viewFactory) : ViewBase
{
    public override object? Build()
    {
        return viewFactory(Context);
    }
}
