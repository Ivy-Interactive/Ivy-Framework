---
searchHints:
  - messaging
  - conversation
  - chat
  - messages
  - communication
  - agent
  - ai
---

# Chat

The `Chat` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) renders a conversation between a user and an assistant.

Messages are supplied as `ChatMessage` objects and new messages are sent through the `OnSend` event.

## Basic Chat

A simple chat with an echo bot that repeats user messages.

This demonstrates the fundamental usage of the Chat widget with basic message handling and [state management](../../03_Hooks/02_Core/03_UseState.md).

```csharp demo-tabs
public class BasicChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "Hello! I'm an echo bot. I'll repeat whatever you say!")
        ));

        void OnSend(Event<Chat, string> @event)
        {
            var messagesWithUser = messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value));
            messages.Set(messagesWithUser);

            var messagesWithAssistant = messagesWithUser.Add(new ChatMessage(ChatSender.Assistant, $"You said: {@event.Value}"));
            messages.Set(messagesWithAssistant);
        }

        return new Chat(messages.Value.ToArray(), OnSend)
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## AI Assistant with Loading States and Cancel Request

A chat that simulates AI processing with loading indicators and demonstrates how to cancel ongoing requests.

This example shows how to implement async message handling with loading states and request cancellation. When a user sends a message, a loading indicator appears. The user can click the Cancel button at any time to stop the operation.

```csharp demo-tabs
public class LoadingChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "I'm an AI assistant! You can cancel my responses.")
        ));

        var ctsState = UseState<CancellationTokenSource?>(default(CancellationTokenSource?));

        void OnSend(Event<Chat, string> e)
        {
            // Cancel previous request if any
            ctsState.Value?.Cancel();

            var cts = new CancellationTokenSource();
            ctsState.Set(cts);

            // Add user message
            var list = messages.Value.Add(new ChatMessage(ChatSender.User, e.Value));

            // Add assistant message with loading
            var assistantIndex = list.Length;
            list = list.Add(new ChatMessage(ChatSender.Assistant, new ChatLoading()));
            messages.Set(list);

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3000, cts.Token);

                    // Replace loading with response
                    var all = messages.Value.ToList();
                    all[assistantIndex] = new ChatMessage(ChatSender.Assistant,
                        $"Response to: '{e.Value}'");
                    messages.Set(all.ToImmutableArray());
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation
                    var all = messages.Value.ToList();
                    all[assistantIndex] = new ChatMessage(ChatSender.Assistant,
                        new Error("Request Cancelled", "The request was cancelled by the user."));
                    messages.Set(all.ToImmutableArray());
                }
                finally
                {
                    ctsState.Set(default(CancellationTokenSource?));
                }
            });
        }

        void OnCancel(Event<Chat> _)
        {
            ctsState.Value?.Cancel();
        }

        return new Chat(messages.Value.ToArray(), OnSend, OnCancel)
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## Interactive Chat with Streaming Output and Cancel

A chat that demonstrates real-time streaming responses with the ability to cancel streaming at any time.

This example shows how to implement streaming chat responses with proper cancellation support. The user's message is added immediately, followed by a loading indicator. Then, the assistant's response streams in word by word. The Cancel Request button remains visible throughout the streaming process, allowing the user to stop it at any moment. If cancelled, the partial response is preserved.

