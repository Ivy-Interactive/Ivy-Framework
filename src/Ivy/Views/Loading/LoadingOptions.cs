// ReSharper disable once CheckNamespace
namespace Ivy;

public record LoadingOptions
{
    public string Message { get; init; } = "Loading...";
    public string? Status { get; init; }
    public int? Progress { get; init; }
    public bool Indeterminate { get; init; } = true;
    public bool Cancellable { get; init; }
    public bool IsCancelling { get; init; }
    public TimeSpan CancellingDisplayDuration { get; init; } = TimeSpan.FromMilliseconds(800);
}
