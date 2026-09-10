using System.Collections.Concurrent;
using System.Reactive.Disposables;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Holds the WebMCP tools declared by the views of a single SignalR connection and mirrors them to
/// the browser, which in turn registers them on <c>document.modelContext</c>.
/// </summary>
/// <param name="client">Sender for the owning connection.</param>
/// <param name="enabled">False when the host never called <c>server.UseWebMcp()</c>.</param>
public class WebMcpToolService(IClientProvider client, bool enabled) : IWebMcpToolService
{
    private sealed record Registration(WebMcpToolDescriptor Descriptor, Func<string?, Task<WebMcpToolResult>> Handler);

    private readonly ConcurrentDictionary<string, Registration> _tools = new();

    /// <inheritdoc />
    public WebMcpAvailability Availability { get; private set; } = WebMcpAvailability.Unknown;

    /// <inheritdoc />
    public event Action? AvailabilityChanged;

    /// <inheritdoc />
    public void ReportAvailability(bool available)
    {
        var reported = available ? WebMcpAvailability.Available : WebMcpAvailability.Unavailable;
        if (Availability == reported) return;

        Availability = reported;
        AvailabilityChanged?.Invoke();
    }

    /// <inheritdoc />
    public IDisposable Register(WebMcpToolDescriptor descriptor, Func<string?, Task<WebMcpToolResult>> handler)
    {
        _tools[descriptor.ToolId] = new Registration(descriptor, handler);
        PushToolList();

        return Disposable.Create(() =>
        {
            if (_tools.TryRemove(descriptor.ToolId, out _))
            {
                PushToolList();
            }
        });
    }

    /// <inheritdoc />
    public void Update(string toolId, WebMcpToolDescriptor descriptor)
    {
        if (!_tools.TryGetValue(toolId, out var existing)) return;
        _tools[toolId] = existing with { Descriptor = descriptor };
        PushToolList();
    }

    /// <inheritdoc />
    public Task<WebMcpToolResult> InvokeAsync(string toolId, string? argumentsJson)
    {
        if (!_tools.TryGetValue(toolId, out var registration))
        {
            return Task.FromResult(WebMcpToolResult.Error("This tool is no longer available."));
        }

        return registration.Handler(argumentsJson);
    }

    /// <summary>
    /// Sends the full current tool set. Deltas would buy nothing here: the browser side replaces its
    /// registrations wholesale, so the last message always wins.
    /// </summary>
    private void PushToolList()
    {
        if (!enabled) return;

        var payload = _tools.Values
            .Select(r => r.Descriptor)
            .Where(d => d.Enabled)
            .OrderBy(d => d.Name, StringComparer.Ordinal)
            .Select(d => new Dictionary<string, object?>
            {
                ["toolId"] = d.ToolId,
                ["name"] = d.Name,
                ["title"] = d.Title,
                ["description"] = d.Description,
                ["inputSchema"] = d.InputSchema,
                ["readOnly"] = d.ReadOnly,
                ["untrustedContent"] = d.UntrustedContent
            })
            .ToArray();

        client.Sender.Send("WebMcpTools", payload);
    }
}
