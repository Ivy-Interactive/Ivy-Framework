using Ivy.Docs.Shared.Services;

namespace Ivy.Docs.Shared.Helpers;

public record SmartSearch(params object?[] children) : WidgetBase<SmartSearch>(children.Where(c => c != null).Cast<object>().ToArray())
{
    internal SmartSearch() : this([]) { }
}

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

        var followUpMessages = UseState(Array.Empty<ChatMessage>());
        var pendingFollowUp = UseState(() => (string?)null);

        UseEffect(
            async () =>
            {
                var q = pendingFollowUp.Value;
                if (string.IsNullOrWhiteSpace(q)) return;
                var result = await questionsClient.AskAsync(q!).ConfigureAwait(false);
                var prev = followUpMessages.Value;
                var withoutLoading = prev.Length > 0 && prev[^1].Children.Length == 1 && prev[^1].Children[0] is ChatLoading
                    ? prev.Take(prev.Length - 1).ToArray()
                    : prev;
                var answerContent = result is { Answer: { } ans }
                    ? (object)new Markdown(ans)
                    : (object)new Markdown("No answer returned.");
                followUpMessages.Set(withoutLoading.Concat(new[] { new ChatMessage(ChatSender.Assistant, answerContent) }).ToArray());
                pendingFollowUp.Set(default(string?));
            },
            EffectTrigger.OnStateChange(pendingFollowUp));

        void SubmitQuestion()
        {
            var q = inputState.Value?.Trim();
            if (string.IsNullOrEmpty(q)) return;
            if (string.Equals(q, queryQuestion.Value, StringComparison.Ordinal)) return;
            resultForQuestion.Set((string?)null);
            query.Mutator.Invalidate();
            queryQuestion.Set(q);
        }

        ValueTask OnFollowUpSend(Event<Chat, string> e)
        {
            var text = e.Value?.Trim();
            if (string.IsNullOrEmpty(text)) return ValueTask.CompletedTask;
            var userMsg = new ChatMessage(ChatSender.User, text);
            var loadingMsg = new ChatMessage(ChatSender.Assistant, new ChatLoading());
            followUpMessages.Set(followUpMessages.Value.Concat(new[] { userMsg, loadingMsg }).ToArray());
            pendingFollowUp.Set(text);
            return ValueTask.CompletedTask;
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
                // Use the same Chat view for the first answer so it matches follow-up responses
                var firstAnswerMessage = new ChatMessage(ChatSender.Assistant, new Markdown(result.Answer));
                var allMessages = new[] { firstAnswerMessage }.Concat(followUpMessages.Value).ToArray();
                resultsContent = new Chat(allMessages, OnFollowUpSend).Placeholder("Ask a follow-up question…");
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

        var searchInput = inputState.ToSearchInput()
            .Placeholder("Search...")
            .ShortcutKey("ESC")
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
        void ClearResults()
        {
            queryQuestion.Set(_ => (string?)null);
            followUpMessages.Set(Array.Empty<ChatMessage>());
            pendingFollowUp.Set(default(string?));
        }
        var clearButton = new Button("Clear", _ => ClearResults());

        var slots = new List<object>
        {
            new Slot("SearchInput", searchInput),
            new Slot("AskButton", askButton),
            new Slot("ClearInputButton", clearInputButton),
            new Slot("ResultsContent", resultsContent),
            new Slot("ClearButton", clearButton)
        };
        if (resultsHeader != null)
            slots.Insert(3, new Slot("ResultsHeader", resultsHeader));
        return new SmartSearch(slots.ToArray());
    }
}
