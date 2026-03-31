using Ivy;
using Ivy.Samples.Shared.Apps;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Code, title: "Developer Mock")]
public class DeveloperMockApp : ViewBase
{
    private readonly string sentryBaseUrl = "https://ivy-interactive.sentry.io/issues/";

    public override object? Build()
    {
        var configState = this.UseState<DeveloperConfigMock?>(() => null);
        var billingDebugState = this.UseState<DebugResponseMock?>(() => null);

        this.UseEffect(() =>
        {
            // Simulating a production environment with long strings to expose visual bugs
            configState.Set(new DeveloperConfigMock
            {
                Environment = "Production",
                Version = "d3b07384d11019623e6587c6536b4122",
                BuildMode = "Release",
                Application = "Ivy.Admin (Production Node 1)",
                CurrentUser = "admin_svc",
                OperatingSystem = "Linux (Debian 11)",
                DatabaseHost = "sql-prod-db-01.database.windows.net",
                BillingServiceUrl = "https://billing.ivy.app",
                PackageVersions = new Dictionary<string, string>
                {
                    { "Ivy", "1.2.6.20240325" },
                    { "Microsoft.EntityFrameworkCore", "10.0.5" },
                    { "Azure.Identity", "1.10.4" },
                    { "Refit", "7.0.0" },
                    { "Sentry", "4.1.0" }
                }
            });

            billingDebugState.Set(new DebugResponseMock
            {
                Environment = "Production",
                Version = "f9a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6",
                BuildMode = "Release",
                Application = "Ivy.Billing (Node 2)",
                OperatingSystem = "Linux (Alpine 3.14)",
                DatabaseHost = "sql-prod-db-01.database.windows.net",
                StripeMode = "Live",
                PackageVersions = new Dictionary<string, string>
                {
                    { "Stripe.net", "43.15.0" },
                    { "Hangfire", "1.8.11" },
                    { "Microsoft.EntityFrameworkCore", "10.0.5" },
                    { "Sentry.AspNetCore", "4.1.0" }
                }
            });
        }, []);

        if (configState.Value == null)
        {
            return new Skeleton();
        }

        var config = configState.Value;
        var billingDebug = billingDebugState.Value;

        // System Information Section
        var systemInfoCard = new Card()
            .Title("Admin Service Info")
            | (Layout.Vertical()
                | (Layout.Horizontal()
                    .Align(Align.Left)
                    | Text.P("Environment:").Small()
                    | new Badge(config.Environment)
                        .Variant(config.GetEnvironmentColor() switch
                        {
                            "destructive" => BadgeVariant.Destructive,
                            "warning" => BadgeVariant.Warning,
                            _ => BadgeVariant.Secondary
                        }))
                | (Layout.Horizontal()
                    | Text.P("Application:").Small()
                    | Text.Code(config.Application).Grow())
                | (Layout.Horizontal()
                    | Text.P("Version:").Small()
                    | Text.Code(config.Version.Length > 8 ? config.Version[..8] : config.Version).Grow())
                | (Layout.Horizontal()
                    | Text.P("Build Mode:").Small()
                    | Text.Code(config.BuildMode).Grow())
                | (Layout.Horizontal()
                    | Text.P("User:").Small()
                    | Text.Code(config.CurrentUser).Grow())
                | (Layout.Horizontal()
                    | Text.P("OS:").Small()
                    | Text.Code(config.OperatingSystem).Grow())
                | (config.DatabaseHost != null
                    ? (Layout.Horizontal()
                        | Text.P("Database:").Small()
                        | Text.Code(config.DatabaseHost).Grow())
                    : null)
            );

        // Quick Links Section
        var quickLinksCard = new Card()
            .Title("Quick Links")
            | (Layout.Vertical()
                | BuildLinkButton("Sentry (Admin)", $"{sentryBaseUrl}?project=4510396451258448&statsPeriod=30d&environment={config.Environment}", Icons.Bug, "View Admin service issues in Sentry")
                | BuildLinkButton("Sentry (Web)", $"{sentryBaseUrl}?project=4508878790852688&statsPeriod=30d&environment={config.Environment}", Icons.Bug, "View Web service issues in Sentry")
                | BuildLinkButton("Sentry (Billing)", $"{sentryBaseUrl}?project=4510391080255568&statsPeriod=30d&environment={config.Environment}", Icons.Bug, "View Billing service issues in Sentry")
                | BuildLinkButton("Azure Portal", config.GetAzurePortalUrl("ivy-services"), Icons.Cloud, "Manage Azure resources")
                | BuildLinkButton("GitHub Repository", config.GetGitHubRepoUrl(), Icons.Github, "View source code and issues")
            );

        // Package Versions Section
        var packageVersionsCard = new Card()
            .Title("Package Versions (Ivy.Admin)")
            | (Layout.Vertical()
                | BuildPackageVersionsList(config.PackageVersions)
            );

        // Service Status Section
        var serviceStatusCard = new Card()
            .Title("Service URLs")
            | (Layout.Vertical()
                | BuildServiceUrl("Billing Service", config.BillingServiceUrl)
            );

        // Billing Service Info Section
        var billingInfoCard = new Card()
            .Title("Billing Service Info")
            | (billingDebug != null
                ? (Layout.Vertical()
                    | (Layout.Horizontal()
                        .Align(Align.Left)
                        | Text.P("Environment:").Small()
                        | new Badge(billingDebug.Environment)
                            .Variant(billingDebug.Environment.ToLower() switch
                            {
                                "production" => BadgeVariant.Destructive,
                                "staging" => BadgeVariant.Warning,
                                _ => BadgeVariant.Secondary
                            }))
                    | (Layout.Horizontal()
                        | Text.P("Application:").Small()
                        | Text.Code(billingDebug.Application).Grow())
                    | (Layout.Horizontal()
                        | Text.P("Version:").Small()
                        | Text.Code(billingDebug.Version.Length > 8 ? billingDebug.Version[..8] : billingDebug.Version).Grow())
                    | (Layout.Horizontal()
                        | Text.P("Build Mode:").Small()
                        | Text.Code(billingDebug.BuildMode).Grow())
                    | (Layout.Horizontal()
                        | Text.P("OS:").Small()
                        | Text.Code(billingDebug.OperatingSystem).Grow())
                    | (billingDebug.DatabaseHost != null
                        ? (Layout.Horizontal()
                            | Text.P("Database:").Small()
                            | Text.Code(billingDebug.DatabaseHost).Grow())
                        : null)
                    | (billingDebug.StripeMode != null
                        ? (Layout.Horizontal()
                            | Text.P("Stripe Mode:").Small()
                            | new Badge(billingDebug.StripeMode)
                                .Variant(billingDebug.StripeMode == "Live" ? BadgeVariant.Destructive : BadgeVariant.Secondary))
                        : null))
                : new Skeleton());

        // Debug Section
        var debugCard = new Card()
            .Title("Debug")
            | (Layout.Vertical()
                | BuildDebugButton("Throw Admin Exception", () => { })
                | BuildDebugButton("Throw Billing Exception", async () => { await Task.CompletedTask; })
                | BuildDebugButton("Send Test Slack Message", async () => { await Task.CompletedTask; })
                | BuildDebugButton("Send Test Error Log", () => { })
                | BuildDebugButton("Send Warning Log", () => { })
                | BuildDebugButton("Send Info Log", () => { })
            );

        var column1 = Layout.Vertical(
            Text.H3("Info"),
            systemInfoCard,
            billingInfoCard,
            packageVersionsCard,
            serviceStatusCard
        ).Height(Size.MaxContent()).Width(Size.Full());

        var column2 = Layout.Vertical(
            Text.H3("Links & Debug"),
            quickLinksCard,
            debugCard
        ).Height(Size.MaxContent()).Width(Size.Full());

        return Layout.Horizontal()
            .Align(Align.TopLeft)
            .Width(Size.Full())
            | column1
            | column2;
    }

