using Ivy.Apps;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views;

/// <summary>
/// Displays an exception using the same layout as ErrorApp: callout + "View details" with full error in sheet.
/// Used when Build() throws, connection fails, or content is an Exception.
/// </summary>
public class ErrorView(Exception e) : ViewBase, IStateless
{
    public override object? Build()
    {
        e = e.UnwrapAggregate();
        var title = e.GetType().Name;
        var message = e.Message ?? "An error occurred.";
        var fullError = e.ToString();

        try
        {
            return ErrorDisplay.Build(
                title,
                message,
                () => new Error("Full error", fullError, null),
                CalloutVariant.Error,
                Icons.CircleAlert
            );
        }
        catch
        {
            return new Error(title, message, e.StackTrace);
        }
    }
}
