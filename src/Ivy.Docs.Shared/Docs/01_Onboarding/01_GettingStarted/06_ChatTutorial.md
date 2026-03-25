---
prepare: |
    var client = UseService<IClientProvider>();
searchHints:
  - tutorial
  - ai
  - chat
  - openai
  - example
  - ichatclient
  - microsoft.extensions.ai
---

# Chat Tutorial

<Ingress>
Create an AI-powered chat application that suggests [Lucide icons](../../02_Widgets/01_Primitives/02_Icon.md) based on application descriptions using modern chat patterns.
</Ingress>

## Prerequisites

Before starting this tutorial, make sure you have:

1. [Installed](02_Installation.md) Ivy on your development machine
2. An `IChatClient` registered in your services (e.g. via OpenAI):

```csharp
var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
{
    Endpoint = new Uri(endpoint)
});
server.Services.AddSingleton<IChatClient>(
    openAiClient.GetChatClient("gpt-4o").AsIChatClient());
```

## Create the Chat Application

Let's create a new chat application that helps users find appropriate Lucide icons for their applications. We'll create a new file called `LucideIconAgentApp.cs` in your `Apps` folder.

First, let's create the basic structure:

```csharp
[App(icon: Icons.Sparkles)]
public class LucideIconAgentApp() : SampleBase(Align.TopRight)
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();
        var chatClient = UseService<IChatClient?>();

        var messages = UseState(ImmutableArray.Create<ChatMessage>(new ChatMessage(ChatSender.Assistant,
            "Hello! I'm the Lucide Icon Agent. I can help you find icons for your app. Please describe your application.")));

        if (chatClient == null)
        {
            return Callout.Error("IChatClient is not configured. Please register an IChatClient in your service configuration.");
        }

        return new Chat(messages.Value.ToArray(), OnSend);
    }
}
```

## Add Message Handling

Now let's implement the message handling logic. We'll add the `OnSend` method that processes user input and generates icon suggestions:

```csharp
async ValueTask OnSend(Event<Chat, string> @event)
{
    messages.Set(messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value)));
    var currentMessages = messages.Value;
    messages.Set(messages.Value.Add(new ChatMessage(ChatSender.Assistant, new ChatStatus("Thinking..."))));
    
    var agent = new LucideIconAgent(chatClient);
    var suggestion = await agent.SuggestIconAsync(@event.Value);
    if(suggestion != null)
    {
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
}
```

## Create the Icon Agent

The `LucideIconAgent` class handles the AI-powered icon suggestions using `Microsoft.Extensions.AI.IChatClient`. Create this class in the same file:

```csharp
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
```

> **Note:** `Microsoft.Extensions.AI.ChatMessage` is fully qualified above because the Ivy `ChatMessage` type (used by the Chat widget) shares the same name. `ChatRole` comes from `Microsoft.Extensions.AI`.

## How It Works

1. The app starts with a welcome message asking users to describe their application
2. When a user sends a message, it's added to the chat history
3. The `LucideIconAgent` uses `IChatClient` to analyze the description and suggest relevant Lucide icons
4. The suggested icons are displayed as [clickable buttons](../../02_Widgets/03_Common/01_Button.md)
5. Clicking an icon copies its name to the clipboard and shows a [toast notification](../02_Concepts/23_Alerts.md)

## Try It Out

You can now run the project and try it out! Describe your application, and the AI will suggest appropriate Lucide icons that you can use in your project.

<Callout Icon="Info">
Make sure you have registered an `IChatClient` in your services before running the project.
</Callout>

You can find the full source code for the project at [GitHub](https://github.com/Ivy-Interactive/Ivy-Framework/tree/main/Ivy.Samples/Apps/Demos/LucideIconAgentApp.cs).
