namespace Ivy.Tendril.Apps.Trash;

public class SidebarView(
    List<TrashFileInfo> files,
    IState<string?> selectedFile,
    IState<string?> searchFilter) : ViewBase
{
    private readonly List<TrashFileInfo> _files = files;
    private readonly IState<string?> _selectedFile = selectedFile;
    private readonly IState<string?> _searchFilter = searchFilter;

    public override object Build()
    {
        var filteredFiles = _files.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(_searchFilter.Value))
        {
            var searchTerm = _searchFilter.Value.ToLowerInvariant();
            filteredFiles = filteredFiles.Where(f =>
                f.FileName.ToLowerInvariant().Contains(searchTerm) ||
                f.OriginalRequest.ToLowerInvariant().Contains(searchTerm) ||
                f.Project.ToLowerInvariant().Contains(searchTerm) ||
                f.DuplicateOf.ToLowerInvariant().Contains(searchTerm)
            );
        }

        var filteredList = filteredFiles.ToList();

        var header = _searchFilter.ToSearchInput().Placeholder("Search trash...");

        object content;
        if (filteredList.Count == 0)
        {
            content = Layout.Vertical().AlignContent(Align.Center).Gap(2).Padding(4)
                | new Icon(Icons.Trash2).Size(Size.Units(6)).Color(Colors.Gray)
                | Text.Muted("No trash items")
                | Text.Muted("Duplicate plans will appear here").Small();
        }
        else
        {
            content = new List(filteredList.Select(f =>
            {
                var item = f;
                return new ListItem(item.FileName.Replace(".md", ""))
                    .Content(Layout.Horizontal().Gap(1)
                        | new Badge(item.Project).Variant(BadgeVariant.Outline).Small()
                        | Text.Muted(item.Date.ToString("yyyy-MM-dd")).Small())
                    .OnClick(() => _selectedFile.Set(item.FilePath));
            }));
        }

        return new HeaderLayout(header, content);
    }
}
