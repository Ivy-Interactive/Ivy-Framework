using System.Reactive.Linq;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Shared;
using Ivy.Views.Blades;

namespace Ivy.Views;

/// <summary>Represents a filtered list view that provides search functionality and dynamic data fetching based on filter criteria.</summary>
/// <typeparam name="T">The type of data records to display in the filtered list.</typeparam>
public class FilteredListView<T>(
    Func<string, Task<T[]>> fetchRecords,
    Func<T, ListItem> createItem,
    object? toolButtons = null,
    TimeSpan? throttle = null,
    Action<string>? onFilterChanged = null
) : ViewBase
{
    /// <summary>
    /// Builds the filtered list view layout, including search input,
    /// tool buttons, and the filtered list of items with loading states.
    /// </summary>
    /// <returns>A blade layout containing search input, tool buttons,
    /// and the filtered list with appropriate loading indicators.</returns>
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
            loading.Value ? Text.Muted("Loading...") : new List(items)
        );
    }
}