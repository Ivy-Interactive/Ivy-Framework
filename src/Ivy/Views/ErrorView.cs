// ReSharper disable once CheckNamespace
namespace Ivy;

public class ErrorView(Exception e) : ViewBase, IStateless
{
    public override object? Build()
    {
        e = e.UnwrapAggregate();

        return new Error(e.GetType().Name, e.Message, e.StackTrace);
    }
}
