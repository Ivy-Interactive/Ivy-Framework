using Microsoft.Extensions.Logging;

namespace Ivy.Plugins;

public sealed class PluginShutdownContext
{
    public required CancellationToken CancellationToken { get; init; }
    public required PluginShutdownReason Reason { get; init; }
    public required ILogger Logger { get; init; }
}
