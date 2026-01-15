using System.Reactive.Linq;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views.Blades;

namespace Ivy.Views;

/// <summary>
/// A generic list view with built-in search filtering and asynchronous data fetching.
/// Provides a search input, loading state, and renders items using a blade-style header layout.
/// </summary>
public class FilteredListView<T>(
    Func<string, Task<T[]>> fetchRecords,
    Func<T, ListItem> createItem,
    object? toolButtons = null,
    TimeSpan? throttle = null,
    Action<string>? onFilterChanged = null
) : ViewBase
{
    public override object? Build()
    {
        var records = UseState(Array.Empty<T>);

        var filter = UseState("");
        var loading = UseState(true);

        UseEffect(() =>
        {
            onFilterChanged?.Invoke(filter.Value);
            loading.Set(true);
        }, [filter]);

        UseEffect(async () =>
        {
            records.Set(await fetchRecords(filter.Value));
            loading.Set(false);
        }, [filter.Throttle(throttle ?? TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        var items = records.Value.Select(createItem);

        var header = Layout.Horizontal().Gap(1)
                      | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                      | toolButtons;

        return new Fragment()
               | new BladeHeader(header)
               | (loading.Value ? Text.Muted("Loading...") : new List(items))
            ;

    }
}