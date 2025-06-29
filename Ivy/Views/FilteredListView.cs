using System.Reactive.Linq;
using Ivy.Blades;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Shared;

namespace Ivy.Views;

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

        return BladeHelper.WithHeader(
            (Layout.Horizontal().Gap(1)
             | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
             | toolButtons!),
            loading.Value ? "Loading..." : new List(items)
        );
    }
}