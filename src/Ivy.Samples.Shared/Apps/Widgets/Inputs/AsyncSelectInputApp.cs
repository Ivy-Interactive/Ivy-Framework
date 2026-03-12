using Ivy.Samples.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Timer, searchHints: ["dropdown", "autocomplete", "search", "picker", "async", "options"])]
public class AsyncSelectInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var guidState = UseState(default(Guid?));
        var guidStateGhost = UseState(default(Guid?));
        var factory = UseService<SampleDbContextFactory>();

        QueryResult<Option<Guid?>[]> QueryCategories(IViewContext context, string query)
        {
            var lowerQuery = query.ToLowerInvariant();
            return context.UseQuery<Option<Guid?>[], (string, string)>(
                key: (nameof(QueryCategories), query),
                fetcher: async ct =>
                {
                    await using var db = factory.CreateDbContext();
                    return [.. (await db.Categories
                            .Where(e => EF.Functions.Like(e.Name.ToLower(), $"%{lowerQuery}%"))
                            .OrderBy(e => e.Name)
                            .ThenBy(e => e.Id)
                            .Select(e => new { e.Id, e.Name })
                            .Distinct()
                            .Take(50)
                            .ToArrayAsync(ct))
                        .Select(e => new Option<Guid?>(e.Name, e.Id))];
                });
        }

        QueryResult<Option<Guid?>?> LookupCategory(IViewContext context, Guid? id)
        {
            return context.UseQuery<Option<Guid?>?, (string, Guid?)>(
                key: (nameof(LookupCategory), id),
                fetcher: async ct =>
                {
                    if (id == null) return null;
                    await using var db = factory.CreateDbContext();
                    var category = await db.Categories.FindAsync([id], ct);
                    if (category == null) return null;
                    return new Option<Guid?>(category!.Name, category!.Id);
                });
        }

        return Layout.Vertical().Gap(6)
            | Text.H3("Basic")
            | guidState.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category")
            | Text.H3("Ghost")
            | Text.P("Ghost styling removes borders and background fill.")
            | guidStateGhost.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category (Ghost)").Ghost();
    }
}
