using System.Text.Json;
using Ivy.Core;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Arguments for the generic error display app. Passed when showing a friendly error page for any framework or app error.
/// Use the static factory methods and <see cref="ToArgsJson"/> to build args from anywhere (e.g. router, auth, middleware).
/// </summary>
/// <param name="Title">Heading shown on the error page (e.g. "Ouch! :|").</param>
/// <param name="Message">Explanation shown below the title.</param>
/// <param name="Kind">Optional error kind for styling.</param>
/// <param name="Details">Optional full error text (e.g. server response, stack trace). When set, "View details" sheet shows this content.</param>
public record ErrorAppArgs(string Title, string Message, string? Kind = null, string? Details = null)
{
    public const string KindNotFound = "NotFound";
    public const string KindNoApps = "NoApps";
    public const string KindServerError = "ServerError";
    public const string KindUnauthorized = "Unauthorized";
    public const string KindForbidden = "Forbidden";

    public static ErrorAppArgs ForNotFound(string? title = null, string? message = null) =>
        new(title ?? ErrorApp.DefaultTitle, message ?? ErrorApp.DefaultNotFoundMessage, KindNotFound);

    public static ErrorAppArgs ForNoApps(string? title = null, string? message = null) =>
        new(title ?? ErrorApp.DefaultTitle, message ?? ErrorApp.DefaultNoAppsMessage, KindNoApps);

    public static ErrorAppArgs ForServerError(string message, string? title = null, string? details = null) =>
        new(title ?? ErrorApp.DefaultTitle, message, KindServerError, details);

    public static ErrorAppArgs ForUnauthorized(string? title = null, string? message = null, string? details = null) =>
        new(title ?? "Unauthorized", message ?? "Please sign in to continue.", KindUnauthorized, details);

    public static ErrorAppArgs Custom(string title, string message, string? kind = null, string? details = null) =>
        new(title, message, kind, details);

    public static string ToArgsJson(ErrorAppArgs args) =>
        JsonSerializer.Serialize(args, JsonHelper.DefaultOptions);
}

/// <summary>
/// Shared error page layout: callout + "View details" button that opens a sheet with error content (e.g. full server response).
/// </summary>
internal static class ErrorDisplay
{
    public const string SheetTitle = "Error details";
    public const string SheetDescription = "Full error as returned by the server";

    /// <param name="sheetContent">When null, no "View details" button is shown (e.g. for NotFound, NoApps, or ServerError without details).</param>
    public static object Build(string title, string message, Func<object>? sheetContent = null, CalloutVariant variant = CalloutVariant.Warning, Icons? icon = null, string? exceptionTypeName = null)
    {
        var callout = CalloutForVariant(variant, title, message, icon);

        var vertical = Layout.Vertical().Center();
        vertical |= callout;

        // Add contextual hint if available for this exception type
        var hint = exceptionTypeName != null ? ExceptionHints.GetHint(exceptionTypeName) : null;
        if (hint != null)
        {
            vertical |= Callout.Info(hint.Description, hint.Title);
        }

        if (sheetContent != null)
        {
            vertical |= new Button("View details")
                .Variant(ButtonVariant.Outline)
                .Icon(Icons.FileCode)
                .WithSheet(sheetContent, title: SheetTitle, description: SheetDescription);
        }

        return Layout.Center() | vertical;
    }

    public static object SheetContentForServerResponse(string title, string message, string? details)
    {
        if (!string.IsNullOrWhiteSpace(details))
            return new Error("Full server response", details, null);
        return new Error("Error details", "No full server response was provided.", $"{title}\n\n{message}");
    }

    public static (CalloutVariant variant, Icons? icon) VariantAndIconForKind(string? kind)
    {
        return kind switch
        {
            ErrorAppArgs.KindNotFound => (CalloutVariant.Warning, Icons.SearchX),
            ErrorAppArgs.KindNoApps => (CalloutVariant.Warning, Icons.LayoutList),
            ErrorAppArgs.KindServerError => (CalloutVariant.Error, Icons.CircleAlert),
            ErrorAppArgs.KindUnauthorized => (CalloutVariant.Warning, Icons.LogIn),
            ErrorAppArgs.KindForbidden => (CalloutVariant.Error, Icons.ShieldAlert),
            _ => (CalloutVariant.Warning, Icons.CircleAlert)
        };
    }

    private static Callout CalloutForVariant(CalloutVariant variant, string title, string message, Icons? icon)
    {
        var callout = variant switch
        {
            CalloutVariant.Error => Callout.Error(message, title),
            CalloutVariant.Success => Callout.Success(message, title),
            CalloutVariant.Warning => Callout.Warning(message, title),
            _ => Callout.Info(message, title)
        };
        return icon != null ? callout.Icon(icon.Value) : callout;
    }
}

/// <summary>
/// Generic error display app. Shows a configurable title and message via UseArgs&lt;ErrorAppArgs&gt;().
/// Supports optional <see cref="ErrorAppArgs.Kind"/> for different visuals. Registered via <see cref="Server.UseErrorNotFound{T}"/>.
/// </summary>
public class ErrorApp : ViewBase
{
    public const string DefaultTitle = "Ouch! :|";
    public const string DefaultNotFoundMessage = "Apologies, the app you were looking for was not found.";
    public const string DefaultNoAppsMessage = "No apps are registered on this server.";
    public const string DefaultServerErrorMessage = "Something went wrong. Please try again later.";

    public override object? Build()
    {
        var args = UseArgs<ErrorAppArgs>();
        var title = args?.Title ?? DefaultTitle;
        var message = args?.Message ?? DefaultNotFoundMessage;
        var (variant, icon) = ErrorDisplay.VariantAndIconForKind(args?.Kind);

        return ErrorDisplay.Build(
            title,
            message,
            sheetContent: !string.IsNullOrWhiteSpace(args?.Details) ? () => ErrorDisplay.SheetContentForServerResponse(title, message, args?.Details) : null,
            variant,
            icon,
            exceptionTypeName: args?.Kind == ErrorAppArgs.KindServerError ? title : null
        );
    }
}

/// <summary>
/// Displays an exception using the same layout as <see cref="ErrorApp"/>: callout + "View details" with full error in sheet.
/// Used when Build() throws, connection fails, or content is an Exception. Keeps <see cref="ErrorView"/> unchanged.
/// </summary>
public class ExceptionErrorView(Exception e) : ViewBase, IStateless
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
                Icons.CircleAlert,
                exceptionTypeName: title
            );
        }
        catch
        {
            return new Error(title, message, e.StackTrace);
        }
    }
}
