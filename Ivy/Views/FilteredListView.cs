using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Blades;

namespace Ivy.Views;

public class FilteredListView<T>(
    Func<string, Task<T[]>> fetchRecords,
    Func<T, ListItem> createItem,
    object? toolButtons = null,
    TimeSpan? throttle = null,
    Action<string>? onFilterChanged = null,
    [CallerFilePath] string callerFile = "",
    [CallerLineNumber] int callerLine = 0
) : ViewBase
{
    public override object? Build()
    {
        var filter = UseState("");
        var throttledFilter = UseState("");

        UseEffect(() =>
        {
            onFilterChanged?.Invoke(filter.Value);
        }, [throttledFilter]);

        // Update throttled value after debounce
        UseEffect(() =>
        {
            throttledFilter.Set(filter.Value);
        }, [filter.Throttle(throttle ?? TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        var query = UseQuery(
            key: (callerFile, callerLine, filter: throttledFilter.Value),
            fetcher: (key, _) => fetchRecords(key.filter),
            options: QueryScope.View);

        var items = (query.Value ?? []).Select(createItem);

        return BladeHelper.WithHeader(
            (Layout.Horizontal().Gap(1)
             | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
             | toolButtons!),
            query.IsLoading
                ? Text.Muted("Loading...")
                : new List(items)
        );
    }
}