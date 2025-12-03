using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

public class ErrorView(Exception e) : ViewBase, IStateless
{
    public override object? Build()
    {
        e = e.UnwrapAggregate();

        return new Error(e.GetType().Name, e.Message, e.StackTrace);
    }
}