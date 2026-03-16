// ReSharper disable once CheckNamespace
namespace Ivy;

public record LoadingOptions
{
    public string Message { get; init; } = "Loading...";
    public string? Status { get; init; }
    public int? Progress { get; init; }
    public bool Indeterminate { get; init; } = true;
}
