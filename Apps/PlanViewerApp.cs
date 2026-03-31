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
        var openFile = UseState<string?>(null);

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

        var mainLayout = new HeaderLayout(
            header: header,
            content: new Markdown(content).DangerouslyAllowLocalFiles()
                .OnLinkClick(url =>
                {
                    if (url.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                    {
                        var filePath = url.Substring("file:///".Length);
                        openFile.Set(filePath);
                    }
                })
        ).Scroll(Scroll.None).Size(Size.Full());

        if (openFile.Value is { } filePath2)
        {
            var ext = Path.GetExtension(filePath2);
            var imageExts = new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp" };
            object sheetContent;
            if (imageExts.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                var imageUrl = $"/ivy/local-file?path={Uri.EscapeDataString(filePath2)}";
                sheetContent = new Image(imageUrl) { ObjectFit = ImageFit.Contain, Alt = Path.GetFileName(filePath2) };
            }
            else
            {
                var fileContent = File.Exists(filePath2) ? File.ReadAllText(filePath2) : "File not found.";
                var language = FileApp.GetLanguage(ext);
                sheetContent = new Markdown($"```{language.ToString().ToLowerInvariant()}\n{fileContent}\n```");
            }

            return new Fragment(
                mainLayout,
                new Sheet(
                    onClose: () => openFile.Set(null),
                    content: sheetContent,
                    title: Path.GetFileName(filePath2)
                ).Width(Size.Half())
            );
        }

        return mainLayout;
    }
}
