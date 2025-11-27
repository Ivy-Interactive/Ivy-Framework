using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ivy.Middleware;

public class IvyErrorFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Only handle requests expecting HTML
        var acceptHeader = context.HttpContext.Request.Headers.Accept;
        var acceptsHtml = Microsoft.Net.Http.Headers.MediaTypeHeaderValue.ParseList(acceptHeader)
            .Any(h => h.IsSubsetOf(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/html")));

        if (!acceptsHtml)
        {
            return;
        }

        string? title = null;
        string? message = null;
        int? statusCode = null;

        // Check for ObjectResult with error status codes (4xx, 5xx)
        if (context.Result is ObjectResult objectResult && objectResult.StatusCode >= 400)
        {
            title = $"Error {objectResult.StatusCode}";
            message = objectResult.Value?.ToString() ?? "An error occurred";
            statusCode = objectResult.StatusCode;
        }
        // Check for StatusCodeResult with error status codes (4xx, 5xx)
        else if (context.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode >= 400)
        {
            title = $"Error {statusCodeResult.StatusCode}";
            message = $"An error occurred (Status Code: {statusCodeResult.StatusCode})";
            statusCode = statusCodeResult.StatusCode;
        }

        if (title != null && message != null && statusCode != null)
        {
            // Get index.html with error meta tags injected (sync call is acceptable here as filter needs to complete)
            var html = WebApplicationExtensions.GetIndexHtmlWithError(title, message).GetAwaiter().GetResult();

            context.Result = new ContentResult
            {
                Content = html,
                ContentType = "text/html",
                StatusCode = statusCode
            };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
