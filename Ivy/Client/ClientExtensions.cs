using System;
using System.Threading.Tasks;
using Ivy.Auth;
using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Client;

public class ToasterMessage
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class ErrorMessage
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? StackTrace { get; set; }
}

public static class ClientExtensions
{
    public static void CopyToClipboard(this IClientProvider client, string content)
    {
        client.Sender.Send("CopyToClipboard", content);
    }

    public static void OpenUrl(this IClientProvider client, string url)
    {
        client.Sender.Send("OpenUrl", url);
    }
    public static void OpenUrl(this IClientProvider client, Uri uri)
    {
        client.Sender.Send("OpenUrl", uri.ToString());
    }

    public static Task<AuthResult> SignInToFirebaseAsync(
        this IClientProvider client,
        string apiKey,
        string authDomain,
        string projectId,
        AuthOption? authOption = null,
        string requestId = "")
    {
        // Generate a unique request ID if none provided
        if (string.IsNullOrEmpty(requestId))
        {
            requestId = Guid.NewGuid().ToString();
        }

        // Set up the completion source that will be completed when we get the response
        var taskCompletionSource = new TaskCompletionSource<AuthResult>();

        // Register the completion source so it can be completed when we get the response
        AuthResponses.RegisterResponse(requestId, taskCompletionSource);

        // Convert AuthOption to a serializable object for passing to JS
        var authOptionData = authOption == null ? null : new
        {
            flow = authOption.Flow.ToString(),
            name = authOption.Name,
            id = authOption.Id,
            icon = authOption.Icon?.ToString(),
            tag = authOption.Tag?.ToString()
        };

        // Send the request with the requestId, Firebase configuration, and auth option
        client.Sender.Send("SignInToFirebase", new
        {
            requestId,
            config = new
            {
                apiKey,
                authDomain,
                projectId
            },
            authOption = authOptionData
        });

        // Return the task that will complete when the response is received
        return taskCompletionSource.Task;
    }

    // Version that accepts auth option but uses default configuration
    public static Task<AuthResult> SignInToFirebaseAsync(
        this IClientProvider client,
        AuthOption authOption,
        string requestId = "")
    {
        return SignInToFirebaseAsync(
            client,
            Environment.GetEnvironmentVariable("FIREBASE_API_KEY") ?? "",
            Environment.GetEnvironmentVariable("FIREBASE_AUTH_DOMAIN") ?? "",
            Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID") ?? "",
            authOption,
            requestId);
    }

    // Simplified version that uses default configuration and Google provider
    public static Task<AuthResult> SignInToFirebaseAsync(this IClientProvider client, string requestId = "")
    {
        return SignInToFirebaseAsync(
            client,
            Environment.GetEnvironmentVariable("FIREBASE_API_KEY") ?? "",
            Environment.GetEnvironmentVariable("FIREBASE_AUTH_DOMAIN") ?? "",
            Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID") ?? "",
            null,
            requestId);
    }

    public static void Redirect(this IClientProvider client, string url)
    {
        client.Sender.Send("Redirect", url);
    }

    public static void SetJwt(this IClientProvider client, AuthToken? authToken)
    {
        client.Sender.Send("SetJwt", authToken);
    }

    public static void SetTheme(this IClientProvider client, Theme theme)
    {
        client.Sender.Send("SetTheme", theme.ToString());
    }

    public static void Toast(this IClientProvider client, string description, string? title = null)
    {
        client.Sender.Send("Toast", new ToasterMessage { Description = description, Title = title });
    }

    public static void Toast(this IClientProvider client, Exception ex)
    {
        var innerException = Utils.GetInnerMostException(ex);
        client.Sender.Send("Toast", new ToasterMessage { Description = innerException.Message, Title = "Failed" });
    }

    public static void Error(this IClientProvider client, Exception ex)
    {
        var innerException = Utils.GetInnerMostException(ex);
        var notification = new ErrorMessage
        {
            Description = innerException.Message,
            Title = innerException.GetType().Name,
            StackTrace = innerException.StackTrace
        };
        client.Sender.Send("Error", notification);
    }
}