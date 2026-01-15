using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

/// <summary>
/// A dedicated view for displaying detailed exception information, including message and stack trace.
/// Wraps the `Error` primitive to provide a full-page or modal error presentation.
/// </summary>
public class ErrorView(Exception e) : ViewBase, IStateless
{
    public override object? Build()
    {
        e = e.UnwrapAggregate();

        return new Error(e.GetType().Name, e.Message, e.StackTrace);
    }
}