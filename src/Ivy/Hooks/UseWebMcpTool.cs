using System.Reactive.Disposables;
using System.Text.Json;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A single block of content returned from a WebMCP tool. Mirrors the MCP content-block shape
/// that <c>document.modelContext</c> expects back from <c>execute()</c>.
/// </summary>
public record WebMcpContentBlock(string Type, string Text)
{
    /// <summary>Creates a plain text content block.</summary>
    public static WebMcpContentBlock FromText(string text) => new("text", text);
}

/// <summary>
/// The result of a WebMCP tool invocation. Handlers may return this directly for full control,
/// or return any other value and let <see cref="UseWebMcpToolExtensions"/> normalize it.
/// </summary>
public record WebMcpToolResult
{
    /// <summary>Content blocks handed to the calling agent.</summary>
    public required IReadOnlyList<WebMcpContentBlock> Content { get; init; }

    /// <summary>When true the agent treats the call as failed rather than successful.</summary>
    public bool IsError { get; init; }

    /// <summary>A successful result carrying a single text block.</summary>
    public static WebMcpToolResult Text(string text) => new() { Content = [WebMcpContentBlock.FromText(text)] };

    /// <summary>A successful result carrying no content.</summary>
    public static WebMcpToolResult Empty() => new() { Content = [] };

    /// <summary>A failed result. The agent sees <c>isError: true</c>.</summary>
    public static WebMcpToolResult Error(string message) =>
        new() { Content = [WebMcpContentBlock.FromText(message)], IsError = true };
}

/// <summary>Optional metadata for a tool registered with <c>UseWebMcpTool</c>.</summary>
public record WebMcpToolOptions
{
    /// <summary>Human readable label an agent may show in its UI.</summary>
    public string? Title { get; init; }

    /// <summary>Declares that the tool does not modify state (<c>annotations.readOnlyHint</c>).</summary>
    public bool ReadOnly { get; init; }

    /// <summary>
    /// Declares that the tool returns content originating outside the application
    /// (<c>annotations.untrustedContentHint</c>). Set this for anything echoing database rows or
    /// user-supplied text so the agent treats the result as data rather than instructions.
    /// </summary>
    public bool UntrustedContent { get; init; }

    /// <summary>When false the tool is withheld from the browser without removing the hook call.</summary>
    public bool Enabled { get; init; } = true;
}

/// <summary>A tool as advertised to the browser.</summary>
public record WebMcpToolDescriptor
{
    /// <summary>Stable per-registration identity. Not visible to the agent.</summary>
    public required string ToolId { get; init; }

    /// <summary>The tool name the agent sees. Must be unique within the page.</summary>
    public required string Name { get; init; }

    /// <summary>Natural language description of what the tool does.</summary>
    public required string Description { get; init; }

    /// <summary>Human readable label an agent may show in its UI.</summary>
    public string? Title { get; init; }

    /// <summary>JSON Schema for the tool arguments, serialized as JSON. Null when the tool takes none.</summary>
    public string? InputSchema { get; init; }

    /// <summary><c>annotations.readOnlyHint</c>.</summary>
    public bool ReadOnly { get; init; }

    /// <summary><c>annotations.untrustedContentHint</c>.</summary>
    public bool UntrustedContent { get; init; }

    /// <summary>When false the tool is not pushed to the browser.</summary>
    public bool Enabled { get; init; } = true;
}

/// <summary>Whether the browser on the other end of this connection can actually run WebMCP tools.</summary>
public enum WebMcpAvailability
{
    /// <summary>The browser has not reported yet. Expect this for the first moments of a connection.</summary>
    Unknown,

    /// <summary><c>document.modelContext</c> is present; registered tools are live.</summary>
    Available,

    /// <summary>
    /// The browser does not expose <c>document.modelContext</c>, so no tool can be called. Either
    /// WebMCP is disabled on the server or the browser is not enrolled in the origin trial.
    /// </summary>
    Unavailable
}

