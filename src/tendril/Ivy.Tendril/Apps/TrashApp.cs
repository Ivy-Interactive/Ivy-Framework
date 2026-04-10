using Ivy.Tendril.Apps.Trash;
using Ivy.Tendril.Apps.Trash.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public record TrashFileInfo(
    string FilePath,
    string FileName,
    DateTime Date,
    string OriginalRequest,
    string DuplicateOf,
    string Project,
    string Content);

[App(title: "Trash", icon: Icons.Trash2, group: ["Apps"], order: MenuOrder.Trash, isVisible: false)]
public class TrashApp : ViewBase
{
    public override object Build()
    {
        var configService = UseService<IConfigService>();
        var jobService = UseService<IJobService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var selectedFile = UseState<string?>(null);
        var confirmDelete = UseState(false);
        var searchFilter = UseState<string?>("");
        var openFile = UseState<string?>(null);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromSeconds(10));

        var trashDir = Path.Combine(configService.TendrilHome, "Trash");
        var files = LoadTrashFiles(trashDir);

        // Apply search filter for selection logic
        var filteredFiles = files.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(searchFilter.Value))
        {
            var searchTerm = searchFilter.Value.ToLowerInvariant();
            filteredFiles = filteredFiles.Where(f =>
                f.FileName.ToLowerInvariant().Contains(searchTerm) ||
                f.OriginalRequest.ToLowerInvariant().Contains(searchTerm) ||
                f.Project.ToLowerInvariant().Contains(searchTerm) ||
                f.DuplicateOf.ToLowerInvariant().Contains(searchTerm)
            );
        }

        var filteredList = filteredFiles.ToList();

        // Auto-select first file if selection is invalid
        if (selectedFile.Value is { } sel && filteredList.All(f => f.FilePath != sel))
            selectedFile.Set(filteredList.FirstOrDefault()?.FilePath);

        var selected = filteredList.FirstOrDefault(f => f.FilePath == selectedFile.Value);

        var sidebar = new SidebarView(files, selectedFile, searchFilter);

        // Main content
        object mainContent;
        if (files.Count == 0)
        {
            mainContent = Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                          | new Icon(Icons.Trash2).Size(Size.Units(8)).Color(Colors.Gray)
                          | Text.Muted("No trash files yet")
                          | Text.Muted("Duplicate plans will appear here").Small();
        }
        else if (selected is null)
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
                             ? new Fragment()
                             : Text.Muted($"Duplicate of: {selected.DuplicateOf}"));

            var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
                            | new Button("Delete").Icon(Icons.Trash).Outline().OnClick(() => confirmDelete.Set(true))
                            | new Button("Force Plan").Icon(Icons.Zap).Primary().OnClick(() =>
                            {
                                if (!string.IsNullOrEmpty(selected.OriginalRequest))
                                {
                                    var project = string.IsNullOrEmpty(selected.Project) ? "[Auto]" : selected.Project;
                                    jobService.StartJob("MakePlan", "-Description",
                                        $"{selected.OriginalRequest} [FORCE]", "-Project", project);
                                    client.Toast("MakePlan job started", "Force Plan");
                                }
                            });

            var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200)))
                                    | new Markdown(selected.Content)
                                        .DangerouslyAllowLocalFiles()
                                        .OnLinkClick(FileLinkHelper.CreateFileLinkClickHandler(openFile));

            mainContent = new HeaderLayout(
                header,
                new FooterLayout(
                    actionBar,
                    scrollableContent
                ).Size(Size.Full())
            ).Scroll(Scroll.None).Size(Size.Full());
        }

        var elements = new List<object>
        {
            new SidebarLayout(
                mainContent,
                sidebar
            )
        };

        if (openFile.Value is { } filePath)
        {
            var fileLinkSheet = FileLinkHelper.BuildFileLinkSheet(
                filePath,
                () => openFile.Set(null),
                [],
                configService);
            if (fileLinkSheet is not null)
                elements.Add(fileLinkSheet);
        }

        elements.Add(new DeleteTrashFileDialog(confirmDelete, selected, selectedFile, refreshToken));

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
            var content = FileHelper.ReadAllText(filePath);
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