```csharp demo-tabs
public class StreamingChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "I'm a streaming assistant! You can cancel my responses at any time.")
        ));

        // Tracks whether streaming is active - controls Cancel Request button visibility
        var isStreaming = UseState(false);
        var ctsState = UseState<CancellationTokenSource?>(default(CancellationTokenSource?));

        void OnSend(Event<Chat, string> @event)
        {
            // Cancel previous request if any
            ctsState.Value?.Cancel();

            var cts = new CancellationTokenSource();
            ctsState.Set(cts);

            // Set streaming state to true - this shows the Cancel Request button
            isStreaming.Set(true);

            // Add user message immediately
            var messagesWithUser = messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value));
            messages.Set(messagesWithUser);

            // Add loading indicator
            var assistantMessageIndex = messagesWithUser.Length;
            var messagesWithLoading = messagesWithUser.Add(new ChatMessage(ChatSender.Assistant, new ChatLoading()));
            messages.Set(messagesWithLoading);

            // Start streaming in background
            _ = Task.Run(async () =>
            {
                var hasStartedStreaming = false;
                try
                {
                    await Task.Delay(3000, cts.Token);

                    var words = new[] { "I'm", "processing", "your", "message:", $"'{@event.Value}'.",
                        "This", "is", "a", "streaming", "response", "that", "appears", "word", "by", "word." };

                    var collectedWords = new List<string>();
                    foreach (var word in words)
                    {
                        collectedWords.Add(word);
                        var text = string.Join(" ", collectedWords);

                        // Update message with accumulated text
                        var all = messages.Value.ToList();
                        all[assistantMessageIndex] = new ChatMessage(ChatSender.Assistant, text);
                        messages.Set(all.ToImmutableArray());

                        hasStartedStreaming = true;
                        await Task.Delay(300, cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    var all = messages.Value.ToList();
                    if (assistantMessageIndex < all.Count)
                    {
                        if (!hasStartedStreaming)
                        {
                            all[assistantMessageIndex] = new ChatMessage(ChatSender.Assistant,
                            new Error("Cancelled", "Response was cancelled."));
                            messages.Set(all.ToImmutableArray());
                        }
                        // Otherwise preserve partial streamed text
                        messages.Set(all.ToImmutableArray());
                    }
                }
                finally
                {
                    // Clear streaming state - hides Cancel Request button
                    isStreaming.Set(false);
                    ctsState.Set(default(CancellationTokenSource?));
                }
            });
        }

        void OnCancel(Event<Chat> _)
        {
            ctsState.Value?.Cancel();
        }

        return new Chat(messages.Value.ToArray(), OnSend, OnCancel)
            .Streaming(isStreaming.Value)  // Pass streaming state to control Cancel button
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## Interactive Chat with Rich Content

A chat that responds with interactive elements like [buttons](../03_Common/01_Button.md) and [cards](../03_Common/04_Card.md).

This demonstrates how to return complex UI components as chat responses, creating dynamic and engaging conversations with rich media content.

```csharp demo-tabs
public class InteractiveChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "I can show interactive elements! Try sending 'buttons', 'card', or 'form' to see different responses.")
        ));

        void OnSend(Event<Chat, string> @event)
        {
            var messagesWithUser = messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value));
            messages.Set(messagesWithUser);

            object response = @event.Value.ToLower() switch
            {
                "buttons" => Layout.Horizontal().Gap(1)
                    | new Button("Primary").Variant(ButtonVariant.Primary)
                    | new Button("Secondary").Variant(ButtonVariant.Secondary)
                    | new Button("Outline").Variant(ButtonVariant.Outline),

                "card" => new Card("Interactive Card", new Button("Action")),

                "form" => Layout.Vertical().Gap(1)
                    | new TextInput().Placeholder("Enter your name")
                    | new TextInput().Placeholder("Enter your email")
                    | new Button("Submit").Variant(ButtonVariant.Primary),

                _ => $"You said: '{@event.Value}'. Try sending 'buttons', 'card', or 'form' for interactive responses!"
            };

            messages.Set(messagesWithUser.Add(new ChatMessage(ChatSender.Assistant, response)));
        }

        return new Chat(messages.Value.ToArray(), OnSend)
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## Error Handling Chat

A chat that demonstrates error handling and different message types.

This example shows how to use the [Error](../01_Primitives/13_Error.md) widget for different message severities and how to integrate ChatStatus for loading indicators within chat conversations.

