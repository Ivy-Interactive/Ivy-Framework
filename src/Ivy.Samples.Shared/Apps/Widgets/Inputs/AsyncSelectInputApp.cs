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
        var guidStateNullableMismatch = UseState(default(Guid?));
        var guidStateInvalid = UseState(default(Guid?));
        var onBlurState = UseState(default(Guid?));
        var onBlurLabel = UseState("");
        var onFocusState = UseState(default(Guid?));
        var onFocusLabel = UseState("");
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

        // This is to test the exact nullable mapping fix where delegates return non-nullable Option<Guid>
        QueryResult<Option<Guid>[]> QueryCategoriesNonNullable(IViewContext context, string query)
        {
            var lowerQuery = query.ToLowerInvariant();
            return context.UseQuery<Option<Guid>[], (string, string)>(
                key: (nameof(QueryCategoriesNonNullable), query),
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
                        .Select(e => new Option<Guid>(e.Name, e.Id))];
                });
        }

        QueryResult<Option<Guid>?> LookupCategoryNonNullable(IViewContext context, Guid id)
        {
            return context.UseQuery<Option<Guid>?, (string, Guid)>(
                key: (nameof(LookupCategoryNonNullable), id),
                fetcher: async ct =>
                {
                    await using var db = factory.CreateDbContext();
                    var category = await db.Categories.FindAsync([id], ct);
                    if (category == null) return null;
                    return new Option<Guid>(category!.Name, category!.Id);
                });
        }

        return Layout.Vertical().Gap(6)
            | Text.H1("AsyncSelect Input")
            | Text.H3("Basic")
            | guidState.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category")
            | Text.H3("Ghost")
            | Text.P("Ghost styling removes borders and background fill.")
            | guidStateGhost.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category (Ghost)").Ghost()
            | Text.H3("Nullable State Mapping")
            | Text.P("Testing the nullable mapping fix where the state is Guid? but delegates return Option<Guid>.")
            | guidStateNullableMismatch.ToAsyncSelectInput(QueryCategoriesNonNullable, LookupCategoryNonNullable, placeholder: "Select Category (Nullable Mismatch)")
            | Text.H3("Invalid")
            | guidStateInvalid.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category (Invalid)")
                .Invalid("Category selection is invalid")
            | Text.H2("Events")
            | (Layout.Vertical()
                | new Card(
                    Layout.Vertical().Gap(2)
                        | Text.P("The blur event fires when the input loses focus.").Small()
                        | onBlurState.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category").OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                        | (onBlurLabel.Value != ""
                            ? Callout.Success(onBlurLabel.Value)
                            : Callout.Info("Interact then click away to see blur events"))
                ).Title("OnBlur Handler")
                | new Card(
                    Layout.Vertical().Gap(2)
                        | Text.P("The focus event fires when you click on or tab into the input.").Small()
                        | onFocusState.ToAsyncSelectInput(QueryCategories, LookupCategory, placeholder: "Select Category").OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                        | (onFocusLabel.Value != ""
                            ? Callout.Success(onFocusLabel.Value)
                            : Callout.Info("Click or tab into the input to see focus events"))
                ).Title("OnFocus Handler")
            );
    }
}
