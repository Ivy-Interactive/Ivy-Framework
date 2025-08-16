using Ivy.Samples.Shared.Helpers;
using Ivy.Shared;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Timer)]
public class AsyncSelectInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var guidState = this.UseState(default(Guid?));

        var factory = UseService<SampleDbContextFactory>();

        async Task<Option<Guid?>[]> QueryCategories(string query)
        {
            await using var db = factory.CreateDbContext();
            return (await db.Categories
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.Name, e.Id))
                .ToArray();
        }

        async Task<Option<Guid?>?> LookupCategory(Guid? id)
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var category = await db.Categories.FindAsync(id);
            if (category == null) return null;
            return new Option<Guid?>(category!.Name, category!.Id);
        }

        return Layout.Vertical(
            guidState.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category")
        );
    }
}