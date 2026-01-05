using Ivy.Auth.Clerk.ApiClient.Responses;
using Ivy.Auth.Clerk.ApiClient.Models;

namespace Ivy.Auth.Clerk.ApiClient;

public class ClerkException : Exception
{
    public List<ClerkError> Errors { get; set; } = new();

    public string ClerkTraceId { get; set; } = string.Empty;

    public ClerkException() { }

    public ClerkException(string message) : base(message) { }

    public ClerkException(string message, Exception? innerException)
        : base(message, innerException) { }

    public ClerkException(ClerkErrorResponse response)
        : base(BuildMessage(response))
    {
        Errors = response.Errors;
        ClerkTraceId = response.ClerkTraceId;
    }

    private static string BuildMessage(ClerkErrorResponse response)
    {
        if (response.Errors.Count == 0)
            return $"Clerk returned an unknown error (trace: {response.ClerkTraceId})";

        var err = response.Errors[0];
        return $"{err.Code}: {err.Message}\n{err.LongMessage}\nTrace ID: {response.ClerkTraceId}";
    }
}
