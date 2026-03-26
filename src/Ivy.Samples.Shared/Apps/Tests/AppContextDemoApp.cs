namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Info, group: ["Tests"], isVisible: false, searchHints: ["app", "context", "environment", "debug", "connection"])]
public class AppContextDemoApp : ViewBase
{
    public override object? Build()
    {
        var context = UseService<AppContext>();

        return Layout.Vertical().Gap(4).Padding(4)
            | Text.H1("App Context Demo")
            | Text.Lead("This application displays the current runtime environment information provided by the AppContext.")

            | new Card(
                Layout.Vertical().Gap(2)
                | new { Property = "AppId", Value = context.AppId }.ToDetails()
                | new { Property = "NavigationAppId", Value = context.NavigationAppId }.ToDetails()
                | new { Property = "ConnectionId", Value = context.ConnectionId }.ToDetails()
                | new { Property = "MachineId", Value = context.MachineId }.ToDetails()
            ).Title("Identification").Description("Identifiers for the current app, session and client.")

            | new Card(
                Layout.Vertical().Gap(2)
                | new { Property = "Scheme", Value = context.Scheme }.ToDetails()
                | new { Property = "Host", Value = context.Host }.ToDetails()
                | new { Property = "BasePath", Value = context.BasePath ?? "(none)" }.ToDetails()
                | new { Property = "BaseUrl", Value = context.BaseUrl }.ToDetails()
            ).Title("Hosting").Description("Information about how and where the application is hosted.")

            | new Card(
                Layout.Vertical().Gap(2)
                | Text.P("You can access these values in the frontend using Ivy's utility functions:")
                | new CodeBlock(
                    """
                    import { getIvyBasePath, getIvyHost } from "@/lib/utils";
                    
                    const basePath = getIvyBasePath();
                    const host = getIvyHost();
                    """, Languages.Javascript)
            ).Title("Frontend Usage").Description("Accessing context values in React.");
    }
}
