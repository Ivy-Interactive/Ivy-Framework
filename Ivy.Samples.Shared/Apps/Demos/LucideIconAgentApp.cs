using Ivy.Shared;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Sparkles)]
public class LucideIconAgentApp() : SampleBase(Align.TopRight)
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        var messages = UseState(ImmutableArray.Create<ChatMessage>(new ChatMessage(ChatSender.Assistant,
            "Hello! I'm the Lucide Icon Agent. I can help you find icons for your app. Please describe your application.")));

        async void OnSendMessage(Event<Chat, string> @event)
        {
            messages.Set(messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value)));
            var currentMessages = messages.Value;
            messages.Set(messages.Value.Add(new ChatMessage(ChatSender.Assistant, new ChatStatus("Thinking..."))));

            var agent = new LucideIconAgent();
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

public class LucideIconAgent
{
    private readonly Kernel _kernel;

    public LucideIconAgent()
    {
        var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        _kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o-2024-11-20", openAiKey)
            .Build();
    }

    public async Task<string?> SuggestIconAsync(string appDescription)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();

        var allIcons = Enum.GetValues<Icons>().Where(e => e != Icons.None);

        history.AddSystemMessage(
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
        );
        history.AddUserMessage(appDescription);

        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            kernel: _kernel);

        return result.Content;
    }
}
