using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public record TrashFileInfo(string FilePath, string FileName, DateTime Date, string OriginalRequest, string DuplicateOf, string Project, string Content);

[App(title: "Trash", icon: Icons.Trash2, group: new[] { "Tools" }, order: 40, isVisible: false)]
public class TrashApp : ViewBase
{
    public override object? Build()
    {
        var configService = UseService<ConfigService>();
        var jobService = UseService<JobService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var selectedFile = UseState<string?>(null);
        var confirmDelete = UseState(false);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromSeconds(10));

        var trashDir = Path.Combine(configService.TendrilHome, "Trash");
        var files = LoadTrashFiles(trashDir);

        // Auto-select first file if selection is invalid
        if (selectedFile.Value is { } sel && !files.Any(f => f.FilePath == sel))
            selectedFile.Set(files.FirstOrDefault()?.FilePath);

        var selected = files.FirstOrDefault(f => f.FilePath == selectedFile.Value);

        // Sidebar content - file list
        var sidebarContent = new List(files.Select(f =>
        {
            var item = f;
            return new ListItem(item.FileName.Replace(".md", ""))
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(item.Project).Variant(BadgeVariant.Outline).Small()
                    | Text.Muted(item.Date.ToString("yyyy-MM-dd")).Small())
                .OnClick(() => selectedFile.Set(item.FilePath));
        }));

        // Main content
        object mainContent;
        if (selected is null)
        {
            mainContent = Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                | Text.Muted("Select a file from the sidebar");
        }
        else
        {
            var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
                | Text.Block(selected.FileName).Bold()
                | new Badge(selected.Project).Variant(BadgeVariant.Outline)
                | (string.IsNullOrEmpty(selected.DuplicateOf)
                    ? (object)new Fragment()
                    : Text.Muted($"Duplicate of: {selected.DuplicateOf}"));

            var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
                | new Button("Delete").Icon(Icons.Trash).Outline().OnClick(() => confirmDelete.Set(true))
                | new Button("Force Plan").Icon(Icons.Zap).Primary().OnClick(() =>
                {
                    if (!string.IsNullOrEmpty(selected.OriginalRequest))
                    {
                        var project = string.IsNullOrEmpty(selected.Project) ? "[Auto]" : selected.Project;
                        jobService.StartJob("MakePlan", "-Description", $"[FORCE] {selected.OriginalRequest}", "-Project", project);
                        client.Toast("MakePlan job started", "Force Plan");
                    }
                });

            var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200)))
                | new Markdown(selected.Content);

            mainContent = new HeaderLayout(
                header: header,
                content: new FooterLayout(
                    footer: actionBar,
                    content: scrollableContent
                ).Size(Size.Full())
            ).Scroll(Scroll.None).Size(Size.Full());
        }

        var elements = new List<object>
        {
            new SidebarLayout(
                mainContent: mainContent,
                sidebarContent: sidebarContent
            )
        };

        if (confirmDelete.Value && selected is not null)
        {
            var deletePath = selected.FilePath;
            elements.Add(new Dialog(
                _ => confirmDelete.Set(false),
                new DialogHeader("Delete Trash File"),
                new DialogBody(
                    Text.P($"Permanently delete {selected.FileName}?")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => confirmDelete.Set(false)),
                    new Button("Delete").Destructive().ShortcutKey("Enter").AutoFocus().OnClick(() =>
                    {
                        if (File.Exists(deletePath))
                            File.Delete(deletePath);
                        selectedFile.Set(null);
                        confirmDelete.Set(false);
                        refreshToken.Refresh();
                    })
                )
            ).Width(Size.Rem(40)));
        }

        return new Fragment(elements.ToArray());
    }

    private static List<TrashFileInfo> LoadTrashFiles(string trashDir)
    {
        if (!Directory.Exists(trashDir))
            return [];

        return Directory.GetFiles(trashDir, "*.md")
            .Select(ParseTrashFile)
            .Where(f => f is not null)
            .Select(f => f!)
            .OrderByDescending(f => f.Date)
            .ToList();
    }

    private static TrashFileInfo? ParseTrashFile(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);

            var date = DateTime.MinValue;
            var originalRequest = "";
            var duplicateOf = "";
            var project = "";
            var body = content;

            // Parse YAML frontmatter
            if (content.StartsWith("---"))
            {
                var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
                if (endIndex > 0)
                {
                    var frontmatter = content.Substring(3, endIndex - 3);
                    body = content.Substring(endIndex + 3).TrimStart('\r', '\n');

                    foreach (var line in frontmatter.Split('\n'))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("date:"))
                        {
                            var val = trimmed.Substring("date:".Length).Trim();
                            DateTime.TryParse(val, out date);
                        }
                        else if (trimmed.StartsWith("originalRequest:"))
                        {
                            originalRequest = trimmed.Substring("originalRequest:".Length).Trim().Trim('"');
                        }
                        else if (trimmed.StartsWith("duplicateOf:"))
                        {
                            duplicateOf = trimmed.Substring("duplicateOf:".Length).Trim().Trim('"');
                        }
                        else if (trimmed.StartsWith("project:"))
                        {
                            project = trimmed.Substring("project:".Length).Trim().Trim('"');
                        }
                    }
                }
            }

            return new TrashFileInfo(filePath, fileName, date, originalRequest, duplicateOf, project, body);
        }
        catch
        {
            return null;
        }
    }
}
