using System;
using System.Collections.Concurrent;

namespace Ivy.Auth;

public record AuthenticationResult
{
    public bool Success { get; set; }

    public AuthToken? Token { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorCode { get; set; }
}

public static class AuthResponses
{
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<AuthenticationResult>> _pendingResponses = new();

    public static void RegisterResponse(string requestId, TaskCompletionSource<AuthenticationResult> taskCompletionSource)
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

    public static void ProcessResponse(string requestId, AuthenticationResult result)
    {
        if (_pendingResponses.TryRemove(requestId, out var taskCompletionSource))
        {
            taskCompletionSource.TrySetResult(result);
        }
    }

    public static void ProcessError(string requestId, string errorMessage, string? errorCode = null)
    {
        if (_pendingResponses.TryRemove(requestId, out var taskCompletionSource))
        {
            var result = new AuthenticationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };

            taskCompletionSource.TrySetResult(result);
        }
    }
}