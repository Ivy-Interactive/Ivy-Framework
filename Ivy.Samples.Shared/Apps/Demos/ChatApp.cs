using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.MessageCircle, searchHints: ["chat", "messaging", "conversation", "realtime", "polling"])]
public class ChatApp() : SampleBase(Align.TopRight)
{
    protected override object? BuildSample()
    {
        var username = UseState<string?>();

        if (username.Value is null)
        {
            return new UsernamePrompt(s => username.Set(s));
        }

        return new ChatRoom(username.Value);
    }
}

public class UsernamePrompt(Action<string> onUsernameSet) : ViewBase
{
    public override object? Build()
    {
        var inputValue = UseState("");

        void HandleSubmit()
        {
            var trimmed = inputValue.Value.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                onUsernameSet(trimmed);
            }
        }

        return Layout.Center().Padding(10)
               | (Layout.Vertical().Gap(4).Width(Size.Units(80))
                  | Text.H1("Welcome to Chat")
                  | Text.Muted("Enter your username to join the conversation.")
                  | inputValue.ToTextInput()
                        .Placeholder("Username")
                  | new Button("Join Chat", _ => HandleSubmit())
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.LogIn)
                        .Disabled(string.IsNullOrWhiteSpace(inputValue.Value)));
    }
}

public record ChatMessageData(string Username, string Content, DateTime Timestamp);

public static class ChatRoomStore
{
    private static readonly List<ChatMessageData> _messages = [];
    private static readonly object _lock = new();

    public static IReadOnlyList<ChatMessageData> GetMessages()
    {
        lock (_lock)
        {
            return _messages.ToList();
        }
    }

    public static void AddMessage(string username, string content)
    {
        lock (_lock)
        {
            _messages.Add(new ChatMessageData(username, content, DateTime.Now));

            // Keep only last 100 messages
            if (_messages.Count > 100)
            {
                _messages.RemoveAt(0);
            }
        }
    }
}

public class ChatRoom(string username) : ViewBase
{
    public override object? Build()
    {
        var messagesQuery = UseQuery(
            key: "chat-messages",
            fetcher: ct =>
            {
                var messages = ChatRoomStore.GetMessages();
                return Task.FromResult(messages);
            },
            options: new QueryOptions
            {
                Scope = QueryScope.Server
            });

        var storedMessages = messagesQuery.Value ?? [];

        var chatMessages = storedMessages.Select(msg =>
        {
            var isOwnMessage = msg.Username == username;
            var sender = isOwnMessage ? ChatSender.User : ChatSender.Assistant;

            // For other users, show their username with the message
            object content = isOwnMessage
                ? msg.Content
                : Layout.Vertical().Gap(1)
                  | Text.Bold(msg.Username)
                  | Text.Literal(msg.Content);

            return new ChatMessage(sender, content);
        }).ToArray();

        void OnSendMessage(Event<Chat, string> e)
        {
            var trimmed = e.Value.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                ChatRoomStore.AddMessage(username, trimmed);
                messagesQuery.Mutator.Revalidate();
            }
        }

        return new Chat(chatMessages, OnSendMessage)
            .Placeholder($"Message as {username}...").Height(Size.Full());
    }
}
