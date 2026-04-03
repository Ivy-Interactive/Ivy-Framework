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

        try
        {
            Console.WriteLine($"[PlanViewer] Loading plan from: {folderPath}");
            var folderName = Path.GetFileName(folderPath);
            var content = planService.ReadLatestRevision(folderName);
            Console.WriteLine($"[PlanViewer] Revision content length: {content?.Length ?? 0}");

            if (string.IsNullOrEmpty(content))
                return Text.P("Plan not found or empty.");

            var plan = planService.GetPlanByFolder(folderPath);
            Console.WriteLine($"[PlanViewer] Plan loaded: {plan?.Id} {plan?.Title}");
            var title = plan?.Title ?? folderName;

            var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
                | Text.Block($"#{plan?.Id} {title}").Bold()
                | new Badge(plan?.Project ?? "").Variant(BadgeVariant.Outline)
                | new Badge(plan?.Level ?? "").Variant(config.GetBadgeVariant(plan?.Level ?? ""));

            var mainLayout = new HeaderLayout(
                header: header,
                content: new Markdown(MarkdownHelper.AnnotateBrokenFileLinks(content)).DangerouslyAllowLocalFiles()
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
                    if (File.Exists(filePath2))
                    {
                        var fileContent = File.ReadAllText(filePath2);
                        var language = FileApp.GetLanguage(ext);
                        sheetContent = new Markdown($"```{language.ToString().ToLowerInvariant()}\n{fileContent}\n```");
                    }
                    else
                    {
                        var fileName = Path.GetFileName(filePath2);
                        var repoPaths = (plan?.Repos?.Count ?? 0) > 0
                            ? plan!.Repos
                            : config.GetProject(plan?.Project ?? "")?.RepoPaths ?? [];
                        var suggestions = MarkdownHelper.FindFilesInRepos(repoPaths, fileName);
                        var notFoundContent = suggestions.Count > 0
                            ? $"File not found.\n\nDid you mean:\n{string.Join("\n", suggestions.Select(s => $"- `{s}`"))}"
                            : "File not found.";
                        sheetContent = new Markdown(notFoundContent);
                    }
                }

                var finalContent = File.Exists(filePath2)
                    ? (object)new HeaderLayout(
                        header: new Button("Open in VS Code").Icon(Icons.ExternalLink).Outline().OnClick(() =>
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "code",
                                Arguments = $"\"{filePath2}\"",
                                UseShellExecute = true
                            });
                        }),
                        content: sheetContent
                    )
                    : sheetContent;

                Console.WriteLine("[PlanViewer] Build complete, returning widget tree (sheet)");
                return new Fragment(
                    mainLayout,
                    new Sheet(
                        onClose: () => openFile.Set(null),
                        content: finalContent,
                        title: Path.GetFileName(filePath2)
                    ).Width(Size.Half()).Resizable()
                );
            }

            Console.WriteLine("[PlanViewer] Build complete, returning widget tree");
            return mainLayout;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PlanViewerApp.Build failed: {ex}");
            return Text.P($"Error loading plan: {ex.Message}");
        }
    }
}
