namespace Ivy.Helpers;

public static class TimeoutHelper
{
    private const int DefaultAuthTimeoutSeconds = 30;

    public static async Task<T> WithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default,
        int timeoutSeconds = DefaultAuthTimeoutSeconds)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        return await operation(linkedCts.Token);
    }

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