```csharp demo-tabs
public class ErrorHandlingChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "I demonstrate error handling! Try sending 'error', 'warning', or 'success' to see different message types.")
        ));

        void OnSend(Event<Chat, string> @event)
        {
            var messagesWithUser = messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value));
            messages.Set(messagesWithUser);

            object response = @event.Value.ToLower() switch
            {
                "error" => new Error("Something went wrong!", "This is an error message! Something went wrong."),

                "warning" => new Error("Be careful!", "This is a warning message! Be careful."),

                "success" => new Error("Great job!", "This is a success message! Everything worked."),

                "loading" => new ChatStatus("Processing your request..."),

                _ => $"You said: '{@event.Value}'. Try sending 'error', 'warning', 'success', or 'loading'!"
            };

            messages.Set(messagesWithUser.Add(new ChatMessage(ChatSender.Assistant, response)));
        }

        return new Chat(messages.Value.ToArray(), OnSend)
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## Advanced Chat with Commands

A sophisticated chat that responds to specific commands with different content types.

This example showcasing the full range of Ivy [widgets](../../01_Onboarding/02_Concepts/03_Widgets.md) that can be embedded in chat responses.

```csharp demo-tabs
public class AdvancedChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant,
                "Welcome to the Advanced Chat! Try these commands:\n" +
                "• 'analyze code' - I'll show code analysis\n" +
                "• 'create form' - I'll show an interactive form\n" +
                "• 'show chart' - I'll display a chart\n" +
                "• 'table data' - I'll show tabular data\n" +
                "• Any other message - I'll respond normally")
        ));

        void OnSend(Event<Chat, string> @event)
        {
            var messagesWithUser = messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value));
            messages.Set(messagesWithUser);

            object response = @event.Value.ToLower() switch
            {
                "analyze code" => new Code("""
                    public class Example
                    {
                        public string ProcessData(string input)
                        {
                            if (string.IsNullOrEmpty(input))
                                return "Empty input";

                            return input.ToUpper();
                        }
                    }
                    """, Languages.Csharp),

                "create form" => Layout.Vertical().Gap(1)
                    | new TextInput().Placeholder("Enter project name")
                    | new TextInput().Placeholder("Enter description")
                    | new SelectInput<string>(new[] { new Option<string>("Web", "Web"), new Option<string>("Mobile", "Mobile"), new Option<string>("Desktop", "Desktop") })
                    | new SelectInput<string>(new[] { new Option<string>("Low", "Low"), new Option<string>("Medium", "Medium"), new Option<string>("High", "High") })
                    | new Button("Create Project").Variant(ButtonVariant.Primary),

                "show chart" => new LineChart(
                    new[] {
                        new { Month = "Jan", Value = 10 },
                        new { Month = "Feb", Value = 20 },
                        new { Month = "Mar", Value = 15 },
                        new { Month = "Apr", Value = 25 },
                        new { Month = "May", Value = 30 },
                        new { Month = "Jun", Value = 35 },
                        new { Month = "Jul", Value = 40 }
                    },
                    "Value",
                    "Month"
                ).Height(Size.Units(50))
                 .Width(Size.Units(80)),

                "table data" => new Table(
                    new TableRow(new TableCell("Name"), new TableCell("Age"), new TableCell("Role"), new TableCell("Department")).IsHeader(),
                    new TableRow(new TableCell("John Doe"), new TableCell("30"), new TableCell("Developer"), new TableCell("Engineering")),
                    new TableRow(new TableCell("Jane Smith"), new TableCell("25"), new TableCell("Designer"), new TableCell("Design")),
                    new TableRow(new TableCell("Bob Johnson"), new TableCell("35"), new TableCell("Manager"), new TableCell("Product")),
                    new TableRow(new TableCell("Alice Williams"), new TableCell("28"), new TableCell("Developer"), new TableCell("Engineering")),
                    new TableRow(new TableCell("Charlie Brown"), new TableCell("32"), new TableCell("QA Engineer"), new TableCell("Quality Assurance"))
                ).Width(Size.Units(100)),

                _ => $"You said: '{@event.Value}'. Try the commands: 'analyze code', 'create form', 'show chart', or 'table data'!"
            };

            messages.Set(messagesWithUser.Add(new ChatMessage(ChatSender.Assistant, response)));
        }

        return new Chat(messages.Value.ToArray(), OnSend)
            .Placeholder("Type your message here...")
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

<WidgetDocs Type="Ivy.Chat" ExtensionTypes="Ivy.ChatExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Chat.cs"/>
