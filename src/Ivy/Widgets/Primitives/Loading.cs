using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A centralized loading spinner or indicator. Can overlay content or block interaction during async operations.
/// </summary>
public record Loading : WidgetBase<Loading>
{
    public Loading() { }
}

public static class LoadingExtensions
{

}