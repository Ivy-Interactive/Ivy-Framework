using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations;

public class SidebarView(
    List<Recommendation> recommendations,
    IState<Recommendation?> selectedState,
    IState<string?> projectFilter,
    IState<string?> planStatusFilter,
    int totalCount,
    bool hasActiveFilters,
    IState<string?> textFilter) : ViewBase
{
    private readonly bool _hasActiveFilters = hasActiveFilters;
    private readonly IState<string?> _planStatusFilter = planStatusFilter;
    private readonly IState<string?> _projectFilter = projectFilter;
    private readonly List<Recommendation> _recommendations = recommendations;
    private readonly IState<Recommendation?> _selectedState = selectedState;
    private readonly IState<string?> _textFilter = textFilter;
    private readonly int _totalCount = totalCount;

    public object BuildHeader()
    {
        var projectOptions = _recommendations
            .GroupBy(r => r.Project)
            .OrderByDescending(g => g.Count())
            .Select(g => new Option<string>($"{g.Key} ({g.Count()})", g.Key))
            .ToArray<IAnyOption>();

        var statusOptions = _recommendations
            .Select(r => r.SourcePlanStatus)
            .Distinct()
            .OrderBy(s => s)
            .Select(s => new Option<string>(s.ToString(), s.ToString()))
            .ToArray<IAnyOption>();

        return Layout.Vertical()
               | _textFilter.ToSearchInput().Placeholder("Search recommendations...")
               | new Expandable(
                   "Filters",
                   Layout.Vertical()
                   | _projectFilter.ToSelectInput(projectOptions).Placeholder("All Projects").Nullable().WithField()
                       .Label("Project")
                   | _planStatusFilter.ToSelectInput(statusOptions).Placeholder("All Statuses").Nullable().WithField()
                       .Label("Plan Status")
               ).Open(false).Ghost();
    }

    public object BuildContent()
    {
        var filtered = _recommendations
            .Where(r => _projectFilter.Value == null || r.Project == _projectFilter.Value)
            .Where(r => _planStatusFilter.Value == null || r.SourcePlanStatus.ToString() == _planStatusFilter.Value)
            .Where(r =>
            {
                if (string.IsNullOrWhiteSpace(_textFilter.Value)) return true;
                var search = _textFilter.Value.ToLowerInvariant();
                return r.Title.ToLowerInvariant().Contains(search) ||
                       r.Description.ToLowerInvariant().Contains(search) ||
                       r.PlanId.Contains(search) ||
                       r.PlanTitle.ToLowerInvariant().Contains(search);
            })
            .ToList();

        if (filtered.Count == 0 && _hasActiveFilters && _totalCount > 0)
            return Layout.Vertical().AlignContent(Align.Center).Gap(2).Padding(4)
                   | new Icon(Icons.ListFilterPlus).Size(Size.Units(6)).Color(Colors.Gray)
                   | Text.Muted("No matching recommendations")
                   | Text.Muted("Try adjusting your filters").Small();

        return new List(filtered.Select(rec =>
        {
            var clickableRec = rec;

            var preview = rec.Description.Length > 120
                ? rec.Description[..120] + "…"
                : rec.Description;

            return new ListItem($"#{rec.PlanId} {rec.Title}", preview)
                .OnClick(() => _selectedState.Set(clickableRec));
        }));
    }

    public override object Build()
    {
        return BuildContent();
    }
}