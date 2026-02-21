using Ivy.Docs.Shared.Services;

namespace Ivy.Docs.Shared.Views;

/// <summary>
/// Smart search widget for the docs app: ask a question and get an AI-generated answer
/// with optional links to reference docs. Shown at the top of the main content.
/// </summary>
public class SmartSearchView : ViewBase
{
    public override object? Build()
    {
        var questionsClient = UseService<IIvyDocsQuestionsClient>();
        var client = UseService<IClientProvider>();
        var inputState = UseState("");
        var queryQuestion = UseState(() => (string?)null); // when set, we run the query and show sheet
        var isSheetOpen = UseState(false);

        var query = UseQuery<IvyDocsQuestionResult?, string>(
            key: queryQuestion.Value,
            fetcher: async (question, ct) =>
            {
                if (string.IsNullOrWhiteSpace(question)) return null;
                return await questionsClient.AskAsync(question!, ct).ConfigureAwait(false);
            },
            options: new QueryOptions { Scope = QueryScope.View, RevalidateOnMount = false });

        void SubmitQuestion()
        {
            var q = inputState.Value?.Trim();
            if (string.IsNullOrEmpty(q)) return;
            query.Mutator.Invalidate(); // clear result and set Loading so sheet shows searching view
            queryQuestion.Set(q);
            isSheetOpen.Set(true);
        }

        object? sheetContent = null;
        if (isSheetOpen.Value)
        {
            var isFetching = query.Loading || query.Validating;
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
            else if (query.Error is { } err)
            {
                sheetContent = Layout.Vertical().Gap(4)
                    | Callout.Error(err.Message)
                    | new Button("Retry", _ =>
                    {
                        query.Mutator.Revalidate();
                    }).Variant(ButtonVariant.Outline);
            }
            else if (query.Value is { } result)
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
                .Width(Size.Fraction(0.4f))
            : null;

        return Layout.Vertical().Gap(2)
            | new Card(searchBar)
            | (sheet ?? (object?)null!);
    }
}
