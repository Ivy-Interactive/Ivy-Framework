using System;
using System.Linq;
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
/// View for smart search: ask a question and get an AI-generated answer inline.
/// Results shown below the search bar (no sheet).
/// </summary>
public class SmartSearchView : ViewBase
{
    public override object? Build()
    {
        var questionsClient = UseService<IIvyDocsQuestionsClient>();
        var inputState = UseState("");
        var queryQuestion = UseState(() => (string?)null);
        var resultForQuestion = UseState(() => (string?)null);
        var lastResultRef = UseRef<IvyDocsQuestionResult?>(() => null);

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
            if (string.Equals(q, queryQuestion.Value, StringComparison.Ordinal)) return;
            resultForQuestion.Set((string?)null);
            query.Mutator.Invalidate();
            queryQuestion.Set(q);
        }

        object? resultsContent = null;
        if (queryQuestion.Value != null)
        {
            var resultJustArrived = query.Value != null && !query.Loading && !query.Validating && query.Error is null
                && !ReferenceEquals(query.Value, lastResultRef.Value);
            if (resultJustArrived)
            {
                lastResultRef.Value = query.Value;
                resultForQuestion.Set(queryQuestion.Value);
            }
            var waitingForNewResult = queryQuestion.Value != resultForQuestion.Value;
            var isFetching = query.Loading || query.Validating || waitingForNewResult;
            if (isFetching)
            {
                resultsContent = Layout.Vertical().Gap(4)
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
                resultsContent = Layout.Vertical().Gap(4)
                    | new Markdown(result.Answer);
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

        var searchInput = inputState.ToTextInput()
            .Placeholder("Search...")
            .TestId("docs-smart-search-input");
        var askButton = new Button("Ask", SubmitQuestion)
            .Variant(ButtonVariant.Ai)
            .Small()
            .TestId("docs-smart-search-ask");

        var clearInputButton = new Button("", _ => inputState.Set(""));

        if (queryQuestion.Value == null || resultsContent == null)
        {
            return new SmartSearch([new Slot("SearchInput", searchInput), new Slot("AskButton", askButton), new Slot("ClearInputButton", clearInputButton)]);
        }

        var apiTitle = query.Value is { Title: { } t } && !string.IsNullOrWhiteSpace(t) ? t : null;
        var resultsHeader = apiTitle != null ? Text.H2(apiTitle).Bold() : null;
        var clearButton = new Button("Clear", _ => queryQuestion.Set(_ => (string?)null));
        object[] children = resultsHeader != null
            ? [new Slot("SearchInput", searchInput), new Slot("AskButton", askButton), new Slot("ClearInputButton", clearInputButton), new Slot("ResultsHeader", resultsHeader), new Slot("ResultsContent", resultsContent), new Slot("ClearButton", clearButton)]
            : [new Slot("SearchInput", searchInput), new Slot("AskButton", askButton), new Slot("ClearInputButton", clearInputButton), new Slot("ResultsContent", resultsContent), new Slot("ClearButton", clearButton)];
        return new SmartSearch(children);
    }
}