    private static object BuildLinkButton(string label, string? url, Icons icon, string tooltip)
    {
        if (string.IsNullOrEmpty(url))
        {
            return new Button(label)
                .Icon(icon)
                .Variant(ButtonVariant.Outline)
                .Disabled()
                .Tooltip("Not configured")
                .Width(Size.Full());
        }

        return new Button(label)
            .Icon(icon)
            .Variant(ButtonVariant.Outline)
            .Url(url)
            .Tooltip(tooltip)
            .Width(Size.Full());
    }

    private static object BuildServiceUrl(string serviceName, string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return Layout.Horizontal()
                .Align(Align.Left)
                | Text.P($"{serviceName}:").Small()
                | Text.Code("Not configured");
        }

        return Layout.Horizontal()
            .Align(Align.Left)
            | Text.P($"{serviceName}:").Small()
            | new Button(url)
                .Variant(ButtonVariant.Inline)
                .Url(url)
                .Tooltip($"Open {serviceName}");
    }

    private static object BuildPackageVersionsList(Dictionary<string, string> versions)
    {
        if (versions.Count == 0)
        {
            return Text.P("No package versions available").Small();
        }

        var items = Layout.Vertical();

        foreach (var (package, version) in versions.OrderBy(kv => kv.Key))
        {
            items = items
                | (Layout.Horizontal()
                    .Align(Align.Left)
                    | Text.P($"{package}:").Small()
                    | Text.Code(version).Grow());
        }

        return items;
    }

    private static object BuildDebugButton(string label, Action action)
    {
        return new Button(label)
            .Variant(ButtonVariant.Outline)
            .OnClick(() => action())
            .Width(Size.Full());
    }

    private static object BuildDebugButton(string label, Func<Task> asyncAction)
    {
        return new Button(label)
            .Variant(ButtonVariant.Outline)
            .OnClick(async () => await asyncAction())
            .Width(Size.Full());
    }
}

public class DeveloperConfigMock
{
    public string Environment { get; set; } = "Unknown";
    public string Version { get; set; } = "Unknown";
    public string BuildMode { get; set; } = "Unknown";
    public string Application { get; set; } = "Unknown";
    public string CurrentUser { get; set; } = "Unknown";
    public string OperatingSystem { get; set; } = "Unknown";
    public string? DatabaseHost { get; set; }
    public string? BillingServiceUrl { get; set; }
    public Dictionary<string, string> PackageVersions { get; set; } = new();

    public string GetEnvironmentColor() => Environment.ToLower() switch
    {
        "production" => "destructive",
        "staging" => "warning",
        _ => "secondary"
    };

    public string GetAzurePortalUrl(string serviceName) => "https://portal.azure.com";
    public string GetGitHubRepoUrl() => "https://github.com/Ivy-Interactive/Ivy-Services";
}

public class DebugResponseMock
{
    public string Environment { get; set; } = "Unknown";
    public string Version { get; set; } = "Unknown";
    public string BuildMode { get; set; } = "Unknown";
    public string Application { get; set; } = "Unknown";
    public string OperatingSystem { get; set; } = "Unknown";
    public string? DatabaseHost { get; set; }
    public string? StripeMode { get; set; }
    public Dictionary<string, string> PackageVersions { get; set; } = new();
}
