using Ivy.Docs.Shared.Services;

namespace Ivy.Docs.Shared.Views;

/// <summary>
/// Smart search widget for the docs app: ask a question and get an AI-generated answer
/// with optional links to reference docs. Shown at the top of the main content.
/// </summary>
public class SmartSearchView : ViewBase
{
    private sealed record AskKey(string Question, int AskId);

    public override object? Build()
    {
        var questionsClient = UseService<IIvyDocsQuestionsClient>();
        var client = UseService<IClientProvider>();
        var inputState = UseState("");
        var queryQuestion = UseState(() => (string?)null); // when set, we run the query and show sheet
        var isSheetOpen = UseState(false);
        var askId = UseState(0); // increment on each Ask so each question gets a fresh query
        // Which key we're showing result for; null = show searching. Cleared on new Ask so we never show old answer.
        var displayedKey = UseState(() => (AskKey?)null);

        var currentKey = queryQuestion.Value is { } q ? new AskKey(q, askId.Value) : (AskKey?)null;
        var query = UseQuery<IvyDocsQuestionResult?, AskKey>(
            key: currentKey,
            fetcher: async (key, ct) =>
            {
                if (string.IsNullOrWhiteSpace(key.Question)) return null;
                return await questionsClient.AskAsync(key.Question, ct).ConfigureAwait(false);
            },
            options: new QueryOptions { Scope = QueryScope.View, RevalidateOnMount = false });

        // When query has finished for current key and we're "waiting" (displayedKey null), commit to showing this result
        UseEffect(() =>
        {
            if (displayedKey.Value is null && currentKey is not null && !query.Loading && !query.Validating && (query.Value is not null || query.Error is not null))
                displayedKey.Set(currentKey);
        }, EffectTrigger.OnBuild());

        void SubmitQuestion()
        {
            var q = inputState.Value?.Trim();
            if (string.IsNullOrEmpty(q)) return;
            query.Mutator.Invalidate();
            displayedKey.Set((AskKey?)null);
            askId.Set(askId.Value + 1);
            queryQuestion.Set(q);
            isSheetOpen.Set(true);
        }

        object? sheetContent = null;
        if (isSheetOpen.Value)
        {
            // Show searching when loading, validating, or result is for a previous question (displayedKey not yet current)
            var isFetching = query.Loading || query.Validating || (currentKey is not null && currentKey != displayedKey.Value);
            if (isFetching)
            {
                sheetContent = Layout.Vertical().Gap(4)
                    | Layout.Horizontal().Gap(2).Align(Align.Center)
                        | new Loading()
                        | Text.P("Finding an answer...")
                    | new Skeleton().Height(80)
                    | new Skeleton().Height(120)
                    | new Skeleton().Height(60);
            }
            else if (query.Error is { } err && currentKey == displayedKey.Value)
            {
                sheetContent = Layout.Vertical().Gap(4)
                    | Callout.Error(err.Message)
                    | new Button("Retry", _ =>
                    {
                        query.Mutator.Revalidate();
                    }).Variant(ButtonVariant.Outline);
            }
            else if (query.Value is { } result && currentKey == displayedKey.Value)
            {
                object? sourceLinks = null;
                if (result.Sources.Count > 0)
                {
                    var linkButtons = result.Sources.Select(s => (object)new Button(s.Title, _ => client.OpenUrl(s.Url))
                        .Variant(ButtonVariant.Ghost)).ToArray();
                    sourceLinks = Layout.Vertical().Gap(2)
                        | Text.P("Reference:").Bold()
                        | (Layout.Vertical().Gap(1) | new Fragment(linkButtons));
                }

                sheetContent = Layout.Vertical().Gap(4)
                    | new Markdown(result.Answer)
                        | (sourceLinks != null
                        ? Layout.Vertical().Gap(2)
                            | new Separator()
                            | sourceLinks
                        : null);
            }
        }

        var searchBar = Layout.Horizontal().Gap(2).Align(Align.Center)
            | (Layout.Vertical().Width(Size.Grow())
                | inputState.ToTextInput()
                    .Placeholder("Ask a question about Ivy... (e.g. how to use BoolInput)")
                    .TestId("docs-smart-search-input"))
            | new Button("Ask", SubmitQuestion)
                .Variant(ButtonVariant.Primary)
                .TestId("docs-smart-search-submit");

        var sheet = sheetContent != null
            ? new Sheet(_ =>
            {
                isSheetOpen.Set(false);
                return ValueTask.CompletedTask;
            }, sheetContent, title: queryQuestion.Value, description: "AI-generated answer from Ivy docs")
                .Width(Size.Rem(28))
            : null;

        return Layout.Vertical().Gap(2)
            | new Card(searchBar)
            | (sheet ?? (object?)null!);
    }
}
