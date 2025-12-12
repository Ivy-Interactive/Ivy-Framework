namespace Ivy.Core.HttpTunneling;

public class PendingHttpRequest : IDisposable
{
    public TaskCompletionSource<HttpResponseMessage> CompletionSource { get; }
    public CancellationTokenSource TimeoutCts { get; }
    public CancellationTokenRegistration CancellationRegistration { get; set; }

    private readonly CancellationTokenSource _linkedCts;

    public PendingHttpRequest(CancellationToken cancellationToken)
    {
        CompletionSource = new TaskCompletionSource<HttpResponseMessage>();
        TimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, TimeoutCts.Token);

        CancellationRegistration = _linkedCts.Token.Register(() =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                CompletionSource.TrySetCanceled(cancellationToken);
            }
            else
            {
                CompletionSource.TrySetException(new TimeoutException(
                    "HTTP tunnel request timed out after 30 seconds"));
            }
        });
    }

    public void Dispose()
    {
        CancellationRegistration.Dispose();
        TimeoutCts.Dispose();
        _linkedCts.Dispose();
    }
}
