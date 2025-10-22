namespace Ivy.Helpers;

/// <summary>
/// Helper class for adding timeout functionality to async operations.
/// </summary>
public static class TimeoutHelper
{
    /// <summary>
    /// Default timeout duration for auth operations.
    /// </summary>
    private const int DefaultAuthTimeoutSeconds = 30;

    /// <summary>
    /// Executes an async operation with a timeout, optionally linking with a parent cancellation token.
    /// </summary>
    /// <typeparam name="T">The return type of the operation</typeparam>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="cancellationToken">Optional parent cancellation token to link with the timeout</param>
    /// <param name="timeoutSeconds">Timeout duration in seconds (defaults to 30 seconds)</param>
    /// <returns>The result of the operation</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation times out or is cancelled</exception>
    public static async Task<T> WithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default,
        int timeoutSeconds = DefaultAuthTimeoutSeconds)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        return await operation(linkedCts.Token);
    }

    /// <summary>
    /// Executes an async operation with a timeout, optionally linking with a parent cancellation token.
    /// </summary>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="cancellationToken">Optional parent cancellation token to link with the timeout</param>
    /// <param name="timeoutSeconds">Timeout duration in seconds (defaults to 30 seconds)</param>
    /// <exception cref="OperationCanceledException">Thrown when the operation times out or is cancelled</exception>
    public static async Task WithTimeoutAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default,
        int timeoutSeconds = DefaultAuthTimeoutSeconds)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        await operation(linkedCts.Token);
    }
}
