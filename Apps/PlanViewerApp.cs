using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public record PlanViewerAppArgs(string PlanFolderPath);

[App(title: "Plan Viewer", icon: Icons.FileText, isVisible: false)]
public class PlanViewerApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<PlanViewerAppArgs>();
        var planService = UseService<PlanReaderService>();
        var config = UseService<ConfigService>();
        var navigator = UseNavigation();

        if (args?.PlanFolderPath is not { } folderPath || string.IsNullOrWhiteSpace(folderPath))
            return Text.P("No plan path provided.");

        var folderName = Path.GetFileName(folderPath);
        var content = planService.ReadLatestRevision(folderName);

        if (string.IsNullOrEmpty(content))
            return Text.P("Plan not found or empty.");

        var plans = planService.GetPlans();
        var plan = plans.FirstOrDefault(p => p.FolderPath == folderPath);
        var title = plan?.Title ?? folderName;

        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block($"#{plan?.Id} {title}").Bold()
            | new Badge(plan?.Project ?? "").Variant(BadgeVariant.Outline)
            | new Badge(plan?.Level ?? "").Variant(config.GetBadgeVariant(plan?.Level ?? ""));

        return new HeaderLayout(
            header: header,
            content: new Markdown(content).DangerouslyAllowLocalFiles()
                .OnLinkClick(url =>
                {
                    if (url.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                    {
                        var filePath = url.Substring("file:///".Length);
                        navigator.Navigate<FileApp>(new FileAppArgs(filePath));
                    }
                })
        ).Scroll(Scroll.None).Size(Size.Full());
    }
}