/// <summary>
/// Per-connection registry of WebMCP tools. Registrations are pushed to the browser, which mirrors
/// them onto <c>document.modelContext</c>.
/// </summary>
public interface IWebMcpToolService
{
    /// <summary>Registers a tool. Dispose the result to unregister it.</summary>
    IDisposable Register(WebMcpToolDescriptor descriptor, Func<string?, Task<WebMcpToolResult>> handler);

    /// <summary>Replaces the descriptor of an already registered tool, keeping its handler.</summary>
    void Update(string toolId, WebMcpToolDescriptor descriptor);

    /// <summary>Invokes a registered tool with raw JSON arguments.</summary>
    Task<WebMcpToolResult> InvokeAsync(string toolId, string? argumentsJson);

    /// <summary>What the browser last reported about its WebMCP support.</summary>
    WebMcpAvailability Availability { get; }

    /// <summary>Raised when <see cref="Availability"/> changes.</summary>
    event Action? AvailabilityChanged;

    /// <summary>Called from the hub when the browser reports its WebMCP support.</summary>
    void ReportAvailability(bool available);
}

/// <summary>
/// Exposes an Ivy view's capabilities to browser-resident AI agents through the WebMCP
/// (<c>document.modelContext</c>) API. Tools live as long as the view that declared them.
/// </summary>
public static class UseWebMcpToolExtensions
{
    // No arguments.

    /// <summary>Registers a WebMCP tool that takes no arguments and returns nothing.</summary>
    public static void UseWebMcpTool(this IViewContext context, string name, string description,
        Action handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, null, _ =>
        {
            handler();
            return Task.FromResult<object?>(null);
        }, options);

    /// <summary>Registers a WebMCP tool that takes no arguments and returns a result.</summary>
    public static void UseWebMcpTool(this IViewContext context, string name, string description,
        Func<object?> handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, null, _ => Task.FromResult(handler()), options);

    /// <summary>Registers an asynchronous WebMCP tool that takes no arguments and returns nothing.</summary>
    public static void UseWebMcpTool(this IViewContext context, string name, string description,
        Func<Task> handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, null, async _ =>
        {
            await handler();
            return null;
        }, options);

    /// <summary>Registers an asynchronous WebMCP tool that takes no arguments and returns a result.</summary>
    public static void UseWebMcpTool(this IViewContext context, string name, string description,
        Func<Task<object?>> handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, null, _ => handler(), options);

    // With arguments. The JSON Schema advertised to the agent is derived from TArgs.

    /// <summary>Registers a WebMCP tool whose arguments are described by <typeparamref name="TArgs"/>.</summary>
    public static void UseWebMcpTool<TArgs>(this IViewContext context, string name, string description,
        Action<TArgs> handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, typeof(TArgs), json =>
        {
            handler(DeserializeArguments<TArgs>(json));
            return Task.FromResult<object?>(null);
        }, options);

    /// <summary>Registers a WebMCP tool whose arguments are described by <typeparamref name="TArgs"/>.</summary>
    public static void UseWebMcpTool<TArgs>(this IViewContext context, string name, string description,
        Func<TArgs, object?> handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, typeof(TArgs),
            json => Task.FromResult(handler(DeserializeArguments<TArgs>(json))), options);

    /// <summary>Registers an asynchronous WebMCP tool whose arguments are described by <typeparamref name="TArgs"/>.</summary>
    public static void UseWebMcpTool<TArgs>(this IViewContext context, string name, string description,
        Func<TArgs, Task> handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, typeof(TArgs), async json =>
        {
            await handler(DeserializeArguments<TArgs>(json));
            return null;
        }, options);

    /// <summary>Registers an asynchronous WebMCP tool whose arguments are described by <typeparamref name="TArgs"/>.</summary>
    public static void UseWebMcpTool<TArgs>(this IViewContext context, string name, string description,
        Func<TArgs, Task<object?>> handler, WebMcpToolOptions? options = null) =>
        UseWebMcpToolCore(context, name, description, typeof(TArgs),
            json => handler(DeserializeArguments<TArgs>(json)), options);

