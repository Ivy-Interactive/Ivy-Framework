using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ivy.Auth;

/// <summary>
/// Represents the result of a Firebase authentication operation.
/// </summary>
public record FirebaseAuthResult
{
    /// <summary>
    /// Gets or sets whether the authentication was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the Firebase ID token.
    /// </summary>
    public string? IdToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token for obtaining new ID tokens.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the user's unique ID.
    /// </summary>
    public string? Uid { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the photo URL of the user.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Gets or sets the error message if authentication failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the error code if authentication failed.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the expiry time of the ID token.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}

/// <summary>
/// Static class to manage Firebase authentication responses.
/// </summary>
public static class FirebaseAuthResponses
{
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<FirebaseAuthResult>> _pendingResponses = new();

    /// <summary>
    /// Registers a TaskCompletionSource for a Firebase authentication request.
    /// </summary>
    /// <param name="requestId">The unique ID of the request.</param>
    /// <param name="taskCompletionSource">The TaskCompletionSource to complete when a response is received.</param>
    public static void RegisterResponse(string requestId, TaskCompletionSource<FirebaseAuthResult> taskCompletionSource)
    {
        _pendingResponses.TryAdd(requestId, taskCompletionSource);

        // Add automatic cleanup after a timeout period
        Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ =>
        {
            if (_pendingResponses.TryRemove(requestId, out var tcs))
            {
                tcs.TrySetCanceled();
            }
        });
    }

    /// <summary>
    /// Processes a Firebase authentication response received from the client.
    /// </summary>
    /// <param name="requestId">The unique ID of the request.</param>
    /// <param name="result">The authentication result from Firebase.</param>
    public static void ProcessResponse(string requestId, FirebaseAuthResult result)
    {
        if (_pendingResponses.TryRemove(requestId, out var taskCompletionSource))
        {
            taskCompletionSource.TrySetResult(result);
        }
    }

    /// <summary>
    /// Processes a Firebase authentication error received from the client.
    /// </summary>
    /// <param name="requestId">The unique ID of the request.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    public static void ProcessError(string requestId, string errorMessage, string? errorCode = null)
    {
        if (_pendingResponses.TryRemove(requestId, out var taskCompletionSource))
        {
            var result = new FirebaseAuthResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };

            taskCompletionSource.TrySetResult(result);
        }
    }
}