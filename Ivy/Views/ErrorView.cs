using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

public class ErrorView(System.Exception e) : ViewBase, IStateless
{
    public override object? Build()
    {
        return new Error(e.GetType().Name, e.Message, e.StackTrace);
    }
}