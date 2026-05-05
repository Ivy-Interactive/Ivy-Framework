using System.Collections.Immutable;
using System.Text;
using AiChatMessage = Microsoft.Extensions.AI.ChatMessage;
using AiChatRole = Microsoft.Extensions.AI.ChatRole;
using IChatClient = Microsoft.Extensions.AI.IChatClient;

namespace MyProject.Apps;

[App(title: "Chat", icon: Icons.MessageSquare)]
public class ChatApp : ViewBase
{
    public override object? Build()
    {
        var chatClient = UseService<IChatClient>();
        var messages = UseState(ImmutableArray.Create<ChatMessage>());
        var history = UseState(ImmutableArray.Create<AiChatMessage>());
        var isStreaming = UseState(false);

        void OnSend(Event<Chat, string> e)
        {
            var userText = e.Value;

            var updatedMessages = messages.Value
                .Add(new ChatMessage(ChatSender.User, userText))
                .Add(new ChatMessage(ChatSender.Assistant, new ChatStatus("Thinking...")));
            messages.Set(updatedMessages);

            var updatedHistory = history.Value
                .Add(new AiChatMessage(AiChatRole.User, userText));
            history.Set(updatedHistory);
            isStreaming.Set(true);

            _ = Task.Run(async () =>
            {
                try
                {
                    var builder = new StringBuilder();
                    await foreach (var update in chatClient.GetStreamingResponseAsync(updatedHistory.ToArray()))
                    {
                        builder.Append(update.Text);
                        var streamed = messages.Value
                            .RemoveAt(messages.Value.Length - 1)
                            .Add(new ChatMessage(ChatSender.Assistant, builder.ToString()));
                        messages.Set(streamed);
                    }

                    var responseText = builder.ToString();
                    history.Set(history.Value
                        .Add(new AiChatMessage(AiChatRole.Assistant, responseText)));
                }
                catch (Exception ex)
                {
                    var errorMessages = messages.Value
                        .RemoveAt(messages.Value.Length - 1)
                        .Add(new ChatMessage(ChatSender.Assistant, $"Error: {ex.Message}"));
                    messages.Set(errorMessages);
                }
                finally
                {
                    isStreaming.Set(false);
                }
            });
        }

        void OnCancel(Event<Chat> e)
        {
            isStreaming.Set(false);
        }

        return new Chat(messages.Value.ToArray(), OnSend, OnCancel)
            .Placeholder("Ask anything...");
    }
}
