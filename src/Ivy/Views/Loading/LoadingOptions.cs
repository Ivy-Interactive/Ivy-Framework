// ReSharper disable once CheckNamespace
namespace Ivy;

public record LoadingOptions
{
    public static readonly TimeSpan DefaultCancellingDisplayDuration = TimeSpan.FromMilliseconds(800);

    public string Message { get; init; } = "Loading...";
    public string? Status { get; init; }
    public int? Progress { get; init; }
    public bool Indeterminate { get; init; } = true;
    public bool Cancellable { get; init; }
    public bool IsCancelling { get; internal init; }

    public TimeSpan CancellingDisplayDuration { get; init; } = DefaultCancellingDisplayDuration;
}
