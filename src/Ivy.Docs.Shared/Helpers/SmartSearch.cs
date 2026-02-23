using Ivy.Core;
using Ivy.Docs.Shared.Services;

// ReSharper disable once CheckNamespace
namespace Ivy.Docs.Shared.Helpers;

/// <summary>
/// Container widget for the smart search UI with custom styling on the frontend.
/// </summary>
public record SmartSearch(params object?[] children) : WidgetBase<SmartSearch>(children.Where(c => c != null).Cast<object>().ToArray())
{
    internal SmartSearch() : this([]) { }
}

/// <summary>
/// View for smart search: ask a question and get an AI-generated answer inline
/// with optional links to reference docs. Results shown below the search bar (no sheet).
/// </summary>
public class SmartSearchView : ViewBase
{
    public override object? Build()
    {
        var questionsClient = UseService<IIvyDocsQuestionsClient>();
        var client = UseService<IClientProvider>();
        var inputState = UseState("");
        var queryQuestion = UseState(() => (string?)null);

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
            var questionChanged = q != queryQuestion.Value;
            if (questionChanged)
                query.Mutator.Invalidate();
            queryQuestion.Set(q);
        }

        object? resultsContent = null;
        if (queryQuestion.Value != null)
        {
            var isFetching = query.Loading || query.Validating;
            if (isFetching)
            {
                resultsContent = Layout.Vertical().Gap(4)
                    | Layout.Horizontal().Gap(2).Align(Align.Center)
                        | new Loading()
                        | Text.P("Finding an answer...")
                    | new Skeleton().Height(80)
                    | new Skeleton().Height(120)
                    | new Skeleton().Height(60);
            }
            else if (query.Error is { } err)
            {
                resultsContent = Layout.Vertical().Gap(4)
                    | Callout.Error(err.Message)
                    | new Button("Retry", _ => query.Mutator.Revalidate()).Variant(ButtonVariant.Outline);
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

                resultsContent = Layout.Vertical().Gap(4)
                    | new Markdown(result.Answer)
                        | (sourceLinks != null
                        ? Layout.Vertical().Gap(2)
                            | new Separator()
                            | sourceLinks
                        : null);
            }
            else if (!query.Loading && !query.Validating && query.Error is null)
            {
                resultsContent = Layout.Center()
                    | (Layout.Vertical().Gap(4).Center()
                        | Text.H1("No answer found :|").Bold()
                        | Text.Muted("We couldn't find an answer to your question in the Ivy docs. Try rephrasing or browse the documentation.")
                    );
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

        var content = Layout.Vertical().Gap(4)
            | searchBar
            | (resultsContent ?? (object?)null!);
        return new SmartSearch(content);
    }
}