    /// <summary>
    /// Reports whether the browser can actually run WebMCP tools, so a view can offer a fallback
    /// instead of silently doing nothing. Starts at <see cref="WebMcpAvailability.Unknown"/> and
    /// rebuilds the view once the browser reports in.
    /// </summary>
    public static IState<WebMcpAvailability> UseWebMcpAvailability(this IViewContext context)
    {
        var service = context.UseService<IWebMcpToolService>();
        var state = context.UseState(() => service.Availability);

        context.UseEffect(() =>
        {
            void OnChanged() => state.Set(service.Availability);

            service.AvailabilityChanged += OnChanged;
            // The report can land between this build and the effect running.
            OnChanged();

            return Disposable.Create(() => service.AvailabilityChanged -= OnChanged);
        }, [EffectTrigger.OnMount()]);

        return state;
    }

    private static void UseWebMcpToolCore(IViewContext context, string name, string description,
        Type? argumentsType, Func<string?, Task<object?>> invoke, WebMcpToolOptions? options)
    {
        options ??= new WebMcpToolOptions();

        var service = context.UseService<IWebMcpToolService>();
        var toolId = context.UseRef(() => Guid.NewGuid().ToString("N"));

        // The effect below is registered once and pins the delegate it captured on the first build.
        // Routing through a ref that is refreshed every build keeps the handler — and the view state
        // it closes over — current. See ViewContext.UseEffectHook.
        var invokeRef = context.UseRef(invoke);
        invokeRef.Value = invoke;

        var descriptor = new WebMcpToolDescriptor
        {
            ToolId = toolId.Value,
            Name = name,
            Description = description,
            Title = options.Title,
            InputSchema = argumentsType == null ? null : WebMcpSchemaGenerator.GetSchemaJson(argumentsType),
            ReadOnly = options.ReadOnly,
            UntrustedContent = options.UntrustedContent,
            Enabled = options.Enabled
        };
        var descriptorRef = context.UseRef(descriptor);
        descriptorRef.Value = descriptor;

        // A state whose value is the descriptor itself doubles as the change trigger for the update
        // effect below. buildOnChange is false so refreshing it cannot loop the build.
        var descriptorState = context.UseState(descriptor, buildOnChange: false);
        descriptorState.Set(descriptor);

        // Registration happens exactly once. EffectQueue only drains returned disposables at unmount,
        // so a re-running effect would leak registrations rather than replace them.
        context.UseEffect(
            () => service.Register(descriptorRef.Value, json => NormalizeAsync(invokeRef.Value, json)),
            [EffectTrigger.OnMount()]);

        // Descriptor changes are applied in place, which needs no cleanup.
        context.UseEffect(() => service.Update(toolId.Value, descriptorRef.Value), descriptorState);
    }

    /// <summary>
    /// Maps a handler's return value onto the shape <c>execute()</c> must resolve with, matching the
    /// normalization in GoogleChromeLabs/use-webmcp-tool.
    /// </summary>
    private static async Task<WebMcpToolResult> NormalizeAsync(Func<string?, Task<object?>> invoke, string? argumentsJson)
    {
        try
        {
            var result = await invoke(argumentsJson);
            return result switch
            {
                null => WebMcpToolResult.Empty(),
                WebMcpToolResult toolResult => toolResult,
                string text => WebMcpToolResult.Text(text),
                Exception exception => WebMcpToolResult.Error(ExceptionHelper.GetInnerMostException(exception).Message),
                _ => WebMcpToolResult.Text(JsonSerializer.Serialize(result, JsonHelper.CamelCaseOptions))
            };
        }
        catch (Exception exception)
        {
            // A failure must never read as success to the agent.
            return WebMcpToolResult.Error(ExceptionHelper.GetInnerMostException(exception).Message);
        }
    }

    private static TArgs DeserializeArguments<TArgs>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default!;
        return JsonSerializer.Deserialize<TArgs>(json, JsonHelper.CamelCaseOptions)!;
    }
}
