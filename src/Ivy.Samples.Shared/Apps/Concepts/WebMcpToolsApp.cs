using System.Collections.Immutable;
using System.ComponentModel;

namespace Ivy.Samples.Shared.Apps.Concepts;

/// <summary>
/// Exposes this view's capabilities to a browser-resident AI agent through WebMCP.
///
/// The view renders the todo list but offers no way to edit it: there is no input, no add button and
/// no delete action. Every mutation has to arrive through <c>document.modelContext</c>, which makes
/// the point of WebMCP hard to miss — the handlers run server-side against the very state the UI is
/// built from, so the list re-renders live as the agent works.
/// </summary>
[App(icon: Icons.Bot, searchHints: ["webmcp", "mcp", "agent", "ai", "tools", "modelcontext"])]
public class WebMcpToolsApp : SampleBase
{
    private const string AddTool = "add-todo";
    private const string AddDescription = "Add a new item to the user's active todo list";

    private const string RemoveTool = "remove-todo";
    private const string RemoveDescription = "Remove an item from the user's active todo list";

    private const string ListTool = "list-todos";
    private const string ListDescription = "List the user's current todo items";

    private const string ClearTool = "clear-todos";
    private const string ClearDescription = "Remove every item from the user's todo list";

    public record AddTodoArgs(
        [property: Description("The text content of the todo item")] string Text);

    public record RemoveTodoArgs(
        [property: Description("The exact text of the todo item to remove")] string Text);

    protected override object? BuildSample()
    {
        var todos = UseState(ImmutableArray<string>.Empty);
        var webMcp = UseWebMcpAvailability();

        UseWebMcpTool(AddTool, AddDescription,
            (AddTodoArgs args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Text))
                {
                    return WebMcpToolResult.Error("A todo item needs some text.");
                }

                todos.Set(todos.Value.Add(args.Text));
                return $"Added todo item: \"{args.Text}\"";
            });

        UseWebMcpTool(RemoveTool, RemoveDescription,
            (RemoveTodoArgs args) =>
            {
                if (!todos.Value.Contains(args.Text))
                {
                    return WebMcpToolResult.Error($"There is no todo item called \"{args.Text}\".");
                }

                todos.Set(todos.Value.Remove(args.Text));
                return $"Removed todo item: \"{args.Text}\"";
            });

        UseWebMcpTool(ListTool, ListDescription,
            () => todos.Value,
            new WebMcpToolOptions { ReadOnly = true, UntrustedContent = true });

        UseWebMcpTool(ClearTool, ClearDescription,
            () =>
            {
                var removed = todos.Value.Length;
                todos.Set(ImmutableArray<string>.Empty);
                return $"Cleared {removed} todo item(s).";
            });

        return Layout.Vertical()
               | Text.H2("Agent-only todo list")
               | Callout.Info(
                   "This list has no editing controls on purpose. The only way to change it is for an "
                   + "AI agent in the browser to call one of the WebMCP tools below.")
               | AvailabilityCallout(webMcp.Value)
               | Text.H4("Items")
               | (todos.Value.IsEmpty
                   ? Text.Muted("Nothing to do yet. Ask an agent to add something.")
                   : Layout.Vertical().Gap(2) | todos.Value.Select(Text.Block).ToArray())
               | Text.H4("Tools exposed to the agent")
               | (Layout.Vertical().Gap(2)
                  | ToolRow(AddTool, AddDescription)
                  | ToolRow(RemoveTool, RemoveDescription)
                  | ToolRow(ListTool, $"{ListDescription} (read only)")
                  | ToolRow(ClearTool, ClearDescription));
    }

    private static object AvailabilityCallout(WebMcpAvailability availability) => availability switch
    {
        WebMcpAvailability.Available => Callout.Success(
            "This browser exposes document.modelContext, so the tools below are live. Drive them "
            + "from an agent or the Model Context Tool Inspector extension."),

        WebMcpAvailability.Unavailable => Callout.Warning(
            "This browser does not expose document.modelContext, so nothing below is callable and "
            + "the list cannot change. WebMCP is still behind a Chrome origin trial: for local "
            + "development enable chrome://flags/#enable-webmcp-testing and relaunch Chrome (no "
            + "token needed), or launch Chrome with --enable-features=WebMCPTesting. A deployed "
            + "origin instead needs a token passed to "
            + "server.UseWebMcp(o => o.OriginTrialToken = \"...\")."),

        _ => Callout.Info("Checking whether this browser supports WebMCP…")
    };

    private static object ToolRow(string name, string description) =>
        Layout.Horizontal().Gap(3)
        | Text.Monospaced(name)
        | Text.Muted(description);
}
