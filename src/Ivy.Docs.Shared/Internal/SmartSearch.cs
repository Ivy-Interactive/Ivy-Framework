using Ivy.Core.Apps;
using Ivy.Core.AppShell;
using Ivy.Docs.Shared.Services;

namespace Ivy.Docs.Shared.Internal;

/// <summary>
/// Docs-internal widget: smart search (slots for search input, ask button, overlay trigger, etc.).
/// </summary>
public record SmartSearch(params object?[] children) : WidgetBase<SmartSearch>(children.Where(c => c != null).Cast<object>().ToArray())
{
    internal SmartSearch() : this([]) { }
}

/// <summary>
/// Docs-internal view: smart search (overlay + answer sheet).
/// </summary>
public class SmartSearchView : ViewBase
{
    public override object? Build()
    {
        var questionsClient = UseService<IIvyDocsQuestionsClient>();
        var appRepository = UseService<IAppRepository>();
        var navigator = Context.UseNavigation();
        var overlayOpen = UseState(false);
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

                IvyDocsQuestionResult? result = null;
                try
                {
                    result = await questionsClient.AskAsync(q!).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    result = new IvyDocsQuestionResult($"Error: {ex.Message}", null);
                }

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
            overlayOpen.Set(false);
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
            var resultJustArrived = !query.Loading && !query.Validating && queryQuestion.Value != resultForQuestion.Value;
            if (resultJustArrived)
            {
                lastResultRef.Value = query.Value;
                resultForQuestion.Set(queryQuestion.Value);
            }
            var waitingForNewResult = queryQuestion.Value != resultForQuestion.Value;
            var isFetching = query.Loading || query.Validating || waitingForNewResult;
            if (isFetching)
            {
                resultsContent = Layout.Vertical()
                    | new Loading()
                    | Text.P("Finding an answer...")
                    | new Skeleton().Height(Size.Units(80))
                    | new Skeleton().Height(Size.Units(120))
                    | new Skeleton().Height(Size.Units(60));
            }
            else if (query.Error is { } err)
            {
                resultsContent = Layout.Vertical()
                    | Callout.Error(err.Message)
                    | new Button("Retry", _ => query.Mutator.Revalidate()).Variant(ButtonVariant.Outline);
            }
            else if (query.Value is { } result)
            {
                var firstAnswerMessage = new ChatMessage(ChatSender.Assistant, new Markdown(result.Answer));
                var allMessages = new[] { firstAnswerMessage }.Concat(followUpMessages.Value).ToArray();
                resultsContent = new Chat(allMessages, OnFollowUpSend).Placeholder("Ask a follow-up question…");
            }
            else if (!query.Loading && !query.Validating && query.Error is null)
            {
                resultsContent = Layout.Vertical().Gap(4).Center()
                        | Text.H1("No answer found :|").Bold()
                        | Text.Muted("We couldn't find an answer to your question in the Ivy docs. Try rephrasing or browse the documentation.");
            }
        }

        var searchInput = inputState.ToSearchInput()
            .Placeholder("Search...");
        var askButton = new Button("Get an answer from Ivy Agent", SubmitQuestion)
            .Variant(ButtonVariant.Ai)
            .Small()
            .TestId("docs-smart-search-ask");

        var clearInputButton = new Button("", _ => inputState.Set(""));
        var openTrigger = new Button("", _ =>
        {
            inputState.Set("");
            queryQuestion.Set(_ => (string?)null);
            followUpMessages.Set(Array.Empty<ChatMessage>());
            pendingFollowUp.Set(default(string?));
            overlayOpen.Set(true);
        }).TestId("docs-smart-search-open-trigger");

        var windowQuery = inputState.Value?.Trim() ?? "";
        var allMenuItems = appRepository.GetMenuItems();
        var flattened = allMenuItems.FlattenWithPath().ToList();
        var suggestionItems = string.IsNullOrEmpty(windowQuery)
            ? flattened.Take(10).Select(x => x.Item with { Path = string.IsNullOrEmpty(x.Path) ? null : x.Path }).ToList()
            : flattened
                .Select(x => new { x.Item, x.Path, Score = AppShellUtils.ItemMatchScore(x.Item, windowQuery) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Item.Label)
                .Select(x => x.Item with { Path = string.IsNullOrEmpty(x.Path) ? null : x.Path })
                .ToList();

        var suggestionListItems = suggestionItems.Select(item =>
        {
            var tag = item.Tag?.ToString() ?? "";
            return (object)new ListItem(
                title: item.Label,
                subtitle: item.Path,
                icon: item.Icon,
                tag: item.Tag,
                onClick: (Action)(() =>
                {
                    if (!string.IsNullOrEmpty(tag))
                        navigator.Navigate("app://" + tag);
                    overlayOpen.Set(false);
                }));
        }).ToList();

        object overlayListOrPlaceholder = suggestionListItems.Count > 0
            ? Layout.Vertical(suggestionListItems.ToArray()).Gap(0)
            : Text.Muted("Type to search or pick a suggestion above.");

        var overlayContent = Layout.Vertical().Gap(4)
            | overlayListOrPlaceholder;

        var footer = new DialogFooter(askButton);

        var baseSlots = new List<object>
        {
            new Slot("SearchInput", searchInput),
            new Slot("AskButton", askButton),
            new Slot("ClearInputButton", clearInputButton),
            new Slot("OpenTrigger", openTrigger)
        };

        if (queryQuestion.Value == null || resultsContent == null)
        {
            if (!overlayOpen.Value)
                return new SmartSearch(baseSlots.ToArray());
            baseSlots.Add(new Slot(
                "CloseOverlay",
                new Button("", _ => overlayOpen.Set(false))
                    .TestId("docs-smart-search-close-overlay")));
            baseSlots.Add(new Slot(
                "OverlayPanel",
                new DialogHeader("Search"),
                (Layout.Vertical().Padding(4) | searchInput),
                new DialogBody(overlayContent),
                footer));
            return new SmartSearch(baseSlots.ToArray());
        }

        var apiTitle = query.Value is { Title: { } t } && !string.IsNullOrWhiteSpace(t) ? t : null;
        var resultsHeader = apiTitle != null ? Text.H2(apiTitle).Bold() : null;
        void ClearResults()
        {
            queryQuestion.Set(_ => (string?)null);
            followUpMessages.Set(Array.Empty<ChatMessage>());
            pendingFollowUp.Set(default(string?));
            overlayOpen.Set(false);
        }

        var sheetContent = resultsHeader != null
            ? (object)(Layout.Vertical().Gap(4) | resultsHeader | resultsContent)
            : (object)(Layout.Vertical().Gap(4) | resultsContent);

        var answerSheet = new Sheet(
            _ => { ClearResults(); return ValueTask.CompletedTask; },
            sheetContent,
            "Answer",
            null).Width(Size.Fraction(0.4f));

        return new Fragment(
            new SmartSearch(baseSlots.ToArray()),
            answerSheet);
    }
}
