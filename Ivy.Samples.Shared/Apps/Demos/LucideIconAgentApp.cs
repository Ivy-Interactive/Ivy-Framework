using Ivy.Shared;
using Microsoft.Extensions.AI;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Sparkles, searchHints: ["ai", "agent", "assistant", "chatbot", "suggestions", "semantic"])]
public class LucideIconAgentApp() : SampleBase(Align.TopRight)
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();
        var chatClient = UseService<IChatClient?>();

        if (chatClient == null)
        {
            return Callout.Error("IChatClient is not configured. Please specify OpenAi:ApiKey and OpenAi:Endpoint in your configuration.");
        }

        var messages = UseState(ImmutableArray.Create<ChatMessage>(new ChatMessage(ChatSender.Assistant,
            "Hello! I'm the Lucide Icon Agent. I can help you find icons for your app. Please describe your application.")));

        async ValueTask OnSendMessage(Event<Chat, string> @event)
        {
            messages.Set(messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value)));
            var currentMessages = messages.Value;
            messages.Set(messages.Value.Add(new ChatMessage(ChatSender.Assistant, new ChatStatus("Thinking..."))));

            var agent = new LucideIconAgent(chatClient);
            var suggestion = await agent.SuggestIconAsync(@event.Value);
            if (suggestion != null)
            {
                //suggestion is a string with ; separated icon names
                var icons = suggestion.Split(';');
                Icons[] iconEnums = icons.Select(icon => Enum.TryParse<Icons>(icon, out var result) ? result : Icons.None)
                    .Where(e => e != Icons.None).ToArray();

                Action<Event<Button>> onIconClick = e =>
                {
                    client.CopyToClipboard(e.Sender.Icon?.ToString() ?? "");
                    client.Toast($"Copied '{e.Sender.Icon?.ToString()}' to clipboard", "Icon Copied");
                };

                var content = Layout.Horizontal().Gap(1)
                    | iconEnums.Select(e => e.ToButton(onIconClick).WithTooltip(e.ToString()));

                messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, content)));
            }
            else
            {
                //todo: handle error
            }
        }

        return Layout.Center().Padding(0, 10, 0, 10)
            | new Chat(messages.Value.ToArray(), OnSendMessage).Width(Size.Full().Max(200));
    }
}

public class LucideIconAgent(IChatClient chatClient)
{
    public async Task<string?> SuggestIconAsync(string appDescription)
    {
        var allIcons = Enum.GetValues<Icons>().Where(e => e != Icons.None);

        var messages = new List<Microsoft.Extensions.AI.ChatMessage>
        {
            new(ChatRole.System,
                $"""
                You are an expert on Lucide React.
                User will submit a description of an application that is being built and you will suggest 7 icons from the Lucide React
                library that are good idiomatic alternatives to recommend.
                Answer with ; separated list of icon names.

                Available icons in the Lucide React library:
                ```
                {string.Join("\n", allIcons.Select(e => e.ToString()).ToArray())}
                ```

                Do not use code blocks or any other markdown formatting. No explanation is needed.
                """
            ),
            new(ChatRole.User, appDescription)
        };

        var result = await chatClient.GetResponseAsync(messages);

        return result.Text;
    }
}
